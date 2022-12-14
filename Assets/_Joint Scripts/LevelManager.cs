using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: LevelManager
* 
* Author: Charlie Taylor
* 
* Description: For track levels, sets up the player and AI racers and sets the camera
*              to look at the player
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 30/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class LevelManager : MonoBehaviour
{
    //Array of all the cars
    private GameObject[] m_racers;

    //Refernce to camera manager
    private CameraController m_camera;

    private GameObject m_player;

    private string m_playerName;

    private PositionTracker m_positionTracker;

    private GameController m_gameController;

    //Race Start class
    private RaceStartController m_raceController;

    //Countdown Variables
    private Text m_starterCountdown;
    private float m_startTime;
    private bool m_countdownHasStarted = false;

    private GameObject m_endScreen;

    private GameObject m_uiHolder;
    
    [SerializeField]
    [Tooltip("Icon for the Star Power up, to show in the icon holder in UI")]
    private Sprite m_starIcon;
    [SerializeField]
    [Tooltip("Icon for the Marble Power up, to show in the icon holder in UI")]
    private Sprite m_marbleIcon;

    private Image m_powerUpIcon;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Charlie Taylor & Dean Pearce
    **************************************************************************************/
    void Start()
    {
        m_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();

        m_positionTracker = GameObject.Find("LapManager").GetComponent<PositionTracker>();

        GameObject _gameControllerObject = GameObject.Find("GameController");
        m_gameController = _gameControllerObject.GetComponent<GameController>();

        CarSelectManager _carSelectManager = m_gameController.GetComponent<CarSelectManager>();

        m_playerName = _carSelectManager.GetPlayer();
         
        Destroy(_carSelectManager);

        m_racers = GameObject.FindGameObjectsWithTag("Car");

        m_raceController = GameObject.Find("RaceStartController").GetComponent<RaceStartController>();

        m_starterCountdown = GameObject.Find("StartCountdown").GetComponent<Text>();
        m_starterCountdown.gameObject.SetActive(false);

        m_endScreen = GameObject.Find("UI").transform.GetChild(GameObject.Find("UI").transform.childCount - 1).gameObject;
        m_gameController.SetEndScreen(m_endScreen);

        m_uiHolder = GameObject.Find("UI");

        for (int i = 0; i < m_racers.Length; i++)
        {
            GameObject _thisRacer = m_racers[i];

            RacerInformation _thisRacersScript = _thisRacer.GetComponent<RacerInformation>();


            if (_thisRacersScript.GetRacerName() == m_playerName)
            {
                _thisRacer.AddComponent<PlayerBehaviour>();

                //Assign camera to look at this racer
                m_camera.AssignTargetRacer(_thisRacer);

                m_player = _thisRacer;
            }
            else
            {
                _thisRacer.AddComponent<EnemyBehaviour>();


            }

            m_powerUpIcon = GameObject.Find("Powerup Icon").GetComponent<Image>();

            //Set the stats in the car behaviour script
            //(Created when adding the player/enemy behaviour component)
            //Remove Racer Information script at end of SetStats()

        }
    }

    /**************************************************************************************
    * Name: GetPlayer
    * 
    * Author: Charlie Taylor
    * 
    * Description: Gets player object for the AI racers
    **************************************************************************************/
    public GameObject GetPlayer()
	{
        return m_player;
	}

    /**************************************************************************************
    * Name: GetPlayerName
    * 
    * Author: Charlie Taylor
    * 
    * Description: Gets player name for the PositionTracker
    **************************************************************************************/
    public string GetPlayerName()
	{
        return m_playerName;
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: UpdatePowerUpIcon
    * 
    * Author: Charlie Taylor
    * 
    * Description: Update the power up icon based on the selected power up
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 12/08/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public void UpdatePowerUpIcon(string p_powerUp)
	{
		switch (p_powerUp)
        {
            case "Star":
                m_powerUpIcon.enabled = true;
                m_powerUpIcon.sprite = m_starIcon;
                break;

            case "Projectile":
                m_powerUpIcon.enabled = true;
                m_powerUpIcon.sprite = m_marbleIcon;
                break;

            case "Empty":
                m_powerUpIcon.enabled = false;
                break;
        }


	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    *
    * Author: Charlie Taylor & Dean Pearce
    **************************************************************************************/
    private void Update()
	{
        //Checks to run the countdown, then disable the countdown ui element once finished
        if (m_raceController.GetFlyByStatus() && !m_raceController.GetCountdownStatus())
        {
            CountdownUpdate();
            m_uiHolder.SetActive(true);
        }
        if ((Time.time - m_startTime) > 4f && m_starterCountdown.gameObject.activeSelf)
            m_starterCountdown.gameObject.SetActive(false);

        int _lap = m_positionTracker.PlayerLapUpdate();

        if (_lap > 3 && !m_gameController.GetRaceStatus())
		{
            m_gameController.CompleteLevel();
		}
        UpdatePowerUpIcon(m_player.GetComponent<PlayerBehaviour>().m_powerUp);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Need to re add a car select manager onto the game controller, as it is removed at the start of the race, but reapplied at the end, incase you want to replay,
            //but due to not reaching the end of the race, re add it here
            m_gameController.gameObject.AddComponent<CarSelectManager>();
            m_gameController.GoBackToMenu();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CountdownUpdate
    *
    * Author: Dean Pearce
    *
    * Description: Function for counting down to the start of race
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void CountdownUpdate()
    {
        if (!m_countdownHasStarted)
        {
            m_startTime = Time.time;
            m_countdownHasStarted = true;
            m_starterCountdown.gameObject.SetActive(true);
        }
        float _timeElapsed = Time.time - m_startTime;

        if (_timeElapsed < 1f)
            m_starterCountdown.text = "3";
        if (_timeElapsed >= 1f)
            m_starterCountdown.text = "2";
        if (_timeElapsed >= 2f)
            m_starterCountdown.text = "1";
        if (_timeElapsed >= 3f)
        {
            m_starterCountdown.text = "Go!";
            m_raceController.SetCountdownStatus(true);
        }
    }
}
