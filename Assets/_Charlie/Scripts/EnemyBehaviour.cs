using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: EnemyMovement
*
* Author: Charlie Taylor
*
* Description: Class used to control the Enemies. Child of the CarMovement Class
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 04/07/2021    CT          1.00        -Initial Created
* 20/07/2021    CT          1.10        -Layed foundations for AI, with checks for
*                                       hazards.
* 28/07/2021    CT          1.20        -Implemented basics for rubberbanding,
*                                       including a reference to a new manager script
**************************************************************************************/
public class EnemyBehaviour : CarBehaviour
{
    [Header("---AI Stuff---")]
    [SerializeField]
    [Tooltip("How Far infront of the enemy they should check for obstacles.")]
    private float m_distanceToCheck = 15;

    //Rubberbanding manager
    private RubberbandManager m_rubberbandManager;
    //Reference to level manager, to get the player object from
    private LevelManager m_levelManager;
    //The Player racer object
    private GameObject m_playerRacer;

    //Randomise max speed and acceleration up and down a little bit every second or 2
    private float m_accelerationChange = 1;
    private float m_maxSpeedChange = 5;

    //Timers in seconds
    private float m_randomChangeTime = 5;
    private float m_timer;

    private CarBehaviour m_playerRacersBehaviour;

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
        //Get Rubberband script from the manager object
        m_rubberbandManager = GameObject.Find("RubberbandManager").GetComponent<RubberbandManager>();

        //Get level manager by tag then get player
        m_levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        m_playerRacer = m_levelManager.GetPlayer();

        m_playerRacersBehaviour = m_playerRacer.GetComponent<CarBehaviour>();

        m_timer = m_randomChangeTime;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ApplyRubberbanding
    * 
    * Author: Charlie Taylor
    * 
    * Description: Checks the distance between self and the player, and depending on the
    *              result, will have its max speed affected by a multiplier.
    *              The values are derived from a manager class for the rubberband variables
    *              so that they can be edited in the inspector in 1 location
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/07/2021    CT          1.00        -Initial Created
    * 12/08/2021    CT          1.10        -Added acceleration and max speed randomness to
    *                                       zone C racers so they don't look the same every
    *                                       all together
    **************************************************************************************/
    private void ApplyRubberbanding()
    {
        //Timer for standard speed
        m_timer -= 1 * Time.deltaTime;



        m_distanceToPlayer = -DistanceToTarget(m_playerRacersBehaviour);
        //m_distanceToPlayer = 0; // = Function();
        //Check RubberBandManager for description on all the zones
        float _zoneAMin = m_rubberbandManager.m_zoneAMin;
        float _zoneBMin = m_rubberbandManager.m_zoneBMin;
        //Anything between zone B Min and zone D Min is zone C and has no Boost
        float _zoneDMin = m_rubberbandManager.m_zoneDMin;
        float _zoneEMin = m_rubberbandManager.m_zoneEMin;

        //Speed Boosts/Debuffs.
        float _stage1Boost = m_rubberbandManager.m_stage1Boost;
        float _stage2Boost = m_rubberbandManager.m_stage2Boost;


        //If in Zone E
        if (m_distanceToPlayer <= _zoneEMin)
        {
            m_maxSpeed = m_maxSpeedHolder * _stage2Boost;
        }
        //If in Zone D
        else if (m_distanceToPlayer > _zoneEMin && m_distanceToPlayer <= _zoneDMin)
        {
            m_maxSpeed = m_maxSpeedHolder * _stage1Boost;
        }
        //If in Zone C
        else if (m_distanceToPlayer > _zoneDMin && m_distanceToPlayer < _zoneBMin)
        {
            float _randomSpeedChange = Random.Range(-m_maxSpeedChange, m_maxSpeedChange + 1);
            float _randomAccelChange = Random.Range(-m_accelerationChange, m_accelerationChange + 1);
            if (m_timer <= 0)
            {
                m_timer = m_randomChangeTime;
                m_maxSpeed = m_maxSpeedHolder += _randomSpeedChange;

                m_speedIncrement = m_accelerationHolder += _randomAccelChange;
            }
        }
        //If in Zone B
        else if (m_distanceToPlayer >= _zoneBMin && m_distanceToPlayer < _zoneAMin)
        {
            m_maxSpeed = m_maxSpeedHolder / _stage1Boost;
        }
        //If in Zone A
        else if (m_distanceToPlayer >= _zoneAMin)
        {
            m_maxSpeed = m_maxSpeedHolder / _stage2Boost;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ChangeLane
    *
    * Author: Charlie Taylor
    *
    * Description: Makes the enemy change the lane, which lane is based on a mix of
    *              randomisation and time
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 19/07/2021    CT          1.00        -Initial Created
    * 02/07/2021    CT          1.10        -Edited lane switch randomisation to not go 
    *                                       below the min lanes, or above the max
    **************************************************************************************/
    private void ChangeLane()
    {

        m_laneSwitchSpeedScale = (m_currentSpeed / m_defaultSpeed) % 2;

        if (m_laneSwitchSpeedScale < 1f)
            m_laneSwitchSpeedScale = 1;

        int _lane = m_currentLane;
        int _targetLane;

        //if can change lanes
        //Either +1, -1, or 0. Randomly
        //Must make a temp Int variable otherwise it makes the random number a float.
        //Also max value is exclusive, not inclusive, so will return -1, 0 or 1, but not 2
        int _randomNumber = Random.Range(-1, 2);
        _targetLane = _lane + _randomNumber;
        if (_targetLane < 0)
        {
            _targetLane = 0;
        }
        //We have 5 lanes, but it is 0 based.
        else if (_targetLane > 4)
		{
            _targetLane = 4;
		}
        else if (_targetLane != _lane)
		{
            LaneSwitchCalculation(_targetLane);
        }
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckCollisions
    *
    * Author: Charlie Taylor
    *
    * Description: Function the check collisions that the enemy will detect using raycasts
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 19/07/2021    CT          1.00        -Initial Created
    * 13/08/2021    CT          2.00        -Merged in the DetectRacersInFront code, as it
    *                                       was basically the same.
    *                                       Also modified most of, if not all, the checks
    *                                       on when to change lanes, use powers and wether
    *                                       they should have timers on or not to make the 
    *                                       AI better
    **************************************************************************************/
    private void CheckCollisions()
    {
        if (!m_isChangingLane)
        {
            //Direction
            Vector3 _direction = m_lookTargetPos - transform.position;

            //Draw debug ray of collision length
            Debug.DrawRay(m_raycaster.position, (_direction * (m_currentSpeed * Time.deltaTime) * m_distanceToCheck), Color.red);

            RaycastHit _collision;
            //Send out a raycast from the racer and send it out as far as the enemy's speed, so that it won't go through anything, then add the extension.
            if (Physics.Raycast(m_raycaster.position, _direction, out _collision, (m_currentSpeed * Time.deltaTime) + m_distanceToCheck))
            {
                if (!m_decisionMade)
                {
                    switch (_collision.transform.tag)
                    {
                        case "Car":

                            if (m_canUsePower)
                            {
                                switch (m_powerItem)
                                {
                                    case "Projectile":
                                        Invoke("FireProjectile", 1);
                                        m_powerItem = "Empty";
                                        //Change lane a short while after so the shooter does not crash into their victim
                                        Invoke("ChangeLane", 0.75f);
                                        break;

                                    case "Star":
                                        SetStarMode();
                                        //Don't change lane because you are invulnerable now
                                        m_powerItem = "Empty";
                                        break;
                                }
                                //Must make sure can use power is false again, otherwise they keep using it
                                m_canUsePower = false;
                            }
                            else
                            {
                                ChangeLane();
                            }


                            m_decisionMade = true;
                            break;


                        case "Hazard":
                            //No need to change if still invulnerable from spin out or star time
							if (!m_invulnerable)
                            {
                                //If you can set star mode, use it
                                if (m_canUsePower && m_powerItem == "Star")
								{
                                    SetStarMode();
								}
                                else
                                //Otherwise move out the way
								{
                                    ChangeLane();
                                }
                                
                            }
                            m_decisionMade = true;
                            break;

					}
                }


                if (_collision.transform.tag == "Car")
                {

                    if (!m_decisionMade)
                    {
                        m_decisionMade = true;
                        if (m_powerItem == "Projectile")
                        {
                            {
                                m_canUsePower = false;

                            }
                        }
                    }

                }
            }
        }
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    *
    * Description: Run all the funcitons needed to make the enemy play like a real person
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 19/07/2021    CT          1.00        -Initial Created
    * 28/07/2021    CT          1.10        -Added ApplyRubberbanding script
    **************************************************************************************/
    private new void Update()
    {
        if (!m_raceController.GetFlyByStatus() || !m_raceController.GetCountdownStatus() || m_gameController.GetRaceStatus())
            return;

        base.Update();

        //Do it after so it is similar to player behaviour's order
        CheckCollisions();
        ApplyRubberbanding();
    }
}
