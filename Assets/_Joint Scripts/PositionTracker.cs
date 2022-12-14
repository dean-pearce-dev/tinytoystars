using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: PositionTracker
*
* Author: Dean Pearce
*
* Description: Class for tracking positions of vehicles
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 26/07/2021    DP          1.00        -Initial Created
* ??/07/2021    CT          1.10        -Added functionality to send final position to 
*                                       end screen
**************************************************************************************/
public class PositionTracker : MonoBehaviour
{
    private GameObject m_vehicleHolder;
    private GameObject[] m_vehicleArray;
    private CarBehaviour[] m_carObjectArray;
    private int[] m_vehiclesAheadOfCar;
    private Text m_lapHolder;
    private string[] m_positionArray;
    /*Debug*///private Text m_posTextHolder;
    /*Debug*///private Text m_distanceDebug;
    /*Debug*///private string m_positionHudString;

    private LevelManager m_levelManager;
    private CarBehaviour m_playerScript;

    private Image m_positionImage;

    private GameObject m_uiHolder;

    //Serialized fields for the sprites themselves
    [Header("Positions Images")]
    [SerializeField]
    private Sprite m_1stPlace;
    [SerializeField]
    private Sprite m_2ndPlace;
    [SerializeField]
    private Sprite m_3rdPlace;
    [SerializeField]
    private Sprite m_4thPlace;
    [SerializeField]
    private Sprite m_5thPlace;
    [SerializeField]
    private Sprite m_6thPlace;
    
    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Charlie Taylor & Dean Pearce
    **************************************************************************************/
    private void Start()
    {
        m_levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        string _playerName = m_levelManager.GetPlayerName();
        //Empty holding the vehicles
        m_vehicleHolder = GameObject.Find("Vehicles");
        //Make an array
        m_vehicleArray = new GameObject[m_vehicleHolder.transform.childCount];
        //Array of the car behaviours
        m_carObjectArray = new CarBehaviour[m_vehicleHolder.transform.childCount];

        //UI that holds the posiitons
        /*Debug*///m_posTextHolder = GameObject.Find("Positions").GetComponent<Text>();
        //UI that holds the Current lap (For the player)
        m_lapHolder = GameObject.Find("LapInfo").GetComponent<Text>();
        
        //m_distanceDebug = GameObject.Find("DistanceDebug").GetComponent<Text>();

        m_vehiclesAheadOfCar = new int[m_vehicleHolder.transform.childCount];
        m_positionArray = new string[m_vehicleHolder.transform.childCount];

        for (int i = 0; i < m_vehicleArray.Length; i++)
        {
            GameObject _currentRacer =  m_vehicleArray[i] = m_vehicleHolder.transform.GetChild(i).gameObject;
            CarBehaviour _racerScript = m_carObjectArray[i] = m_vehicleArray[i].GetComponent<CarBehaviour>();

            //This shouuld be _racerScript.GetName(), but for some reason, it returns null. This works fine, but the object MUST be named the same as the name options
            string _thisRacerName = _currentRacer.name;

            if (_thisRacerName == _playerName)
			{
                m_playerScript = _racerScript;
			}

            m_positionArray[i] = _thisRacerName;
            m_vehiclesAheadOfCar[i] = i;
        }

        //Get Images in UI
        m_positionImage = GameObject.Find("Position Image").GetComponent<Image>();

        //Disabling UI for the fly by sequence, gets re-enabled in LevelManager Update after
        //Had to put here for execution order
        m_uiHolder = GameObject.Find("UI");
        m_uiHolder.SetActive(false);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    *
    * Author: Dean Pearce
    **************************************************************************************/
    void Update()
    {
        if (m_vehicleHolder.transform.childCount != m_vehicleArray.Length)
        {
            Start();
        }
        UpdatePositions();
        PreventDuplicate();
        SetPosNames();
        UpdatePositionString();
        PlayerLapUpdate();
        //m_distanceDebug.text = "Distance to target: " + m_carObjectArray[1].DistanceToTarget(m_carObjectArray[0]);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayerLapUpdate
    *
    * Author: Dean Pearce
    *
    * Description: Updates the lap total for displaying to the player
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 26/02/2021    DP          1.00        -Initial Created
    * 05/08/2021    CT          2.00        -Made the function a return, so it can be checked
    *                                       in the Level Manager
    **************************************************************************************/
    public int PlayerLapUpdate()
    {
        int _playerLap = m_playerScript.GetLap();
        m_lapHolder.text = _playerLap.ToString();

        return _playerLap;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UpdatePositionString
    *
    * Author: Dean Pearce
    *
    * Description: Updates the positions of vehicles for displaying to the player
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 26/02/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void UpdatePositionString()
    {
        /*Debug*///m_positionHudString = "Position: \n 1. " + m_positionArray[0] + "\n 2. " + m_positionArray[1] + "\n 3. " + m_positionArray[2] + "\n 4. " + m_positionArray[3] + "\n 5. " + m_positionArray[4] + "\n 6. " + m_positionArray[5];
        /*Debug*///m_posTextHolder.text = m_positionHudString;

        Sprite _imageToSetPosTo;
        //+1 so as to be 1st to 6th, not 0th to 5th
        switch (m_playerScript.m_currentPositionInRace+1)
        {
            default:
                _imageToSetPosTo = m_6thPlace;
                break;

            case 1:
                _imageToSetPosTo = m_1stPlace;
                break;
            case 2:
                _imageToSetPosTo = m_2ndPlace;
                break;
            case 3:
                _imageToSetPosTo = m_3rdPlace;
                break;
            case 4:
                _imageToSetPosTo = m_4thPlace;
                break;
            case 5:
                _imageToSetPosTo = m_5thPlace;
                break;
            case 6:
                _imageToSetPosTo = m_6thPlace;
                break;
        }
        m_positionImage.sprite = _imageToSetPosTo;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UpdatePositions
    *
    * Author: Dean Pearce
    *
    * Description: Figures out the current position ranking of each vehicle
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 26/02/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void UpdatePositions()
    {
        for (int i = 0; i < m_carObjectArray.Length; i++)
        {
            m_vehiclesAheadOfCar[i] = 0;

            for (int j = 0; j < m_carObjectArray.Length; j++)
            {
                //Making sure vehicles being compared aren't the same
                if (i != j)
                {
                    //Same lap
                    if (m_carObjectArray[i].GetLap() == m_carObjectArray[j].GetLap())
                    {
                        //Same route
                        if (m_carObjectArray[i].GetRouteNum() == m_carObjectArray[j].GetRouteNum())
                        {
                            //Interpolation less than vehicle being compared means they're ahead, so add 1 to vehicles ahead
                            if (m_carObjectArray[i].GetCurrentInterpolation() < m_carObjectArray[j].GetCurrentInterpolation())
                                m_vehiclesAheadOfCar[i]++;
                        }
                        //Vehicle being compared is a route or more ahead, add 1 to vehicles ahead
                        else if (m_carObjectArray[i].GetRouteNum() < m_carObjectArray[j].GetRouteNum())
                            m_vehiclesAheadOfCar[i]++;
                    }
                    //Vehicle being compared is a lap or more ahead, add 1 to vehicles ahead
                    else if (m_carObjectArray[i].GetLap() < m_carObjectArray[j].GetLap())
                        m_vehiclesAheadOfCar[i]++;
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PreventDuplicate
    *
    * Author: Dean Pearce
    *
    * Description: Stops vehicles being assigned to the same position rank
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/02/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void PreventDuplicate()
    {
        for (int i = 0; i < m_vehiclesAheadOfCar.Length; i++)
        {
            for (int j = 0; j < m_vehiclesAheadOfCar.Length; j++)
            {
                //Making sure vehicles being compared aren't the same
                if (i != j)
                {
                    //If two vehicles have the same value, the latter has a vehicle ahead added
                    //Prevents duplicates in terms of position ranking
                    //Should only be necessary if two vehicles are at the exact same position
                    if (m_vehiclesAheadOfCar[i] == m_vehiclesAheadOfCar[j])
                        m_vehiclesAheadOfCar[j]++;
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetPosNames
    *
    * Author: Dean Pearce
    *
    * Description: Separate function for setting the names to the string array. Separated due to race condition.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/02/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void SetPosNames()
    {
        for (int i = 0; i < m_carObjectArray.Length; i++)
        {
            m_positionArray[m_vehiclesAheadOfCar[i]] = m_carObjectArray[i].GetName();
            m_carObjectArray[i].SetRacePos(m_vehiclesAheadOfCar[i]);
        }
    }

}
