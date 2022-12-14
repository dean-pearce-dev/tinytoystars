using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: EndScreenManager
*
* Author: Charlie Taylor
*
* Description: Class to manage the end of the screen values and appearence
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 09/08/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class EndScreenManager : MonoBehaviour
{
    [Header("The images for the stars on End Screen")]
    [SerializeField]
    private Sprite m_3Stars;
    [SerializeField]
    private Sprite m_2Stars;
    [SerializeField]
    private Sprite m_1Star;
    [SerializeField]
    private Sprite m_0Stars;
    [SerializeField]
    [Header("Level failure image (Level success is default)")]
    private Sprite m_levelFailTitle;

    private GameController m_gameController;

    private Button m_retryButton;
    private Button m_continueButton;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetupButtons
    * 
    * Author: Dean Pearce
    * 
    * Description: Set up the buttons on the menu
    * 
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public void SetupButtons()
    {
        m_gameController = GameObject.Find("GameController").GetComponent<GameController>();
        m_retryButton = GameObject.Find("RetryButton").GetComponent<Button>();
        m_continueButton = GameObject.Find("ContinueButton").GetComponent<Button>();
        m_retryButton.onClick.AddListener(m_gameController.GoToVehicleSelect);
        m_continueButton.onClick.AddListener(m_gameController.GoBackToMenu);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SetPlace
    * Parameters: p_finalPosition
    * 
    * Author: Dean Pearce
    * 
    * Description: Set the place of the player and change menu based on that value
    * 
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public void SetPlace(int p_finalPosition)
    {
        Image _title = GameObject.Find("Title").GetComponent<Image>();
        Image _stars = GameObject.Find("Stars").GetComponent<Image>();

        switch (p_finalPosition)
        {
            case 1:
                _stars.sprite = m_3Stars;
                break;

            case 2:
                _stars.sprite = m_2Stars;
                break;

            case 3:
                _stars.sprite = m_1Star;
                break;

            default:
                _title.sprite = m_levelFailTitle;
                _stars.sprite = m_0Stars;
                break;

        }
    }
}