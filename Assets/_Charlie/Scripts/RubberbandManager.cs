using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: RubberbandManager
* 
* Author: Charlie Taylor
* 
* Description: A class that holds the values for rubberbanding. This is not static as
*              the values need to be editable in the inspector, so this script needs
*              to be attached to an object.
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 28/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class RubberbandManager : MonoBehaviour
{  
    //Infront of the player
    //Anything including and after zone A has Level 2 Debuff
    [SerializeField]
    [Tooltip("How far infront of the player to receive the 2nd stage of debuff")]
    public float m_zoneAMin = 100;

    //Anything between zone B Min and zone A Min is zone B and has Level 1 Debuff

    //How far, (until they hit next distance marker), until they get the first style Debuff
    [SerializeField]
    [Tooltip("How far infront of the player to receive the 1st stage of debuff")]
    public float m_zoneBMin = 50;

    //Anything between zone B Min and zone D Min is zone C and has no Boost

    //How far, (until they hit next distance marker), until they get the first style Boost
    [SerializeField]
    [Tooltip("How far behind the player to receive the 1st stage of boost")]
    public float m_zoneDMin = -50;

    //Anything between zone D Min and zone E Min is zone D and has Level 1 Boost

    //Behind the Player 
    //Anything including and after zone E has Level 2 Boost
    [SerializeField]
    [Tooltip("How far behind the player to receive the 2nd stage of boost")]
    public float m_zoneEMin = -100;


    //Speed Boosts/Debuffs.
    [SerializeField]
    [Tooltip("The Multiplier for the Middle zone distance (Behind and Ahead)")]
    public float m_stage1Boost = 2;
    [SerializeField]
    [Tooltip("The Multiplier for the furthest zone distance (Behind and Ahead)")]
    public float m_stage2Boost = 3;

}
