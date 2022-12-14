using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**************************************************************************************
* Type: Class
* 
* Name: DebugManager
* 
* Author: Charlie Taylor
* 
* Description: A class that contains some basic debug functionality
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 29/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class DebugManager : MonoBehaviour
{
    //Array of all car debug text objects
    private GameObject[] m_carDebugText;

    //For the cars themselves
    private GameObject[] m_racers;

    private CameraController m_cameraController;

    /**************************************************************************************
    * Type: Functions
    * 
    * Name: Start
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private void Start()
	{
        //has to be done in start. Cannot be done in every call.
        //If wanting to reuse, may need a seperate function and call it here
        m_carDebugText = GameObject.FindGameObjectsWithTag("DebugText");

        m_racers = GameObject.FindGameObjectsWithTag("Car");

        m_cameraController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
    }

	/**************************************************************************************
    * Type: Function
    * 
    * Name: DebugTextToggle
    * 
    * Author: Charlie Taylor
    * 
    * Description: Called by pressing the debug text button in game, this function finds
    *              all the debug text objects, and toggles them on or off depending on
    *              the previous state
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 29/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public void DebugTextToggle()
    {

        for (int i = 0; i < m_carDebugText.Length; i++)
        {
            //get the current text object
            GameObject _thisText = m_carDebugText[i];
            //Toggle it
            _thisText.SetActive(!_thisText.activeInHierarchy);
        }
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: ChangeCameraFocus
    * 
    * Author: Charlie Taylor
    * 
    * Description: Changes the camera to look at different player, but not control them.
    *              Used for debugging only, but may be copied in some way for the
    *              implementation of different players.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 29/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public void ChangeCameraFocus(string p_targetRacer)
	{
        string _racerName;

        for (int i = 0; i < m_racers.Length; i++)
        {
            //get the current car object
            GameObject _thisRacer = m_racers[i];

            _racerName = _thisRacer.GetComponent<CarBehaviour>().m_objName;

            if (_racerName == p_targetRacer)
			{
                m_cameraController.AssignTargetRacer(_thisRacer);
                //Change Camera Target, but for now
             /*   Debug.Log("Target Racer: " + p_targetRacer +
                          " | Found Name: " + _racerName);*/
			}
        }
    }
}
