using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: RaceStartController
*
* Author: Dean Pearce
*
* Description: Simple class for tracking whether the fly by or race countdown has been completed.
*              Uses 2 bools with getters and setters. Has a SetPositions function to set the cars into a starting grid.
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 08/08/2021    DP          1.00        -Initial Created
**************************************************************************************/

public class RaceStartController : MonoBehaviour
{
    private bool m_flyByCameraFinished = false;
    private bool m_raceCountdownFinished = false;
    private GameObject[] m_vehiclesArray;
    private Vector3[] m_startingGrid;
    private int m_totalRoutes;
    private float m_distanceToSetBack = -25f;
    private FlyByCamera m_flyByCam;

    public bool GetFlyByStatus()
    {
        return m_flyByCameraFinished;
    }

    public void SetFlyByStatus(bool p_flyByCameraFinished)
    {
        m_flyByCameraFinished = p_flyByCameraFinished;
    }

    public void SetFlyByCam(FlyByCamera p_flyByCam)
    {
        m_flyByCam = p_flyByCam;
    }

    public bool GetCountdownStatus()
    {
        return m_raceCountdownFinished;
    }

    public void SetCountdownStatus(bool p_raceCountdownFinished)
    {
        m_raceCountdownFinished = p_raceCountdownFinished;
    }

    /**************************************************************************************
    * Type: Function
    * Name: SetPositions
    *
    * Author: Dean Pearce
    *
    * Description: Function to set the start positions of all cars. Is called before the fly by cam runs, so the
    *              camera can see the cars on the track. Setup is in a 2 by 3 grid formation.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 10/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    public void SetPositions()
    {
        m_totalRoutes = GameObject.Find("Routes").transform.childCount;
        m_vehiclesArray = new GameObject[GameObject.Find("Vehicles").transform.childCount];
        m_startingGrid = new Vector3[m_vehiclesArray.Length];
        for (int i = 0; i < m_vehiclesArray.Length; i++)
        {
            m_vehiclesArray[i] = GameObject.Find("Vehicles").transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < m_startingGrid.Length; i++)
        {
            CarBehaviour _currentScript = m_vehiclesArray[i].GetComponent<CarBehaviour>();

            //Checks if the vehicle behaviour script has finished it's start function
            //Will keep returning out until all have finished
            if (!_currentScript.GetScriptStartStatus())
                return;
            
            //Temp variables to use for getting and setting vehicles on the routes
            float _targetInterp = 0;
            int _targetRouteNum = 0;

            //Starting 3 cars on the starting line
            if (i < 3)
            {
                m_startingGrid[i] = _currentScript.RoutePositions(0, 0, i + 1, 0);
                _currentScript.SetStartValues(0, 0, i + 1);
                _currentScript.SetLookTargetPos(_currentScript.RoutePointByDistance(10f));
            }
            //Starting 3 cars by a set distance behind the first 3
            else if (i > 2)
            {
                m_startingGrid[i] = _currentScript.RoutePointByDistance(m_distanceToSetBack, ref _targetInterp, ref _targetRouteNum, i - 2, 0);
                _currentScript.SetStartValues(_targetInterp, _targetRouteNum, i - 2);
                //Setting lap to 0 because the cars start behind the finish line, so passing the line will increase the lap count
                _currentScript.SetLap(0);
                _currentScript.SetLookTargetPos(_currentScript.RoutePointByDistance(10f));
            }

            m_vehiclesArray[i].transform.position = m_startingGrid[i];

        }

        //Telling the fly by camera that the cars are now setup
        m_flyByCam.SetCarPositionStatus(true);
    }
}
