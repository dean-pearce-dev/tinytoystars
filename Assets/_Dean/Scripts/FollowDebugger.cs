using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* Name: FollowDebugger
*
* Author: Dean Pearce
*
* Description: Class for showing the position of each vehicle's look target for the purpose
*              of fine-tuning/debugging
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 12/08/2021    DP          1.00        -Initial Created
**************************************************************************************/
public class FollowDebugger : MonoBehaviour
{
    private RaceStartController m_raceController;
    private GameObject m_followDebugHolder;
    private GameObject[] m_followDebugArray;
    private GameObject m_vehicleHolder;
    private CarBehaviour[] m_vehicleArray;
    private bool m_vehiclesSetup = false;

    /**************************************************************************************
	* Type: Function
	* 
	* Name: Start
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void Start()
    {
        m_raceController = GameObject.Find("RaceStartController").GetComponent<RaceStartController>();
        m_followDebugHolder = gameObject;
        m_followDebugArray = new GameObject[m_followDebugHolder.transform.childCount];
        m_vehicleHolder = GameObject.Find("Vehicles");
        m_vehicleArray = new CarBehaviour[m_vehicleHolder.transform.childCount];

        for (int i = 0; i < m_followDebugArray.Length; i++)
        {
            m_followDebugArray[i] = m_followDebugHolder.transform.GetChild(i).gameObject;
        }
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
        if (!m_raceController.GetFlyByStatus())
            return;
        if (!m_vehiclesSetup)
        {
            for (int i = 0; i < m_vehicleArray.Length; i++)
            {
                m_vehicleArray[i] = m_vehicleHolder.transform.GetChild(i).GetComponent<CarBehaviour>();
            }
            m_vehiclesSetup = true;
        }

        for (int i = 0; i < m_followDebugArray.Length; i++)
        {
            m_followDebugArray[i].transform.position = m_vehicleArray[i].GetLookTargetPos();
        }
    }
}
