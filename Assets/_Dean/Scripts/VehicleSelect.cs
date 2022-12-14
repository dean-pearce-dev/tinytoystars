using UnityEngine;

/**************************************************************************************
* Type: Class
* Name: VehicleSelect
*
* Author: Dean Pearce
*
* Description: Class for the menu scene, which takes input for switching vehicle, and rotates the currently selected vehicle
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* ??/07/2021    DP          1.00        -Initial Created
**************************************************************************************/
public class VehicleSelect : MonoBehaviour
{
    [SerializeField]
    private float m_rotateSpeed = 80f;
    [SerializeField]
    private float m_startRotation = -135f;
    private GameObject m_selectedObject;
    private GameObject[] m_vehicles;
    private GameObject m_vehicleHolder;
    private int m_selectedNum = 0;
    private string m_prefabPathString;
    private CarSelectManager m_carSelectManager;

	/**************************************************************************************
	* Type: Function
	* 
	* Name: Start
	*
	* Author: Dean Pearce
	**************************************************************************************/
	void Start()
    {
        m_vehicleHolder = GameObject.Find("VehicleHolder");
        m_vehicles = new GameObject[m_vehicleHolder.transform.childCount];
        for (int i = 0; i < m_vehicles.Length; i++)
        {
            m_vehicles[i] = m_vehicleHolder.transform.GetChild(i).gameObject;
            m_vehicles[i].SetActive(false);
        }
        m_selectedObject = m_vehicles[m_selectedNum];
        m_selectedObject.SetActive(true);
        m_selectedObject.transform.rotation = Quaternion.Euler(0, m_startRotation, 0);
        m_carSelectManager = GameObject.Find("GameController").GetComponent<CarSelectManager>();
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
        m_selectedObject.transform.Rotate(0, m_rotateSpeed * Time.deltaTime, 0);
        ChangeVehicleSelection();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ConfirmSelection
    *
    * Author: Dean Pearce
    *
    * Description: Confirms the selected car, then loads the target location
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * ??/07/2021    DT          1.00        -Initial Created
    * 04/08/2021    CT          2.00        -Overhauled function to set target car based
    *                                       on name only and pass that value into a manager
    *                                       exclusive to tracks
    **************************************************************************************/
    public void ConfirmSelection()
    {
        m_carSelectManager.SetPlayer(m_selectedObject.name);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ChangeVehicleSelection
    *
    * Author: Dean Pearce
    *
    * Description: Takes user input to change selected vehicle, enables the selection, and disables the rest. Resets rotation.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * ??/07/2021    DT          1.00        -Initial Created
    **************************************************************************************/
    private void ChangeVehicleSelection()
    {
        bool _hasChanged = false;
        if (Input.GetKeyDown("d") && m_selectedNum < m_vehicles.Length - 1)
        {
            m_selectedNum++;
            _hasChanged = true;
        }
        if (Input.GetKeyDown("a") && m_selectedNum > 0)
        {
            m_selectedNum--;
            _hasChanged = true;
        }

        m_selectedObject = m_vehicles[m_selectedNum];

        for (int i = 0; i < m_vehicles.Length; i++)
        {
            m_vehicles[i].SetActive(false);
        }
        m_selectedObject.SetActive(true);

        if (_hasChanged)
            m_selectedObject.transform.rotation = Quaternion.Euler(0, m_startRotation, 0);
        m_carSelectManager.SetPlayer(m_selectedObject.name);
    }
}
