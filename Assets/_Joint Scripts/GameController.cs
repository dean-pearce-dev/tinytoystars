using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**************************************************************************************
* Type: Class
* 
* Name: GameController
*
* Author: Charlie Taylor
*
* Description: A game manager that begins at the start of the game and never destorys
*              itself.
*              Manages Audio and level transitions
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 28/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class GameController : MonoBehaviour
{
    private static GameController g_currentInstance;

    private AudioSource m_audioSource;

    private AudioClip m_menuMusic;
    private AudioClip m_levelMusic;

    private string m_previousLevel;

    //Array for placement at end of race
    private int m_finalPlace;

    private GameObject m_endScreen;
    private bool m_isRaceFinished = false;
    private bool m_hasReturnedFromLevel = false;

    private string m_levelType;

    /**************************************************************************************
    * Type: Function
    * 
    * Name: Awake
    *
    * Author: Charlie Taylor & Dean Pearce
    **************************************************************************************/
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (g_currentInstance == null)
            g_currentInstance = this;
        else
            Destroy(gameObject);

        m_audioSource = GetComponent<AudioSource>();

        //Before
        SetUpMusic();

    }


    /**************************************************************************************
    * Type: Class
    * 
    * Name: SetUpMusic
    *
    * Author: Charlie Taylor
    *
    * Description: Sets up music tracks in the gamecontroller based on the audio manager
    *              script
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 15/08/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    private void SetUpMusic()
	{
        AudioManager _audioManager = gameObject.GetComponent<AudioManager>();
        m_menuMusic = _audioManager.m_menuMusic;
        m_levelMusic = _audioManager.m_levelMusic;


        m_audioSource.clip = m_menuMusic;
        m_audioSource.Play();
    }

    /**************************************************************************************
    * Type: Class
    * 
    * Name: ChangeMusic
    * Parameters: p_music
    *
    * Author: Charlie Taylor
    *
    * Description: Change the music being played, by stopping the old one, reasigning to the
    *              parameter, then play again
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/07/2021    CT          1.00        -Initial Created
    **************************************************************************************/
    public void ChangeMusic(AudioClip p_music)
    {
        m_audioSource.Stop();
        m_audioSource.clip = p_music;
        m_audioSource.Play();
    }

    /**************************************************************************************
    * Type: Class
    * 
    * Name: LevelTransition
    * Parameters: p_targetScene, p_currentSceneType, p_targetSceneType
    *
    * Author: Charlie Taylor
    *
    * Description: Transition to a new scene using the p_targetScene value, but to control
    *              music, pass in p_currentSceneType and p_targetSceneType to check if
    *              the music needs resetting
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/07/2021    CT          1.00        -Initial Created
    * 03/08/2021    CT          2.00        -Completely redesigned the way the script works,
    *                                       no longer using Arrays of scene names
    * 15/08/2021    CT          2.10        -Removed specific environment track options
    **************************************************************************************/
    public void LevelTransition(string p_targetScene, string p_currentSceneType, string p_targetSceneType)
    {
        //Menu by default. Needed to stop Complier errors
        AudioClip _targetRoomMusic = m_menuMusic;

        switch (p_targetSceneType)
        {
            case "Menu":
                _targetRoomMusic = m_menuMusic;
                break;

            case "Level":
                _targetRoomMusic = m_levelMusic;
                break;
        }

        //If the scene type is not the same, reset the music
        if (p_targetSceneType != p_currentSceneType)
        {
            ChangeMusic(_targetRoomMusic);
        }

        SceneManager.LoadScene(p_targetScene);
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CompleteLevel
    *
    * Author: Charlie Taylor
    *
    * Description: Does everything needed for the end of a level.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 28/07/2021    CT          1.00        -Initial Created
    * ??/08/2021    DP          1.10        -Stopped level transition and added CarSelectManager
    *                                       component
    * 15/07/2021    CT          1.20        -stop all car sounds at the end of the race
    **************************************************************************************/
    public void CompleteLevel()
    {
        m_isRaceFinished = true;
        m_endScreen.SetActive(true);
        EndScreenManager _endScript = m_endScreen.GetComponent<EndScreenManager>();
        _endScript.SetupButtons();
        _endScript.SetPlace(m_finalPlace);
        gameObject.AddComponent<CarSelectManager>();

        //Mute all car driving sounds.

        GameObject[] _racers = GameObject.FindGameObjectsWithTag("Car");
        for (int i = 0; i < _racers.Length; i++)
		{
            //Stop Driving/tire skid
            AudioSource _coreSource = _racers[i].GetComponent<AudioSource>();
            _coreSource.Stop();
            //Stop Secondary Sounds
            AudioSource _secondSource = _racers[i].transform.Find("SecondarySounds").gameObject.GetComponent<AudioSource>();
            _secondSource.Stop();

        }
    }

    /**************************************************************************************
    * Name: SetFinalPosition
    * 
    * Author: Charlie Taylor
    * 
    * Description: Sets final position of player, for the end screen, set at end of race
    **************************************************************************************/
    public void SetFinalPosition(int p_position)
	{
        //+1 as p_posiition passes in array elements, so 1st is 0
        m_finalPlace = p_position + 1;
    }

    /**************************************************************************************
    * Name: GoBackToMenu
    * 
    * Author: Dean Pearce
    * 
    * Description: Return to main Menu Scene
    **************************************************************************************/
    public void GoBackToMenu()
    {
        LevelTransition("MainMenu", "Level", "Menu");
    }

    /**************************************************************************************
    * Name: GoToVehicleSelect
    * 
    * Author: Dean Pearce
    * 
    * Description: Return to main Menu Scene but with a bool that cause it to go to vehicle
    *              select
    **************************************************************************************/
    public void GoToVehicleSelect()
    {
        m_hasReturnedFromLevel = true;
        if (SceneManager.GetActiveScene().name == "Level 1" || SceneManager.GetActiveScene().name == "Level 2")
            m_levelType = "Bedroom";
        else if (SceneManager.GetActiveScene().name == "Level 3")
            m_levelType = "Kitchen";
        LevelTransition("MainMenu", "Level", "Menu");
    }

    /**************************************************************************************
    * Name: SetPreviousLevel
    * 
    * Author: Charlie Taylor
    * 
    * Description: Set member variable to a parameter sent in by player behaviour
    **************************************************************************************/
    public void SetPreviousLevel(string p_previousLevel)
    {
        m_previousLevel = p_previousLevel;
    }

    /**************************************************************************************
    * Name: GetPreviousLevel
    * 
    * Author: Charlie Taylor
    * 
    * Description: Return member variable to EndSceenManager for replay level purposes
    **************************************************************************************/
    public string GetPreviousLevel()
    {
        return m_previousLevel;
    }

    /**************************************************************************************
    * Name: SetEndScreen
    * 
    * Author: Dean Pearce
    * 
    * Description: Set the reference to the end screen
    **************************************************************************************/
    public void SetEndScreen(GameObject p_endScreen)
    {
        m_endScreen = p_endScreen;
    }

    /**************************************************************************************
    * Name: GetRaceStatus
    * 
    * Author: Dean Pearce
    * 
    * Description: Get race status, so as to stop car behaviour and such
    **************************************************************************************/
    public bool GetRaceStatus()
    {
        return m_isRaceFinished;
    }
    
    /**************************************************************************************
    * Name: SetRaceStatus
    * 
    * Author: Dean Pearce
    * 
    * Description: Set race status, so race can be set to finished, which is used to stop 
    *              the vehicles while the end screen is active
    **************************************************************************************/
    public void SetRaceStatus(bool p_raceStatus)
    {
        m_isRaceFinished = p_raceStatus;
    }

    /**************************************************************************************
    * Name: GetMenuReturnStatus
    * 
    * Author: Dean Pearce
    * 
    * Description: Has the player returned from a level?
    **************************************************************************************/
    public bool GetMenuReturnStatus()
    {
        return m_hasReturnedFromLevel;
    }

    /**************************************************************************************
    * Name: SetMenuReturnStatus
    * 
    * Author: Dean Pearce
    * 
    * Description: Set the returned from level bool so the menu can check which menu to set active
    *              upon returning by using retry
    **************************************************************************************/
    public void SetMenuReturnStatus(bool p_hasReturnedFromLevel)
    {
        m_hasReturnedFromLevel = p_hasReturnedFromLevel;
    }

    /**************************************************************************************
    * Name: GetLevelType
    * 
    * Author: Dean Pearce
    * 
    * Description: Get the level type string to pass to the menu for retry
    **************************************************************************************/
    public string GetLevelType()
    {
        return m_levelType;
    }
}
