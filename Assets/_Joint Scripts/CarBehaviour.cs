using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**************************************************************************************
* Type: Class
* 
* Name: CarBehaviour
*
* Author: Dean Pearce
*
* Description: Class used to manage movement of players and enemy cars.
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 01/07/2021    DP          1.00        -Initial Created
* 13/07/2021    DP/CT       2.00        -Merged multiple versions into one to make a 
*                                       new main version
* 14/07/2021    CT          3.00        -Moved a lot of duplicate code used in
*                                       projectile into a higher parent class
* 14/07/2021    DP          3.10        -Added begining of Shortcut functionality                                      
**************************************************************************************/
public class CarBehaviour : TrackFollower
{
    //Game Controller Reference
    protected GameController m_gameController;

    //Bool to check if script's start has finished
    private bool m_hasRunStart = false;

    [Header("---Speed Variables---")]
    //Speed Variables
    [SerializeField]
    [Tooltip("Speed of car, without any inputs pressed")]
    protected float m_defaultSpeed = 10f;
    [SerializeField]
    [Tooltip("Maximum Speed of car")]
    protected float m_maxSpeed = 15f;
    [SerializeField]
    [Tooltip("Minimum speed of car")]
    protected float m_minSpeed = 7f;
    [SerializeField]
    [Tooltip("Acceleration of vehicle per second")]
    protected float m_speedIncrement = 3f;

    //Holds the max speed for when it gets modified in the UI, and for rubberbanding
    protected float m_maxSpeedHolder;
    //Same as above, but acceleration
    protected float m_accelerationHolder;

    protected bool m_accelertating;

    [HideInInspector]
    public string m_objName;

    //Lane Switching Variables
    [SerializeField]
    [Tooltip("How fast the car switches lanes")]
    private float m_laneSwitchSpeed = 0.1f;
    protected float m_laneSwitchSpeedScale;
    protected bool m_isChangingLane = false;
    private float m_laneSwitchDistance = 0.1f;
    private float m_laneSwitchInterpolateAmount;
    private float m_laneSwitchInterpolateAmountFollow;
    private Vector3 m_laneSwitchStartPos, m_laneSwitchEndPos, m_laneSwitchStartPosFollow, m_laneSwitchEndPosFollow;
    private int m_routeAfterSwitch;
    private int m_followRouteAfterSwitch;

    //Lap Variables
    private int m_currentLap = 1;
    [HideInInspector]
    public int m_currentPositionInRace;
    protected float m_distanceToPlayer;

    //The child object of the car's visuals
    private GameObject m_visualCar;

    //Empty Object that raycasts are sourced from
    protected Transform m_raycaster;

    [Header("--Spin Out Values--")]
    [SerializeField]
    [Tooltip("The speed at which the car will slow down to after a spin out")]
    private float m_targetVelocity = 0;

    [SerializeField]
    [Tooltip("The distance in units a car will go after spinning out")]
    private float m_spinoutDistance = 10;

    //Timer in seconds
    [SerializeField]
    [Tooltip("How long in seconds until the car starts accelerating again")]
    private float m_spinoutTime = 3;
    //Variable to iterate the timer
    private float m_spinoutTimerIterator;

    [SerializeField]
    [Tooltip("How long the car is stationary before reaccelerating. (Added onto Spin Out Time)")]
    private float m_stationaryTime = 1;

    //Bool used to check if the car is currently spinning out or not
    protected bool m_isSpinning = false;

    //How fast the car decelerates when spinning out. 
    private float m_deceleration;

    //Currently Invulnerable
    protected bool m_invulnerable;

    //Variables for invulnerable timer
    [SerializeField]
    [Tooltip("How long after spinning out the player is invulnerable. (This is added onto the the Spinout Time and Stationary Time)")]
    private float m_invulnerableTime = 1;
    protected float m_iTimeIterator;
    private float m_invulnerableTimeBase;

    //For AI
    protected bool m_decisionMade = false;

    [SerializeField]
    [Tooltip("How long in seconds a racer has to wait before making another decision")]
    private float m_decisionMadeTime = 4;
    //Variable to iterate the timer
    private float m_decisionTimerIterator;

    //Colour percentage modifications, for invulnerability
    private float m_red = 1;
    private float m_green = 1;
    private float m_blue = 1;

    private ColourManager m_colourManager;


    //ProjectilePool
    private ObjectPool m_objectPool;

    protected bool m_canUsePower;
    protected string m_powerItem;
    //Power Up
    [Header("--Star Time--")]
    [SerializeField]
    [Tooltip("How long in seconds a racer is in Star Time")]
    [Range(5, 15)]
    private float m_starTime;

    //Race Start class
    protected RaceStartController m_raceController;

    // Sounds //
    private AudioSource m_coreAudioSource;
    //A secondary audio source for sounds that need to play ontop of driving
    private AudioSource m_secondaryAudioSource;

    //Bool to check of the player has started racing, after the camera pan, and when it is true, start the sound
    private bool m_startedDriveSound;

    //For storing this vehicles unique driving sound
    private AudioClip m_drivingSound;

    private AudioClip m_pickupSound;
    private AudioClip m_starTimeSound;
    private AudioClip m_skidSound;



    //DEBUG INFO
    //[Header("---DEBUG---")]
    //[SerializeField]
    //private TextMeshPro m_debugText;


    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Charlie Taylor & Dean Pearce
    **************************************************************************************/
    protected void Start()
    {
        m_gameController = GameObject.Find("GameController").GetComponent<GameController>();

        
        SetStats();

        //m_debugText = transform.Find("DebugText").gameObject.GetComponent<TextMeshPro>();
        LaneSetup();

        //Find Object Poolf
        m_objectPool = FindObjectOfType<ObjectPool>();

        m_raycaster = gameObject.transform.Find("Raycaster").gameObject.transform;
        m_visualCar = gameObject.transform.GetChild(0).gameObject;

        //Set controller in start rather than making it a serializable field
        m_animController = m_visualCar.GetComponent<Animator>();

        //Set spinout time to add on the stationary time, then set the iterator to that value
        m_spinoutTimerIterator = m_spinoutTime + m_stationaryTime;

        //Set i Frame time to add on the spinout time, then set the iterator to that value
        m_invulnerableTime += m_spinoutTimerIterator;
        m_iTimeIterator = m_invulnerableTime;




        //Assign the max speed and acceleration holders
        m_maxSpeedHolder = m_maxSpeed;
        m_accelerationHolder = m_speedIncrement;

        //Gets the race controller script instance, then sets the vehicles position to the starting line
        m_raceController = GameObject.Find("RaceStartController").GetComponent<RaceStartController>();

        //Created a base i time for when swapping from the star power and the normal spinout timer
        m_invulnerableTimeBase = m_invulnerableTime;

        m_decisionTimerIterator = m_decisionMadeTime;

        //Sounds
        AudioManager _audioManager = m_gameController.GetComponent<AudioManager>();

        m_coreAudioSource = gameObject.GetComponent<AudioSource>();
        m_secondaryAudioSource = transform.Find("SecondarySounds").gameObject.GetComponent<AudioSource>();

        //Store this vehicles unique driving sound that is allocated in the prefab, on the Source
        m_drivingSound = m_coreAudioSource.clip;

        //Assign sounds from the audio manager script
        m_pickupSound = _audioManager.m_pickUp;
        m_starTimeSound = _audioManager.m_starTime;
        m_skidSound = _audioManager.m_skid;

        //Colour Manager
        m_colourManager = m_gameController.GetComponent<ColourManager>();
        //Set up colours for invulnerability
        m_red = m_colourManager.m_red;
        m_blue = m_colourManager.m_blue;
        m_green = m_colourManager.m_green;

        m_hasRunStart = true;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetStats
    *
    * Author: Charlie Taylor
    *
    * Description: To make it so that the designer only has to edit 1 script for a racer,
    *              whether it's going to a player or AI, the values they can change were
    *              all put into a different script that is attached to the racer, that 
    *              will then have all of its values moved over into the actual racer
    *              via the Level Manager and this script.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 30/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    private void SetStats()
	{
        //Get the RacerInformation script
        RacerInformation _racerInformation = GetComponent<RacerInformation>();
        //Call the get values function and return it into a temp variable
        float[][] _racerInfoHolder = _racerInformation.GetRacerValues();
        //The first element of the returned Jagged Array is the Speed Values
        float[] _speedValues = _racerInfoHolder[0];
        //The second element of the returned Jagged Array is the spinout Values
        float[] _spinOutValues = _racerInfoHolder[1];

        m_objName = _racerInformation.GetRacerName();

        m_starTime = _racerInformation.GetStarTime();

        //Assign Speed Values
        m_defaultSpeed = _speedValues[0];
        m_maxSpeed = _speedValues[1];
        m_speedIncrement = _speedValues[2];
        m_laneSwitchSpeed = _speedValues[3];

        //Assign Spinout Values
        m_targetVelocity = _spinOutValues[0];
        m_spinoutDistance = _spinOutValues[1];
        m_spinoutTime = _spinOutValues[2];
        m_stationaryTime = _spinOutValues[3];
        m_invulnerableTime = _spinOutValues[4];

        //Remove the reacer information Script
        Destroy(_racerInformation);
    }



    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * 
    * Description: Does everything needed for every movement based object. Inherited by
    *              children, which is why its protected
    *              Not inherited
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 04/07/2021    CT          1.00        -Initial Created
    * 28/07/2021    CT          1.10        -Moved collision checks into a seperate 
    *                                       function
    * 09/08/2021    DP          1.20        -Added if statement to prevent vehicle update 
    *                                       running if the
    *                                       race hasn't started (determined by the fly by, 
    *                                       and then countdown)
    * 13/08/2021    CT          1.30        -Added Sound effect support for when returning
    *                                       to driving, based on when the update function
    *                                       properly starts
    * 17/08/2021    CT          1.31        -Removed check on CheckCollisions for 
    *                                       m_invulnerable, and moved checks to the collisons
    *                                       themselves
    **************************************************************************************/
    protected void Update()
    {
        //Checking if the FlyByCam and Countdown have finished, or if the race is over. Returns out if necessary to prevent vehicles moving when they shouldn't
        if (!m_raceController.GetFlyByStatus() || !m_raceController.GetCountdownStatus() || m_gameController.GetRaceStatus())
        {
            return;
        }

        if (!m_startedDriveSound)
		{
            m_startedDriveSound = true;
            m_coreAudioSource.Play();

        }
        //Check Timer states at the start.
        CheckTimers();

        //If not spinning, so car is in control, do the accleration and stuff
        if (!m_isSpinning)
        {
            MaintainSpeed();
        }






        if (!m_isChangingLane)
        {
            MoveObject(transform);
        }
        if (m_isChangingLane)
            SmoothLaneMove();

        CheckCollisions();

        /*
        m_debugText.text = ("Name: " + m_objName + "\n" +
                            "Current Route: " + m_currentRouteNum + "\n" +
                            "Current Speed: " + m_currentSpeed + "\n" +
                            "Acceleration: " + m_speedIncrement + "\n" +
                            "Max Speed: " + m_maxSpeed + "\n" +
                            "Distance To Player: " + m_distanceToPlayer + "\n" +
                            "Decision Made: " + m_decisionMade);*/

    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckCollisions
    * 
    * Author: Charlie Taylor
    * 
    * Description: Checks collisions for the cars by firing out a raycast infront of it
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 04/07/2021    CT          1.00        -Initial Created
    * 28/07/2021    CT          1.10        -Moved out of Update() function
    *                                       Added check for if the car detects itself
    * 15/08/2021    CT          1.20        -Added Sound effect support for picking up
    *                                       power ups
    **************************************************************************************/
    private void CheckCollisions()
	{
        Vector3 _direction;
        Vector3 _source = m_raycaster.position;
        //_direction = m_followTarget.position - transform.position;
        _direction = m_lookTargetPos - transform.position;
        RaycastHit _collision;
        Debug.DrawRay(_source, _direction * m_currentSpeed * Time.deltaTime, Color.blue);
        if (Physics.Raycast(_source, _direction, out _collision, m_currentSpeed * Time.deltaTime))
        {
            GameObject _collidedObject = _collision.transform.gameObject;
            switch (_collidedObject.tag)
            {
                case "Hazard":
                    if (!m_invulnerable)
                    {
                        //Spinout
                        SpinOut();
                    }
                    break;

                case "Car":
                    //IF NOT SELF
                    //When traveling at high speeds, the cars could sometimes detect themselves, and
                    //due to them also having the Car tag, would spin out. This check stops it. 
                    //(kept "this." for readability
                    if (_collidedObject != this.gameObject)
                    {
                        if (!m_invulnerable)
                        {
                            //Spinout
                            SpinOut();
                        }
						}
						break;

                case "PowerUp":
                    //Set power up to be whatver the item box had.
                    PowerUp _powerUp = _collidedObject.GetComponent<PowerUp>();
                    m_powerItem = _powerUp.GetPowerUp();
                    _powerUp.RemovePickup();
                    m_canUsePower = true;

                    //Play Sound
                    m_secondaryAudioSource.Stop();
                    m_secondaryAudioSource.clip = m_pickupSound;
                    m_secondaryAudioSource.Play();

                    break;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CheckTimers
    *
    * Author: Charlie Taylor
    *
    * Description: Check the state of basic timers every frame. 
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 03/07/2021    CT          1.00        -Initial Created
    * 26/07/2021    CT          1.10        -Added function that resets colour of player
    *                                       at end of invulnerable timer
    * 10/08/2021    CT          1.20        -Moved other timer based checks into here
    * 15/08/2021    CT          1.30        -Added Sound effect support for spin out
    **************************************************************************************/
    private void CheckTimers()
    {
        //Invulnerability Timer
        if (m_invulnerable)
        {
            m_iTimeIterator -= 1 * Time.deltaTime;
        }

        if (m_iTimeIterator <= 0)
        {
            m_invulnerable = false;
            m_iTimeIterator = m_invulnerableTimeBase;

            //False being reset colours to original
            SetColours(false);
        }

        if (m_isSpinning)
        {
            //During Spinout
            m_spinoutTimerIterator -= Time.deltaTime;

            //Decelerate using calcualtion in SpinOut()
            if (m_currentSpeed > 0)
            {
                m_currentSpeed += m_deceleration * Time.deltaTime;
            }
            //Keep it at 0, never below
            else if (m_currentSpeed < 0)
            {
                m_currentSpeed = 0;
            }

        }

        //When spinout has finished, reset
        if (m_spinoutTimerIterator <= 0)
        {
            //Make sure the playing is stopped (It should be, but make sure)
            m_coreAudioSource.Stop();
            //Set the new sound to be the driving sound again
            m_coreAudioSource.clip = m_drivingSound;
            //Set it to make to sound loop
            m_coreAudioSource.loop = true;
            //Play it
            m_coreAudioSource.Play();

            m_isSpinning = false;
            //Reset timer (Remember to check the one in start if you change this line)
            m_spinoutTimerIterator = m_spinoutTime;

        }

        //Every set amount of seconds, reset decision made. This will add a little bit of randomness as usually, a racer could
        //not perform 2 acts in quick succession, but if they get lucky on the timer, they could
        if (m_decisionMade)
		{
            m_decisionTimerIterator -= 1 * Time.deltaTime;
		}
        if (m_decisionTimerIterator <= 0)
		{
            m_decisionMade = false;
            m_decisionTimerIterator = m_decisionMadeTime;
        }
        

    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SpinOut
    *
    * Author: Charlie Taylor
    *         William Harding
    *
    * Description: The function that will be called when a car needs to spin out
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 27/06/2021    CT          1.00        -Initial Created
    * 02/07/2021    CT/WH       1.10        -Added SUVAT equations, to calculate 
    *                                       deceleration.
    **************************************************************************************/
    public void SpinOut()
    {
        if (!m_invulnerable)
        {
            //Make sure the playing is stopped, as driving is looped
            m_coreAudioSource.Stop();
            //Set the new sound to be the tire skid sound
            m_coreAudioSource.clip = m_skidSound;
            //Set it to make to sound not loop
            m_coreAudioSource.loop = false;
            //Play it
            m_coreAudioSource.Play();

            m_animController.SetTrigger("isSpinning");
            //isSpinning affects what code runs in Update
            m_isSpinning = true;
            SetInvulnerable();
            //Work out the initial Velocity NEEDED to meet reach the distance in the set time
            float _initialVelocity = ((2 * m_spinoutDistance) / m_spinoutTime) + m_targetVelocity;
            //Set the speed to that value
            m_currentSpeed = _initialVelocity;


            //And then, decelerate to the target velocity, to a set position, over the set time
            m_deceleration = 2 * ((m_targetVelocity * m_spinoutTime) - m_spinoutDistance) / (Mathf.Pow(m_spinoutTime, 2));
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetInvulnerable
    *
    * Author: Charlie Taylor
    *
    * Description: Sets up necessary things for when player is invulnerable after being
    *              hit wiht a projectile or colliding with a hazard
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    CT          1.00        -Initial Created
    * 26/07/2021    CT          1.10        -Added function that changes colour of player
    **************************************************************************************/
    private void SetInvulnerable()
    {
        m_invulnerable = true;
        SetColours(true);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetColours
    * Parameters: p_seting
    *
    * Author: Charlie Taylor
    *
    * Description: Set the colours of the mesh renderer of cars to be tinted when becoming
    *              invulnerable
    *              p_setting is true when setting the colour. False when resetting to default
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 26/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    private void SetColours(bool p_setting)
	{
        MeshRenderer[] _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var i_object in _meshRenderers)
        {
            if (p_setting)
            {
                //Have to do a check to see if the material has the property, as without the check, it says it doesn't have it
                if (i_object.material.HasProperty("_Color"))
                {
                    i_object.material.color = new Color(i_object.material.color.r + m_red, i_object.material.color.g + m_green, i_object.material.color.b + m_blue);
                }
            }
            else //if RE setting
            {
                if (i_object.material.HasProperty("_Color"))
                {
                    i_object.material.color = new Color(i_object.material.color.r - m_red, i_object.material.color.g - m_green, i_object.material.color.b - m_blue);
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MaintainSpeed
    *
    * Author: Dean Pearce
    *
    * Description: Makes sure the speed is brought back to default if the player isn't 
    *              manipulating it
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 04/07/2021    CT          1.10        -Removed redundant check for inputs.
    * 28/07/2021    CT          1.20        -Added extra checks for speed, based on new
    *                                       m_accelerating value. 
    *                                       Also made speed not jitter when resting at
    *                                       default speed
    * 01/08/2021    CT          1.21        -Rearanged some statements for efficiency,
    *                                       and made a temporary variable called 
    *                                       _acceleration which is used instead of 
    *                                       m_speedIncrement, and also it uses Delta Time
    *                                       now
    *                                       Made sure the AI who are rubberbanding forward
    *                                       don't have their max speeds knocked down 
    *                                       abruptly when crossing zone thresholds
    **************************************************************************************/
    private void MaintainSpeed()
    {
        float _acceleration = m_speedIncrement * Time.deltaTime;
        //For now, maybe change? Ask Alex. (Even if it stays the same, better to read)
        float _decceleration = _acceleration * 0.75f;

        //Only call and set, if current speed is not already deafault
        //On the next frame, if car accelerated, it would end up past the defualt speed.
        //Car only accelerates (no input) when BELOW default speed, so setting directly to it will stop that call
        if (m_currentSpeed != m_defaultSpeed)
        {
            if ((m_currentSpeed > m_defaultSpeed - _acceleration) && (m_currentSpeed < m_defaultSpeed + _acceleration))
            {
                m_currentSpeed = m_defaultSpeed;
            }
        }
        //Note: Enemies are ALWAYS accelerating to their top speed.
        if (m_currentSpeed < m_defaultSpeed)
        {
            m_accelertating = true;
        }

        //If not at max speed, and holding forward,, Accelerate
        if (m_accelertating && m_currentSpeed < m_maxSpeed)
        {
            m_currentSpeed += _acceleration;
        }

        //Deccelerate if above default and not holding forward
        if (!m_accelertating && m_currentSpeed > m_defaultSpeed)
        {
            m_currentSpeed -= _decceleration;

        }

        //If passed max speed due to acceleration weirdness, SET to max speed
        //But to solve the issue where rubberbanding cars who cross the thresholds for max speeds,
        //check if it is only within the small window that is caused by acceleration going over the max
        if (m_currentSpeed > m_maxSpeed && m_currentSpeed < m_maxSpeed + _decceleration)
        {
            m_currentSpeed = m_maxSpeed;
        } 
        //Is above the threshold and just needs to decelerate
        else if (m_currentSpeed > m_maxSpeed + _decceleration)
		{
            m_currentSpeed -= _decceleration;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LaneSwitchCalculation
    * Parameters: p_laneToSwitch
    *
    * Author: Dean Pearce
    *
    * Description: Calculates where the moving object should move from and to using linear interpolation.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    DP          1.10        -Swapped transform position for Vector3 variable 
    *                                       m_lookTargetPos
    * 14/07/2021    DP          1.20        -Modified RoutePositions parameters to match
    *                                       new version
    * 20/07/2021    DP          1.30        -Reworked the way the lane switching 
    *                                       speed/distance is calculated
    **************************************************************************************/
    protected void LaneSwitchCalculation(int p_laneToSwitch)
    {
        m_laneSwitchDistance = 1 / DistanceRemainder(TotalDistanceOfSegment(m_currentRouteNum));
        m_isChangingLane = true;
        m_routeAfterSwitch = m_currentRouteNum;
        m_followRouteAfterSwitch = m_currentRouteNumFollow;

        //Checks for if the route will be different after lane switch
        if ((m_interpolateAmount + m_laneSwitchDistance) % 1 <= m_interpolateAmount)
            m_routeAfterSwitch = (m_routeAfterSwitch + 1) % m_routesArray.Length;
        if ((m_followTargetInterpolate + m_laneSwitchDistance) % 1 <= m_followTargetInterpolate)
            m_followRouteAfterSwitch = (m_followRouteAfterSwitch + 1) % m_routesArray.Length;

        //Getting the start and end positions ready for lane switch
        m_laneSwitchStartPos = transform.position;
        m_laneSwitchEndPos = RoutePositions((m_interpolateAmount + m_laneSwitchDistance) % 1, m_routeAfterSwitch, p_laneToSwitch, m_currentShortcutRoute);
        m_laneSwitchStartPosFollow = m_lookTargetPos;
        m_laneSwitchEndPosFollow = RoutePointByDistance(m_lookTargetDistance, (m_interpolateAmount + m_laneSwitchDistance) % 1, m_routeAfterSwitch, p_laneToSwitch, m_currentShortcutRoute);

        m_currentLane = p_laneToSwitch;
        m_laneSwitchInterpolateAmount = 0;
        m_laneSwitchInterpolateAmountFollow = 0;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DistanceRemainder
    * Parameters: p_distance
    *
    * Author: Dean Pearce
    *
    * Description: Calculates the remainder of the Distance for lane changing
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 21/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private float DistanceRemainder(float p_distance)
    {
        float _distanceDiv = p_distance / 5f;
        return _distanceDiv;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SmoothLaneMove
    *
    * Author: Dean Pearce
    *
    * Description: Uses similar logic as MoveObject() but instead using the calculated 
    *              points from LaneSwitchCalculation()
    *              Moves the object smoothly between lanes
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    DP          1.10        -Swapped transform position for Vector3 
    *                                       variable m_lookTargetPos
    * 20/07/2021    DP          1.20        -Cleaned up the way interpolation is used to 
    *                                       move between lanes
    **************************************************************************************/
    private void SmoothLaneMove()
    {
        //Increasing interpolation for lane switch movement, Look target is set slightly faster
        //to make the vehicle slowly turn towards it
        m_laneSwitchInterpolateAmount += (m_laneSwitchSpeed * Time.deltaTime) * m_currentSpeed;
        m_laneSwitchInterpolateAmountFollow += ((m_laneSwitchSpeed * 1.5f) * Time.deltaTime) * m_currentSpeed;

        //Moving the objects
        transform.LookAt(m_lookTargetPos);
        transform.position = Vector3.Lerp(m_laneSwitchStartPos, m_laneSwitchEndPos, m_laneSwitchInterpolateAmount);
        m_lookTargetPos = Vector3.Lerp(m_laneSwitchStartPosFollow, m_laneSwitchEndPosFollow, m_laneSwitchInterpolateAmountFollow);

        //Once interpolation reaches 1, lane switching is finished, resets values
        if (m_laneSwitchInterpolateAmount >= 1f)
        {
            m_isChangingLane = false;
            m_laneSwitchInterpolateAmount = 0;
            m_laneSwitchInterpolateAmountFollow = 0;
            m_interpolateAmount += m_laneSwitchDistance;
            m_followTargetInterpolate += m_laneSwitchDistance;
            if (m_interpolateAmount > 1f)
            {
                m_interpolateAmount = m_interpolateAmount % 1;
                m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;
            }
            if (m_followTargetInterpolate > 1f)
            {
                m_followTargetInterpolate = m_followTargetInterpolate % 1;
                m_currentRouteNumFollow = (m_currentRouteNumFollow + 1) % m_routesArray.Length;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MoveObject
    *
    * Author: Dean Pearce
    *
    * Description: Override version of MoveObject() from TrackFollower. Used so that cars can use lap detection.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 24/07/2021    DP          1.00        -Initial Created
    * 04/08/2021    DP          1.10        -Added extra tag conditionals to check if
    *                                       a 4 point route should have no curve
    **************************************************************************************/
    protected override void MoveObject(Transform p_moveTarget)
    {
        float _distanceOfSegment;

        //Checking for NoCurveTag to use the correct function for distance calculation
        if (m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurve" ||
            m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeL" ||
            m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeR")
            _distanceOfSegment = TotalDistanceOfSegment(m_currentRouteNum, m_currentShortcutRoute);
        else
            _distanceOfSegment = TotalDistanceOfSegment(m_currentRouteNum);

        //Using distance to make the speed even on routes of varying distances
        m_interpolateAmount += (m_currentSpeed * Time.deltaTime) / _distanceOfSegment;

        p_moveTarget.position = RoutePositions(m_interpolateAmount, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);

        //Setting look target at a specified distance
        m_lookTargetPos = RoutePointByDistance(m_lookTargetDistance);
        p_moveTarget.LookAt(m_lookTargetPos);

        //Once interpolation hits 1, sets values for the next route
        if (m_interpolateAmount >= 1)
        {
            if (m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurve" ||
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeL" ||
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeR")
            {
                m_currentShortcutRoute++;
                if (m_currentShortcutRoute > 2)
                {
                    m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;
                    m_currentShortcutRoute = 0;
                }
            }
            else
                m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;

            //Code will only reach here if vehicle is passing to route 0, which means the vehicle
            //passed a lap
            if (m_currentRouteNum == 0)
                m_currentLap++;

            m_interpolateAmount = 0f;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TotalDistanceOfLap
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Gets the total distance of a lap in the race.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 30/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    protected float TotalDistanceOfLap()
    {
        float _cumulativeDistance = 0;
        for (int i = 0; i < m_routesArray.Length; i++)
        {
            _cumulativeDistance += TotalDistanceOfSegment(i);
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DistanceRemainingInLap
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Gets the remaining distance of the current lap in the race.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 30/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    protected float DistanceRemainingInLap()
    {
        float _cumulativeDistance = 0;
        for (int i = m_currentRouteNum; i < m_routesArray.Length; i++)
        {
            if (i == m_currentRouteNum)
            {
                _cumulativeDistance += RemainingDistanceOfSegment(m_currentRouteNum, m_interpolateAmount);
            }
            else
            {
                _cumulativeDistance += TotalDistanceOfSegment(i);
            }
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DistanceToTarget
    * Parameters: p_targetObject
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Gets the distance to the target given in the parameter.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 30/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public float DistanceToTarget(CarBehaviour p_targetObject)
    {
        float _cumulativeDistance = 0;
        //If target is on the same lap
        if (m_currentLap == p_targetObject.GetLap())
        {
            //Difference of distance from target
            _cumulativeDistance = Mathf.Abs(DistanceRemainingInLap() - p_targetObject.DistanceRemainingInLap());

            //If distance remaining is less than target, distance is negative
            if (DistanceRemainingInLap() < p_targetObject.DistanceRemainingInLap())
                _cumulativeDistance = -_cumulativeDistance;
        }
        //If target is a lap or multiple laps ahead
        else if (m_currentLap < p_targetObject.GetLap())
        {
            for (int i = m_currentLap; i < p_targetObject.GetLap(); i++)
            {
                //If target is only 1 lap ahead
                if (i == p_targetObject.GetLap() - 1)
                    _cumulativeDistance += DistanceRemainingInLap() + Mathf.Abs(TotalDistanceOfLap() - p_targetObject.DistanceRemainingInLap());
                //If more than 1 lap, add the distance of a whole lap
                else
                    _cumulativeDistance += TotalDistanceOfLap();
            }
        }
        //If a lap or multiple laps ahead of the target
        else if (m_currentLap > p_targetObject.GetLap())
        {
            for (int i = p_targetObject.GetLap(); i < m_currentLap; i++)
            {
                //If 1 lap ahead
                if (i == m_currentLap - 1)
                    _cumulativeDistance += DistanceRemainingInLap() + Mathf.Abs(TotalDistanceOfLap() - p_targetObject.DistanceRemainingInLap());
                //If more than 1 lap, add the distance of a whole lap
                else
                    _cumulativeDistance += TotalDistanceOfLap();
                _cumulativeDistance = -_cumulativeDistance;
            }
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoutePointByDistance
    * Parameters: p_distanceOfPointToGet
    * Return: _routePoint
    *
    * Author: Dean Pearce
    *
    * Description: Gets the route information of a point by how far ahead/behind it is from the object
    *              Used to determine the cars look target position, and the starting grid positions
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 13/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public Vector3 RoutePointByDistance(float p_distanceOfPointToGet)
    {
        float _totalDistance = 0f;

        //Increment to check by determines how precise the function is. Lower number means more precision, but at the cost of performance
        //Since this is used by all vehicles every update for the look target, 0.05f is a decent balance between precision and performance.
        float _incrementToCheckBy = 0.05f;

        Vector3 _currentPos = RoutePositions(m_interpolateAmount, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);
        Vector3 _nextPos = _currentPos;
        Vector3 _routePoint = _currentPos;

        //If parameter number is positive or 0, searches ahead
        if (p_distanceOfPointToGet >= 0)
        {
            //For loop runs until it finds a point that is the specified distance away on the routes
            for (float i = 0; _totalDistance < p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                //If interpolation + i is less than 1, then the resulting position will be on the same route segment
                if (m_interpolateAmount + i <= 1f)
                    _nextPos = RoutePositions(m_interpolateAmount + i, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);
                //If interpolation + i is greater than 1, and i is less than 1, resulting position will be on the next route segment
                else if (m_interpolateAmount + i > 1f && i < 1f)
                    _nextPos = RoutePositions((m_interpolateAmount + i) % 1f, (m_currentRouteNum + 1) % m_routesArray.Length, m_currentLane, m_currentShortcutRoute);
                //If i is greater than 1, then the distance will be further than a whole route segment away, which isn't needed
                //Breaks out of loop to prevent issues
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance += Vector3.Distance(_currentPos, _nextPos);
                //Once total distance hits it's target, sets _routePoint to the last position checked
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    _routePoint = _nextPos;
                }
            }
        }
        //If parameter number is negative, searches behind, same as above, but modified to go backwards
        else if (p_distanceOfPointToGet < 0)
        {
            for (float i = 0; _totalDistance > p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                if (m_interpolateAmount - i >= 0)
                {
                    _nextPos = RoutePositions(m_interpolateAmount - i, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);
                }
                else if (m_interpolateAmount - i < 0 && i < 1f)
                {
                    if (m_currentRouteNum == 0)
                        _nextPos = RoutePositions((m_interpolateAmount - i) + 1f, m_routesArray.Length, m_currentLane, m_currentShortcutRoute);
                    else
                        _nextPos = RoutePositions((m_interpolateAmount - i) + 1f, m_currentRouteNum - 1, m_currentLane, m_currentShortcutRoute);
                }
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance -= Vector3.Distance(_currentPos, _nextPos);
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    _routePoint = _nextPos;
                }
            }
        }
        
        return _routePoint;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoutePointByDistance
    * Parameters: p_distanceOfPointToGet, p_interpolation, p_routeNum, p_laneNum, p_shortcutNum
    * Return: _routePoint
    * 
    * Author: Dean Pearce
    *
    * Description: Overload version of RoutePointByDistance, for getting the distance from a specified point
    *              instead of an object
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 16/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public Vector3 RoutePointByDistance(float p_distanceOfPointToGet, float p_interpolation, int p_routeNum, int p_laneNum, int p_shortcutNum)
    {
        float _totalDistance = 0f;
        float _incrementToCheckBy = 0.01f;
        Vector3 _currentPos = RoutePositions(p_interpolation, p_routeNum, p_laneNum, p_shortcutNum);
        Vector3 _nextPos = _currentPos;
        Vector3 _routePoint = _currentPos;

        if (p_distanceOfPointToGet >= 0)
        {
            for (float i = 0; _totalDistance < p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                if (p_interpolation + i <= 1f)
                    _nextPos = RoutePositions(p_interpolation + i, p_routeNum, p_laneNum, p_shortcutNum);
                else if (p_interpolation + i > 1f && i < 1f)
                    _nextPos = RoutePositions((p_interpolation + i) % 1f, (p_routeNum + 1) % m_routesArray.Length, p_laneNum, p_shortcutNum);
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance += Vector3.Distance(_currentPos, _nextPos);
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    _routePoint = _nextPos;
                }
            }
        }
        else if (p_distanceOfPointToGet < 0)
        {
            for (float i = 0; _totalDistance > p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                if (p_interpolation - i >= 0)
                {
                    _nextPos = RoutePositions(p_interpolation - i, p_routeNum, p_laneNum, p_shortcutNum);
                }
                else if (p_interpolation - i < 0 && i < 1f)
                {
                    if (p_routeNum == 0)
                        _nextPos = RoutePositions((p_interpolation - i) + 1f, m_routesArray.Length, p_laneNum, p_shortcutNum);
                    else
                        _nextPos = RoutePositions((p_interpolation - i) + 1f, p_routeNum - 1, p_laneNum, p_shortcutNum);
                }
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance -= Vector3.Distance(_currentPos, _nextPos);
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    _routePoint = _nextPos;
                }
            }
        }

        return _routePoint;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoutePointByDistance
    * Parameters: p_distanceOfPointToGet, p_interpolation, p_routeNum, p_laneNum, p_shortcutNum
    * Return: _routePoint
    * 
    * Author: Dean Pearce
    *
    * Description: Overload version of RoutePointByDistance, with ref parameters for the purpose of passing back interpolation and
    *              route num values so the position can be used by those values
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 16/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public Vector3 RoutePointByDistance(float p_distanceOfPointToGet, ref float p_interpolation, ref int p_routeNum, int p_laneNum, int p_shortcutNum)
    {
        float _totalDistance = 0f;
        float _incrementToCheckBy = 0.01f;
        Vector3 _currentPos = RoutePositions(p_interpolation, p_routeNum, p_laneNum, p_shortcutNum);
        Vector3 _nextPos = _currentPos;
        Vector3 _routePoint = _currentPos;

        if (p_distanceOfPointToGet >= 0)
        {
            for (float i = 0; _totalDistance < p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                if (p_interpolation + i <= 1f)
                    _nextPos = RoutePositions(p_interpolation + i, p_routeNum, p_laneNum, p_shortcutNum);
                else if (p_interpolation + i > 1f && i < 1f)
                    _nextPos = RoutePositions((p_interpolation + i) % 1f, (p_routeNum + 1) % m_routesArray.Length, p_laneNum, p_shortcutNum);
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance += Vector3.Distance(_currentPos, _nextPos);
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    _routePoint = _nextPos;
                    p_interpolation = (p_interpolation + i) % 1f;
                    if (p_interpolation + i > 1f && i < 1f)
                        p_routeNum = (p_routeNum + 1) % m_routesArray.Length;
                }
            }
        }
        else if (p_distanceOfPointToGet < 0)
        {
            for (float i = 0; _totalDistance > p_distanceOfPointToGet; i += _incrementToCheckBy)
            {
                if (p_interpolation - i >= 0)
                {
                    _nextPos = RoutePositions(p_interpolation - i, p_routeNum, p_laneNum, p_shortcutNum);
                }
                else if (p_interpolation - i < 0 && i < 1f)
                {
                    if (p_routeNum == 0)
                        _nextPos = RoutePositions((p_interpolation - i) + 1f, m_routesArray.Length - 1, p_laneNum, p_shortcutNum);
                    else
                        _nextPos = RoutePositions((p_interpolation - i) + 1f, p_routeNum - 1, p_laneNum, p_shortcutNum);
                }
                else if (i >= 1f)
                {
                    break;
                }

                _totalDistance -= Vector3.Distance(_currentPos, _nextPos);
                if (_totalDistance >= p_distanceOfPointToGet)
                {
                    if (p_interpolation - i < 0 && i < 1f)
                    {
                        if (p_routeNum == 0)
                            p_routeNum = m_routesArray.Length - 1;
                        else
                            p_routeNum = p_routeNum - 1;
                        p_interpolation = (p_interpolation - i) + 1f;
                    }
                    else
                        p_interpolation = p_interpolation - i;
                    _routePoint = _nextPos;
                }
            }
        }

        return _routePoint;
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Name: FireProjectile
    *
    * Author: Charlie Taylor
    *
    * Description: Firing a projectile script
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/08/2021    CT          1.00        -Initial Created/Moved from PlayerBehaviour
    * 18/08/2021    CT          1.01        -Added gameObject to parameters for initialise
    *                                       to stop projectile hitting the shooter
    **************************************************************************************/
    protected void FireProjectile()
	{
        GameObject _newProjectile = m_objectPool.GetProjectile();

        ProjectileBehaviour _projectileScript = _newProjectile.GetComponent<ProjectileBehaviour>();


        _projectileScript.Initialise(m_currentLane,
                                     m_currentRouteNum,
                                     m_interpolateAmount,
                                     m_currentRouteNumFollow,
                                     m_followTargetInterpolate,
                                     gameObject);


        _newProjectile.transform.position = transform.position;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetStarMode
    *
    * Author: Charlie Taylor
    *
    * Description: Setting player to be in Star mode, where they cannot be spun out
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/08/2021    CT          1.00        -Initial Created
    * 15/08/2021    CT          1.10        -Added Sound effect support
    **************************************************************************************/
    protected void SetStarMode()
	{

        m_secondaryAudioSource.Stop();
        m_secondaryAudioSource.clip = m_starTimeSound;
        m_secondaryAudioSource.Play();

        m_invulnerableTime = m_starTime;
        m_iTimeIterator = m_invulnerableTime;
        SetInvulnerable();
	}

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetStartValues
    *
    * Author: Dean Pearce
    *
    * Description: Function for allowing the RaceStartController to set the vehicles positions
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    * 16/08/2021    DP          1.10        -Removed references to follow interpolation
    **************************************************************************************/
    public void SetStartValues(float p_interpolationToSet, int p_routeToSet, int p_laneToSet)
    {
        m_interpolateAmount = p_interpolationToSet;
        m_currentRouteNum = p_routeToSet;
        m_currentLane = p_laneToSet;
    }

    /**************************************************************************************
    * Name: GetCurrentRoutePoint
    * 
    * Author: Dean Pearce
    * 
    * Description: 
    **************************************************************************************/
    public Vector3 GetCurrentRoutePoint()
    {
        return RoutePositions(m_interpolateAmount, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);
    }

    /**************************************************************************************
    * Name: SetRacePos
    * Parameters: p_posToSet
    * 
    * Author: Dean Pearce
    * 
    * Description: Set race position
    **************************************************************************************/
    public void SetRacePos(int p_posToSet)
    {
        m_currentPositionInRace = p_posToSet;
    }

    /**************************************************************************************
    * Name: GetLap
    * 
    * Author: Dean Pearce
    * 
    * Description: Get the current lap the racer is on
    **************************************************************************************/
    public int GetLap()
    {
        return m_currentLap;
    }

    /**************************************************************************************
    * Name: SetLap
    * 
    * Author: Dean Pearce
    * 
    * Description: Set the current lap the racer is on in the race start contorller
    **************************************************************************************/
    public void SetLap(int p_lapToSet)
    {
        m_currentLap = p_lapToSet;
    }

    /**************************************************************************************
    * Name: GetName
    * Parameters: p_posToSet
    * 
    * Author: Charlie Taylor
    * 
    * Description: Get the racer's name, for positioning and player checking
    **************************************************************************************/
    public string GetName()
    {
        return m_objName;
    }

    public int GetRouteNum()
    {
        return m_currentRouteNum;
    }
    
    public float GetCurrentInterpolation()
    {
        return m_interpolateAmount;
    }

    public Vector3 GetLookTargetPos()
    {
        return m_lookTargetPos;
    }
    
    public void SetLookTargetPos(Vector3 p_lookTargetPos)
    {
        m_lookTargetPos = p_lookTargetPos;
    }

    public bool GetScriptStartStatus()
    {
        return m_hasRunStart;
    }
}
