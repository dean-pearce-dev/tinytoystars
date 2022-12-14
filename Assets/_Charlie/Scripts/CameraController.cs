using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: CameraController
*
* Author: Charlie Taylor
*
* Description: Class that will control unique functions of the camera object
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 30/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class CameraController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The LOCAL offset position of the camera to the player (Debug camera follower)")]
    private Vector3 m_positionOffset = new Vector3(0f, 6.5f, -13f);
    [SerializeField]
    [Tooltip("The LOCAL offset rotation of the camera to the player (Debug camera follower)")]
    private Vector3 m_rotationOffset = new Vector3(16f, 0f, 0f);

    /**************************************************************************************
    * Type: Class
    * 
    * Name: AssignTargetRacer
    *
    * Author: Charlie Taylor
    *
    * Description: Changes the focus of the camera to look at a different racer. Either for
    *              debugging or player selection at the start of the game
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 30/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public void AssignTargetRacer(GameObject p_targetRacer)
	{
        //Assign Parent which helps with rotation and moving the camera
        transform.parent = p_targetRacer.transform;
        //Assign postion offset
        transform.localPosition = m_positionOffset;
        //Assign Rotation Offset
        
        Vector3 _targetRotation = p_targetRacer.transform.rotation.eulerAngles + m_rotationOffset;
        transform.rotation = Quaternion.Euler(_targetRotation);

    }
}
