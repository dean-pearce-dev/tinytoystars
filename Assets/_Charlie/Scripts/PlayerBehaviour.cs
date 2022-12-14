using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**************************************************************************************
* Type: Class
* 
* Name: PlayerMovement
*
* Author: Charlie Taylor
*
* Description: Class used to control the player. Child of the CarMovement Class
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 04/07/2021    CT          1.00        -Initial Created
* 12/07/2021    CT          1.10        -Added Object Pooling basics
* 02/08/2021    CT          1.20        -Moved object pooling to parent object, which
*                                       made Start redundant, so removed that
**************************************************************************************/
public class PlayerBehaviour : CarBehaviour
{

    //Bool for checking if the car has finished the race
    private bool m_finishedRace = false;

    [HideInInspector]
    public string m_powerUp = "Empty";
    [HideInInspector]
    public bool m_playerCanUsePower;

    private AudioSource m_horn;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private new void Start()
	{
        base.Start();

        m_horn = transform.Find("Horn").GetComponent<AudioSource>();
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private new void Update()
    {
        //Do at start so you can't do anything until race starts
        base.Update();

        m_powerUp = m_powerItem;
        m_playerCanUsePower = m_canUsePower;
        PlayerInput();

        if (GetLap() > 3 && !m_finishedRace)
        {
            m_finishedRace = true;
            m_gameController.SetFinalPosition(m_currentPositionInRace);
            m_gameController.SetPreviousLevel(SceneManager.GetActiveScene().name);
        }

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: PlayerInput
    *
    * Author: Dean Pearce
    *
    * Description: Function to keep the players inputs clean and organised
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    CT          1.01        -Added functionality to fire projectiles
    * 28/07/2021    CT          1.10        -Changed checks for left and right keys to
    *                                       check if the player is already changing lanes.
    *                                       Also made it so the speed is not changed in
    *                                       here, but in MaintainSpeed in CarBehaviour
    *                                       Removed Deceleration by pressing S
    * 02/08/2021    CT          1.30        -Moved the firing of projectile functionality 
    *                                       into a function of its own in the parent.
    *                                       Also made pressing E call different power ups
    * 04/08/2021    DP          1.40        -Added conditionals to lane change input to check
    *                                       if a lane has a NoChange tag attached
    **************************************************************************************/
    private void PlayerInput()
    {
        if (!m_isSpinning)
        {
            m_laneSwitchSpeedScale = (m_currentSpeed / m_defaultSpeed) % 2;
            if (m_laneSwitchSpeedScale < 1f)
                m_laneSwitchSpeedScale = 1;

            //NoChange conditionals intended for shortcut functionality
            if (Input.GetKeyDown("a") && m_currentLane > 0 && !m_isChangingLane &&
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag != "NoCurveNoChangeL" &&
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag != "NoChangeL")
            {
                LaneSwitchCalculation(m_currentLane - 1);
            }
            if (Input.GetKeyDown("d") && m_currentLane < 4 && !m_isChangingLane &&
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag != "NoCurveNoChangeR" &&
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag != "NoChangeR")
            {
                LaneSwitchCalculation(m_currentLane + 1);
            }
            //Accelerate
            if (Input.GetKey("w"))
            {
                m_accelertating = true;
            }
            else
			{
                m_accelertating = false;
			}

            if (Input.GetKeyDown("e"))
            {
                if (m_canUsePower)
                {
                    m_canUsePower = false;
                    switch (m_powerItem)
                    {
                        case "Projectile":
                            FireProjectile();
                            break;

                        case "Star":
                            SetStarMode();
                            break;

                    }
                    m_powerItem = "Empty";
                }
            }

            //Horn
            if (Input.GetKeyDown("f"))
            {
                m_horn.Play();
            }
        }
    }

}
