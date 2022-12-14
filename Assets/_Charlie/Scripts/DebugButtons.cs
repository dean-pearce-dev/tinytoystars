using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Function
* 
* Name: DebugButtons
*
* Author: Charlie Taylor
*
* Description: A script attached to debug buttons that 
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 04/08/2021    CT          1.00        -Initial Created
**************************************************************************************/

public class DebugButtons : MonoBehaviour
{
    //Enum allows the programmer to select which car this button will look at from a dropdown menu rather than typing in the car name, possibly causing issues
    private enum CarNames
    {
        F1,
        Firetruck,
        Train,
        URO,
        Wasp,
        Tank
    }

    [SerializeField]
    private CarNames m_carName;

    DebugManager m_gameManager;
    Button m_button;

    void Start()
    {
        m_button = GetComponent<Button>();
        m_gameManager = GameObject.Find("DebugManager").GetComponent<DebugManager>();
        m_button.onClick.AddListener(ButtonAction);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ButtonAction
    *
    * Author: Charlie Taylor
    *
    * Description: To let the debug buttons use the dropdown parameters, the button OnClick now 
    *              just calls this function which calls ChangeCameraFocus in the debug manager
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 04/08/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    private void ButtonAction()
    {
        m_gameManager.ChangeCameraFocus(m_carName.ToString());
    }
}
