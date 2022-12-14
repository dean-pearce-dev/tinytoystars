using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: CarSelectManager
* 
* Author: Charlie Taylor
* 
* Description: A class for managing car selection when on the right menu screen for it
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* ??/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class CarSelectManager : MonoBehaviour
{
    private string[] m_sceneInfo;
    private string m_playerCar;


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
    [Tooltip("Debug for playing straight into a Level")]
    private CarNames m_carName;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Awake
    *
    * Author: Charlie Taylor
    **************************************************************************************/
    private void Awake()
	{
        SetPlayer(m_carName.ToString());
	}

    /**************************************************************************************
    * Name: SetSceneInfo
    * Parameters: p_targetScene, p_currentSceneType, p_targetSceneType
    * 
    * Author: Charlie Taylor
    * 
    * Description: Create an array for the scene info, for when LevelTransitioning
    **************************************************************************************/
    public void SetSceneInfo(string p_targetScene, string p_currentSceneType, string p_targetSceneType)
    {
        m_sceneInfo = new string[] { p_targetScene, p_currentSceneType, p_targetSceneType };
    }

    /**************************************************************************************
    * Name: GetSceneInfo
    * 
    * Author: Charlie Taylor
    * 
    * Description: Get the array for the scene info, for when LevelTransitioning
    **************************************************************************************/
    public string[] GetSceneInfo() 
    {
        return m_sceneInfo;
    }

    /**************************************************************************************
    * Name: SetPlayer
    * 
    * Author: Charlie Taylor
    * 
    * Description: Set the car name, so it can be allocated correctly in the level
    **************************************************************************************/
    public void SetPlayer(string p_carName)
    {
        m_playerCar = p_carName;
    }    
    /**************************************************************************************
    * Name: GetPlayer
    * 
    * Author: Charlie Taylor
    * 
    * Description: Get the car namet to be allocated correctly in the level
    **************************************************************************************/
    public string GetPlayer()
    {
        return m_playerCar;
    }
}
