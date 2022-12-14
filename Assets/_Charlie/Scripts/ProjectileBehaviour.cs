using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: Projectile
*
* Author: Charlie Taylor
*
* Description: Parent Class for projectiles that the racers can fire
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 26/06/2021    CT          1.00        -Initial Created
* 08/07/2021    CT          1.10        -Made the projectile use the track movement
* 12/07/2021    CT          1.20        -Removed redundant functions
* 14/07/2021    CT          2.00        -Moved a lot of duplicate code used in
*                                       Movement into a higher parent class
**************************************************************************************/
public class ProjectileBehaviour : TrackFollower
{
    [SerializeField]
    [Tooltip("The speed of the projectile. (Assign in Prefab!)")]
    private float m_speed;
    //Object pooling control
    private ObjectPool m_pool;

    //"Destroy" timer
    [SerializeField]
    [Tooltip("How long in seconds after being fired until it is destroyed (Assign in Prefab!)")]
    private float m_destroyTime = 10;
    private float m_timer;


    //The Cosmetic of the projectile
    private GameObject m_projectileGFX;
    private Transform m_projectileTransform;

    private GameObject m_soundHolder;
    private AudioSource m_destroySoundSource;

    private bool m_isActive = true;

    private GameObject m_shooter;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private void Start()
    {
        //Use the inherited m_currentSpeed for all the inherited functions, but use a serialsied version with simpler name
        m_currentSpeed = m_speed;
        //get object pool
        m_pool = FindObjectOfType<ObjectPool>();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Initialise
    * Parameters: p_newLane, p_currentRouteNum, p_interpolateAmount,
    *             p_currentRouteNumFollow, p_followTargetInterpolate
    *
    * Author: Charlie Taylor
    *
    * Description: A function that is called after the object is activated in the object 
    *              pool. This is called rather than using OnEnable, as variables from the
    *              Player are needed.
    *              It sets up several values that puts the projectile in the correct place
    *              after being re activated
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 13/07/2021    CT          1.00        -Initial Created
    * 18/08/2021    CT          1.10        -Added gameObject of shooter to parameters to 
    *                                       stop projectile hitting the shooter
    **************************************************************************************/
    public void Initialise(int p_newLane,
                           int p_currentRouteNum,
                           float p_interpolateAmount,
                           int p_currentRouteNumFollow,
                           float p_followTargetInterpolate,
                           GameObject p_shooter)
    {
        m_shooter = p_shooter;
        //Game Object of the GFX
        m_projectileGFX = transform.GetChild(0).gameObject;
        //Transform of the GFX
        m_projectileTransform = m_projectileGFX.transform;

        m_soundHolder = transform.GetChild(1).gameObject;
        m_destroySoundSource = m_soundHolder.GetComponent<AudioSource>();

        m_isActive = true;
        m_projectileGFX.SetActive(true);

        m_currentRouteNum = p_currentRouteNum;

        m_interpolateAmount = p_interpolateAmount;

        m_currentRouteNumFollow = p_currentRouteNumFollow;

        m_followTargetInterpolate = p_followTargetInterpolate;

        m_currentLane = p_newLane;

        //Destroy timer set up
        m_timer = m_destroyTime;
        LaneSetup();
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Update
    * 
    * Description: The projectiles are pooled rather tahn destroyed, so on disable, call
    *              the relevent code for pooling this individual object
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 26/06/2021    CT          1.00        -Initial Created
    * 01/08/2021    CT          1.10        -Replaced calling ReturnProjectile in m_pool
    *                                       with disabling this object, as that function
    *                                       is called OnDisable, and this was a problem in
    *                                       a chain of events causing a bug that made the 
    *                                       same projectile get called twice in a row.
    * 15/08/2021    CT          2.00        -The code is the same, but wrapped in a check
    *                                       to see if m_isActive is true, as the script
    *                                       cannot be disabled to stop problems when
    *                                       "destroying" the projectile, but we can't have
    *                                       the projectile continuing to perform for a 
    *                                       second after it is meant to have been destroyed
    **************************************************************************************/
    private void Update()
    {
        if (m_isActive)
        {
            MoveObject(transform);

            RaycastOut();

            m_timer -= 1 * Time.deltaTime;

            if (m_timer <= 0)
            {
                m_timer = m_destroyTime;

                DestroyProjectile();
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RaycastOut
    *
    * Author: Charlie Taylor
    *
    * Description: Projectile's Raycast code, for collisions
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 27/06/2021    CT          1.00        -Initial Created
    * 05/07/2021    CT          2.00        -Rebuilt to use the lane based movement    
    * 12/07/2021    CT          2.01        -Changed _direction to use new target position
    * 13/07/2021    CT          2.02        -Fixed the switch case for Enemy to make them
    *                                       look for the correct script, and also mmade
    *                                       the game object disable itself and use code in
    *                                       OnDisable rather than calling the object pool
    *                                       code in the switch itself
    * 26/07/2021    CT          2.10        -Collisions now check for the Carbehaviour 
    *                                       script against a Car tage rather than seperate
    *                                       Enemy and Player tags
    * 15/08/2021    CT          2.20        -Replace diabling the projectile with a function
    *                                       that does it, but also helps with audio.
    * 18/08/2021    CT          2.30        -Added check to see if the object being collided
    *                                       with is the shooter of the projectile, and if
    *                                       so, don't do the collision stuff
    **************************************************************************************/
    private void RaycastOut()
    {
        Vector3 _projectileTransform = m_projectileTransform.position;
        //Direction
        Vector3 _direction = m_lookTargetPos - transform.position;

        //Draw debug ray of collision length
        Debug.DrawRay(_projectileTransform, _direction * m_currentSpeed * Time.deltaTime, Color.yellow);


        RaycastHit _collision;
        //Send out a raycast from the centre point and send it out as far as the projectile's speed, so that it won't go through anything
        if (Physics.Raycast(_projectileTransform, _direction, out _collision, m_currentSpeed * Time.deltaTime))
        {
            //Store the object it detects first
            GameObject _collidedObject = _collision.transform.gameObject;

            //If not the shooter of the projectile
            if (_collidedObject != m_shooter)
			{

                switch (_collidedObject.tag)
                {
                    case "Hazard":
                        DestroyProjectile();
                        break;

                    case "Car":
                        //If colliding with a car, run the spin-out script on that car
                        //Store the script in a temp variable

                        //Check if it is an Enemy first
                        CarBehaviour _script = _collidedObject.GetComponent<CarBehaviour>();

                        if (_script == null)
                        {
                            _script = _collidedObject.GetComponent<PlayerBehaviour>();

                        }
                        //Run the Function remotely
                        _script.SpinOut();
                        //Then Disable self
                        DestroyProjectile();
                        break;
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: DestroyProjectile
    *
    * Author: Charlie Taylor
    *
    * Description: To allow the projectile to play sounds after it has meant to have been
    *              destroyed, it is now not immedietely disabled, and instead disables the
    *              GFX of the object, and sets a bool to false to tell Update to stop
    *              running, therfore making it not do anything, but not disables (but then
    *              after the sounds have finsihed playing, actually disable it via an Invoke
    *              of a different function that just disables it
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 15/08/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    private void DestroyProjectile()
	{
        //Stops Update happening
        m_isActive = false;
        //Disable the visual part of the container
        transform.GetChild(0).gameObject.SetActive(false);
        //Play break sound
        m_destroySoundSource.Play();
        //A good second to wait to allow the sound to finish before disabling it.
        Invoke("DisableProjectile", 1f);
	}


    /**************************************************************************************
    * Type: Function
    * 
    * Name: DisableProjectile
    *
    * Author: Charlie Taylor
    * 
    * Description: To be invoked by DestroyProjectile, set active needed to be in a function.
    *              It's probably not the cleanese approach but it works
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 14/07/2021    CT          1.00        -Initial Created
    * 15/08/2021    CT          2.00        -Due to needing to Invoke() disabling the object,
    *                                       the call to disable was it's own function, that
    *                                       when called, would call OnDisable. These 2
    *                                       functions were made into 1 to clean up the 
    *                                       flow
    **************************************************************************************/
    private void DisableProjectile()
    {

        if (m_pool != null)
        {
            m_pool.ReturnProjectile(gameObject);
        }
        gameObject.SetActive(false);
    }
}
