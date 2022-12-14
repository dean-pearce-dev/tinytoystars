using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: AudioManager
*
* Author: Charlie Taylor
*
* Description: Class that contains all the audio in one place. 
               Public varaiable as having a getter for every single sound would be a bad idea
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 13/08/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class AudioManager : MonoBehaviour
{
    [Header("Music sounds")]
    [Tooltip("Music for menus")]
    public AudioClip m_menuMusic;
    [Tooltip("Music for Levels")]
    public AudioClip m_levelMusic;

    [Header("Pick up sounds")]
    [Tooltip("Sound for when picking up power ups")]
    public AudioClip m_pickUp;
    [Tooltip("Sound for when activating star time")]
    public AudioClip m_starTime;

    [Header("Projectile Sounds")]
    [Tooltip("Sound for when firng the marble")]
    public AudioClip m_shootMarble;
    [Tooltip("Sound for when marble breaks")]
    public AudioClip m_breakProjectile;

    [Header("Spinout")]
    [Tooltip("Sound for when racers spin out")]
    public AudioClip m_skid;
}
