using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: PowerUp
* 
* Author: Charlie Taylor
* 
* Description: PowerUp class attached to power up pick ups.
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 02/08/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class PowerUp : MonoBehaviour
{
	[SerializeField]
	[Tooltip("What power this is. \"Star\" or \"Projectile\"")]
	private string m_power;

	private bool m_currentlyRemoved = false;

	private float m_timeIterator;
	private float m_timeToRespawn = 5f;

	/**************************************************************************************
	* Name: GetPowerUp
	* 
	* Author: Charlie Taylor
	* 
	* Description: Called by the car picking up the power up, returns this pickups power
	**************************************************************************************/
	public string GetPowerUp()
	{
		return m_power;
	}

	/**************************************************************************************
    * Type: Functions
    * 
    * Name: Update
    *
    * Author: Charlie Taylor
    **************************************************************************************/
	private void Update()
	{
		if (m_currentlyRemoved)
		{
			m_timeIterator -= 1 * Time.deltaTime;
		}
		else
		{
			//Animate, rotate, whatever
		}

		if (m_timeIterator <= 0)
		{
			Respawn();
		}
	}

	/**************************************************************************************
	* Type: Function
	* 
	* Name: RemovePickup
	* 
	* Author: Charlie Taylor
	* 
	* Description: Didn't want to fully dsetroy or disable item pick ups, as I wanted the 
	*			   timer to be local to the individual pickup, and if it was disabled, there
	*			   would need to be a remote object managing the time, so this was cleaner,
	*			   so "removing" the pickup is just disabling the mesh and box collider so
	*			   nothing can re collide with it
	*
	* Change Log:
	* Date          Initials    Version     Comments
	* ----------    --------    -------     ----------------------------------------------
	* 02/08/2021    CT          1.00        -Initial Created
	**************************************************************************************/
	public void RemovePickup()
	{
		//Disable Child Model
		GameObject _model = gameObject.transform.GetChild(0).gameObject;
		_model.SetActive(false);

		GetComponent<BoxCollider>().enabled = false;

		m_currentlyRemoved = true;

		m_timeIterator = m_timeToRespawn;
	}

	/**************************************************************************************
	* Type: Function
	* 
	* Name: Respawn
	* 
	* Author: Charlie Taylor
	* 
	* Description: Resets the mesh renderer and box collider to be active, whilst also 
	*			   resetting the timer
	*
	* Change Log:
	* Date          Initials    Version     Comments
	* ----------    --------    -------     ----------------------------------------------
	* 02/08/2021    CT          1.00        -Initial Created
	**************************************************************************************/
	private void Respawn()
	{
		m_currentlyRemoved = false;

		//Enable Child Model
		GameObject _model = gameObject.transform.GetChild(0).gameObject;
		_model.SetActive(true);
		GetComponent<BoxCollider>().enabled = true;
		m_timeIterator = m_timeToRespawn;
	}

}
