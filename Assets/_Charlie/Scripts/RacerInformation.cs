using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: RacerInformation
* 
* Author: Charlie Taylor
* 
* Description: A class for storing all the information for a given racer. This is
*              attached to racers instead of player or enemy behaviour is due to every
*              racer can be a player or an AI, so this is where the values are set, and
*              then depending on if the racer is set to be the player or not, depends
*              on if PlayerBehaviour or EnemyBehaviour is attached to the racer, with 
*              the information set here
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 31/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class RacerInformation : MonoBehaviour
{
    /*
     * A lot here probably should not have been set up on a racer by racer basis, and only some things like the names should have been set up individually, like name and lane
    However it worked for a long time, and by the time the realisation came to that it was not the most efficient way, it was a bit too late.
    Further work would improve this, and make the core mechanics such as speed and power up times unified.
    */

    //Enum allows the car names to be selected from a dropdown menu, rather than typed, preventing typos that would break A LOT
    private enum CarNames
	{
        F1,
        Firetruck,
        Train,
        URO,
        Wasp,
        Tank
	}

    //This is what actually ends up being selected from the dropdown
    [SerializeField]
    [Tooltip("The name of this racer")]
    private CarNames m_carName;    

    //But due to the above not being a string, this will store that value ToString()-ified
    protected string m_objName;

    [Header("---Speed Variables---")]
    //Speed Variables
    [SerializeField]
    [Tooltip("Speed of car, without any inputs pressed")]
    protected float m_defaultSpeed = 30f;
    [SerializeField]
    [Tooltip("Maximum Speed of car")]
    protected float m_maxSpeed = 40f;
    [SerializeField]
    [Tooltip("Acceleration of vehicle per second")]
    protected float m_speedIncrement = 0.3f;

    //Lane Switching Variables
    [SerializeField]
    [Tooltip("How fast the car switches lanes")]
    private float m_laneSwitchSpeed = 0.1f;

    //Spin Out Values
    [Header("--Spin Out Values--")]
    [SerializeField]
    [Tooltip("The speed at which the car will slow down to after a spin out")]
    private float m_targetVelocity = 0;
    //How far the racer spins out to
    [SerializeField]
    [Tooltip("The distance in units a car will go after spinning out")]
    private float m_spinoutDistance = 10;
    //Timer in seconds
    [SerializeField]
    [Tooltip("How long in seconds until the car starts accelerating again")]
    private float m_spinoutTime = 3;
    //How long the racer is stationary after a spinout
    [SerializeField]
    [Tooltip("How long the car is stationary before reaccelerating. (Added onto Spin Out Time)")]
    private float m_stationaryTime = 1;
    //Variables for invulnerable timer
    [SerializeField]
    [Tooltip("How long after spinning out the player is invulnerable. (This is added onto the the Spinout Time and Stationary Time)")]
    private float m_invulnerableTime = 1;

    //Power Up
    [Header("---Power Ups---")]

    [SerializeField]
    [Tooltip("How long in seconds a racer is in Star Time")]
    [Range(5, 15)]
    private float m_starTime = 7;

    private float[] m_speedValues;
    private float[] m_spinOutValues;


    /**************************************************************************************
    * Type: Functions
    * 
    * Name: Start
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private void Start()
    {
        //INSIDE THE CAR BEHAVIOUR IS SOMETHING VERY SIMILAR (But like, reverse), MAKE SURE IT HAS THE EXACT SAME FORMAT AS THIS!!!
        m_speedValues = new float[] { m_defaultSpeed, m_maxSpeed, m_speedIncrement, m_laneSwitchSpeed };
        m_spinOutValues = new float[] { m_targetVelocity, m_spinoutDistance, m_spinoutTime, m_stationaryTime, m_invulnerableTime };
    }


    /**************************************************************************************
    * Type: Functions
    * 
    * Name: GetRacerValues
    * Returns: _bothArrays
    *
    * Author: Charlie Taylor
    *
    * Description: Returns a jagged array of the 2 arrays of the values needed to be 
    *              passed in
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 31/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public float[][] GetRacerValues()
    {
        //Create a Jagged array to store both arrays in one return values
        float[][] _bothArrays;

        _bothArrays = new float[2][];
        //FIRST is speed Variables
        _bothArrays[0] = m_speedValues;
        //SECOND is spinout variables
        _bothArrays[1] = m_spinOutValues;

        return _bothArrays;
    }
    /**************************************************************************************
    * Name: GetRacerName
    * 
    * Author: Charlie Taylor
    * 
    * Description: Get this racers name, to check if it is the player car
    **************************************************************************************/
    public string GetRacerName()
    {
        m_objName = m_carName.ToString();
        return m_objName;
    }

    /**************************************************************************************
    * Name: GetStarTime
    * 
    * Author: Charlie Taylor
    * 
    * Description: Get the star time value (Probably should have been moved elsewhere, but
    *              time contraints)
    **************************************************************************************/
    public float GetStarTime()
    {
        return m_starTime;
    }
}
