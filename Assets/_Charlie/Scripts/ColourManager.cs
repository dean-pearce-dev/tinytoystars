using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: ColourManager
* 
* Author: Charlie Taylor
* 
* Description: A controller for the colour of cars when they become invulnerable.
*              This is to allow for the designer to change the values in 1 place, 
*              rather than 6
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 18/08/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class ColourManager : MonoBehaviour
{
    //Colour percentage modifications
    [Header("Make sure to overidde changes to the prefab")]
    [Header("(This won't change colours in run time)")]
    [Header("---Colour Stuff---")]
    [Tooltip("Percentage of red pigment to add/subtract to/from the car to show that they are invulnerable")]
    [Range(-1, 1)]
    [SerializeField]
    public float m_red = 1;
    [Tooltip("Percentage of green pigment to add/subtract to/from the car to show that they are invulnerable")]
    [Range(-1, 1)]
    [SerializeField]
    public float m_green = 1;
    [Tooltip("Percentage of blue pigment to add/subtract to/from the car to show that they are invulnerable")]
    [Range(-1, 1)]
    [SerializeField]
    public float m_blue = 1;
}
