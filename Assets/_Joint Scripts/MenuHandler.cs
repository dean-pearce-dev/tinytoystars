using UnityEngine.Audio;
using UnityEngine;

/**************************************************************************************
* Type: Class
* Name: MenuHandler
*
* Author: Dean Pearce
*
* Description: Class for the menu scene, which enables/disables menu elements, and passes info
*              to the game controller class for scene transitions.
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 10/08/2021    DP          1.00        -Initial Created
**************************************************************************************/
public class MenuHandler : MonoBehaviour
{
    private GameObject[] m_menuScreens;
    private string m_levelToSelect;
    private string m_levelType;
    private GameController m_gameController;

    //Audio Mixers
    [Header("Sound Mixers for different sound types")]
    [SerializeField]
    private AudioMixer m_musicAudioMixer;
    [SerializeField]
    private AudioMixer m_carsAudioMixer;
    [SerializeField]
    private AudioMixer m_SFXAudioMixer;
    //Menu Indexes:
    //Main Menu = 0
    //Environment Select = 1
    //Bedroom Select = 2
    //Kitchen Select = 3
    //Vehicle Select = 4

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Start
    *
    * Author: Dean Pearce
    **************************************************************************************/
    void Start()
    {
        m_gameController = GameObject.Find("GameController").GetComponent<GameController>();
        m_menuScreens = new GameObject[GameObject.Find("Canvas").transform.childCount];
        for (int i = 0; i < m_menuScreens.Length; i++)
        {
            m_menuScreens[i] = GameObject.Find("Canvas").transform.GetChild(i).gameObject;
            m_menuScreens[i].SetActive(false);
        }
        m_menuScreens[0].SetActive(true);
        CheckReturnStatus();
    }

    /**************************************************************************************
    * Type: Function
    * Name: CheckReturnStatus
    *
    * Author: Dean Pearce
    *
    * Description: Function for checking whether the game has returned from a level by pressing retry
    *              Gets the level information if so, and enables the vehicle select screen
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void CheckReturnStatus()
    {
        if (m_gameController.GetMenuReturnStatus())
        {
            Debug.Log("VehicleSelect");
            DisableAllMenus();
            m_menuScreens[4].SetActive(true);
            m_levelToSelect = m_gameController.GetPreviousLevel();
            m_levelType = m_gameController.GetLevelType();
            m_gameController.SetMenuReturnStatus(false);
        }
    }


    /**************************************************************************************
    * Type: Function
    * 
    * Author: Dean Pearce
    * 
    * Description: Disable all the menu objects in the scene
    * 
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void DisableAllMenus()
    {
        for (int i = 0; i < m_menuScreens.Length; i++)
        {
            m_menuScreens[i].SetActive(false);
        }
    }

    /**************************************************************************************
    * Type: Functions
    * 
    * Author: Dean Pearce & Charlie Taylor
    * 
    * Description: All of these functions turn off all other menu screen objects, and then
    *              enable themselves, based on their index number
    **************************************************************************************/
    public void GoToMainMenu()
    {
        DisableAllMenus();
        m_menuScreens[0].SetActive(true);
    }

    public void GoToEnvironmentSelect()
    {
        DisableAllMenus();
        m_menuScreens[1].SetActive(true);
    }

    public void GoToBedroomSelect()
    {
        DisableAllMenus();
        m_menuScreens[2].SetActive(true);
    }

    public void GoToKitchenSelect()
    {
        DisableAllMenus();
        m_menuScreens[3].SetActive(true);
    }

    //Vehicle Select has some more to it, passing in what level type it will be travelling to
    public void GoToVehicleSelect(string p_levelStringToStore)
    {
        if (m_menuScreens[2].activeSelf)
            m_levelType = "Bedroom";
        else if (m_menuScreens[3].activeSelf)
            m_levelType = "Kitchen";
        DisableAllMenus();
        m_menuScreens[4].SetActive(true);
        m_levelToSelect = p_levelStringToStore;

    }

    //Function for returning from vehicle select, determines what screen it should return to
    public void GoBackFromVehicleSelect()
    {
        if (m_levelType.Contains("Bedroom"))
            GoToBedroomSelect();
        else if (m_levelType.Contains("Kitchen"))
            GoToKitchenSelect();
    }

    //Changed to private, as now pressing Play will GoToLoadingScreen which then calls this
    private void LoadLevel()
    {
        m_gameController.LevelTransition(m_levelToSelect, "Menu", "Level");
        m_gameController.SetRaceStatus(false);
    }

    public void GoToLoadingScreen()
    {
        DisableAllMenus();
        m_menuScreens[5].SetActive(true);
        //Load for 2 seconds.
        //I know this is not how loading screens work in reality, but it was a common request to show that something was happening when selecting levels and stuff
        Invoke("LoadLevel", 2f);
    }
    public void GoToCredits()
    {
        DisableAllMenus();
        m_menuScreens[6].SetActive(true);
    }
    public void GoToControls()
    {
        DisableAllMenus();
        m_menuScreens[7].SetActive(true);
    }
    public void GoToSettings()
    {
        DisableAllMenus();
        m_menuScreens[8].SetActive(true);
    }

    //Quit the application
    public void QuitGame()
    {
        Application.Quit();
    }

    /**************************************************************************************
    * Type: Functions
    * 
    * Author: Charlie Taylor/William Harding
    * 
    * Description: Functions to manage the different sound mixer volumes using sliders
    * 
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    CT/WH       1.00        -Initial Created
    **************************************************************************************/
    public void SetMusicVolume(float p_volume)
    {
        //Volume float is logorithmic as the volume mixer scale is also. It is *20 so it scales between 0 and -40 on the mixer, being full and mute volumes
        m_musicAudioMixer.SetFloat("Volume", Mathf.Log10(p_volume) * 20);
    }
    public void SetCarVolume(float p_volume)
    {
        //Volume float is logorithmic as the volume mixer scale is also. It is *20 so it scales between 0 and -40 on the mixer, being full and mute volumes
        m_carsAudioMixer.SetFloat("Volume", Mathf.Log10(p_volume) * 20);
    }
    public void SetSFXVolume(float p_volume)
    {
        //Volume float is logorithmic as the volume mixer scale is also. It is *20 so it scales between 0 and -40 on the mixer, being full and mute volumes
        m_SFXAudioMixer.SetFloat("Volume", Mathf.Log10(p_volume) * 20);
    }

}
