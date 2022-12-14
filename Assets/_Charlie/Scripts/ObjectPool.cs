using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: ObjectPool
*
* Author: Charlie Taylor
*
* Description: ObjectPool class, for projectiles. Unlikely to be anything more
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 12/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The prefab of the projectile container object")]
    private GameObject m_projectilePrefab;

    [SerializeField]
    [Tooltip("The prefab of the ObjectPool object")]
    private Queue<GameObject> m_pool = new Queue<GameObject>();


    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetProjectile
    *
    * Author: Charlie Taylor
    *
    * Description: Used in car's calls for projectiles, this returns the projectile to be
    *              fired from the Pool
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 12/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public GameObject GetProjectile()
	{
        GameObject _projectile;
        if (m_pool.Count > 0)
		{
            _projectile = m_pool.Dequeue();

            _projectile.SetActive(true);
        }
        else
        {
            _projectile = Instantiate(m_projectilePrefab);
        }

        return _projectile;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: ReturnProjectile
    *
    * Author: Charlie Taylor
    *
    * Description: Returns the projectile to the queue
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 12/07/2021    CT          1.00        -Initial Created
    * 01/08/2021    CT          1.10        -Removed the call to disable the projectile
    *                                       here as this is called OnDisable in that instance
    *                                       and was part of a chain of events causing a bug
    *                                       that made the same projectile get called twice
    *                                       in a row.
    **************************************************************************************/
    public void ReturnProjectile (GameObject p_projectile)
	{
        m_pool.Enqueue(p_projectile);
	}
}
