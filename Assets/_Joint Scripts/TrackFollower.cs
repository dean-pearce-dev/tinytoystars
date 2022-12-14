using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**************************************************************************************
* Type: Class
* 
* Name: TrackFollower
*
* Author: Charlie Taylor
*
* Description: Parent Class for any object that follows the track
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 14/07/2021    CT          1.00        -Initial Created
**************************************************************************************/
public class TrackFollower : MonoBehaviour
{
    //The speed the object is going at that moment
    protected float m_currentSpeed;

    //Route Variables
    protected GameObject m_routeHolder;
    protected GameObject[] m_routesArray;
    protected int m_currentRouteNum = 0;
    protected int m_currentRouteNumFollow = 0;
    
    protected int m_currentLane;

    //Position to look at (for turning and lane switching)
    protected Vector3 m_lookTargetPos;

    protected float m_lookTargetDistance = 10f;

    //Lane Switch Variables
    //Distance Variables
    protected float m_interpolateAmount;
    protected float m_followTargetInterpolate = 0.1f;
    protected float m_distanceCheckIncrement = 0.1f;

    //Animation Controller, if necessary
    protected Animator m_animController;


    //Shortcut Variables
    protected int m_currentShortcutRoute = 0;
    protected int m_currentShortcutRouteFollow = 0;

	/**************************************************************************************
    * Type: Function
    * 
    * Name: QuadraticLerp
    * Parameters: p_pointA, p_pointB, p_pointC, p_interpolateValue
    * Return: _lerpedVector
    *
    * Author: Dean Pearce
    *
    * Description: Linear interpolation function to be used by CubicLerp function to calculate the curve of a turn.
    * From https://www.youtube.com/watch?v=7j_BNf9s0jM
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    DP          1.10        -Added temp variable for return
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    **************************************************************************************/
	protected Vector3 QuadraticLerp(Vector3 p_pointA, Vector3 p_pointB, Vector3 p_pointC, float p_interpolateValue)
    {
        Vector3 _lerpedVector;
        Vector3 _pointAB = Vector3.Lerp(p_pointA, p_pointB, p_interpolateValue);
        Vector3 _pointBC = Vector3.Lerp(p_pointB, p_pointC, p_interpolateValue);
        _lerpedVector = Vector3.Lerp(_pointAB, _pointBC, p_interpolateValue);

        return _lerpedVector;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: CubicLerp
    * Parameters: p_pointA, p_pointB, p_pointC, p_pointD, p_interpolateValue
    * Return: _lerpedVector
    *
    * Author: Dean Pearce
    *
    * Description: Linear interpolation between four points to calculate a curve.
    * From https://www.youtube.com/watch?v=7j_BNf9s0jM
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    DP          1.10        -Added temp variable for return
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    **************************************************************************************/
    protected Vector3 CubicLerp(Vector3 p_pointA, Vector3 p_pointB, Vector3 p_pointC, Vector3 p_pointD, float p_interpolateValue)
    {
        Vector3 _lerpedVector;
        Vector3 _pointAB_BC = QuadraticLerp(p_pointA, p_pointB, p_pointC, p_interpolateValue);
        Vector3 _pointBC_CD = QuadraticLerp(p_pointB, p_pointC, p_pointD, p_interpolateValue);
        _lerpedVector = Vector3.Lerp(_pointAB_BC, _pointBC_CD, p_interpolateValue);

        return _lerpedVector;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoutePositions
    * Parameters: p_interpolateValue, p_currentRouteNum, p_targetLane,int p_shortcutRouteNum
    * Return: _routePos
    *
    * Author: Dean Pearce
    *
    * Description: Method to calculate route points to cut down on duplicate code
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 13/07/2021    DP          1.10        -Changed out _emptyVector for _routePos and 
    *                                       removed the return for each condition, instead 
    *                                       assigning the value to _routePos, and 
    *                                       returning at the end
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    * 14/07/2021    DP          2.10        -Added logic for shortcut routes/4 point 
    *                                       routes with no curve.
    * 04/08/2021    DP          2.20        -Added extra tag conditionals to check if
    *                                       a 4 point route should have no curve
    **************************************************************************************/
    public Vector3 RoutePositions(float p_interpolateValue, int p_currentRouteNum, int p_targetLane, int p_shortcutRouteNum)
    {
        Vector3 _routePos = new Vector3(0, 0, 0);

        //If a route has 4 children, it's a turn route(unless specified by tag), which requires CubicLerp
        if (m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).transform.childCount == 4)
        {
            //NoCurve tags for shortcut functionality
            if (m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).tag == "NoCurve" || 
                m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).tag == "NoCurveNoChangeL" ||
                m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).tag == "NoCurveNoChangeR")
            {
                //Lerping between the 4-point no curve routes
                if (p_shortcutRouteNum == 0)
                    _routePos = Vector3.Lerp(m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(0).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(1).position, p_interpolateValue);
                else if (p_shortcutRouteNum == 1)
                    _routePos = Vector3.Lerp(m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(1).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(2).position, p_interpolateValue);
                else if (p_shortcutRouteNum == 2)
                    _routePos = Vector3.Lerp(m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(2).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(3).position, p_interpolateValue);
            }
            else
                _routePos = CubicLerp(m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(0).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(1).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(2).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(3).position, p_interpolateValue);
        }
        //If a route has 2 children, it's a straight, so Lerp is used
        else if (m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).transform.childCount == 2)
            _routePos = Vector3.Lerp(m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(0).position, m_routesArray[p_currentRouteNum].transform.GetChild(p_targetLane).GetChild(1).position, p_interpolateValue);

        return _routePos;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TotalDistanceOfSegment
    * Parameters: p_currentRouteNum
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Method to get distance of the current segment that the player is 
    *              traversing. Used to keep speed constant over interpolation
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    **************************************************************************************/

    protected float TotalDistanceOfSegment(int p_currentRouteNum)
    {
        float _cumulativeDistance = 0;
        Vector3 _nextPoint;
        Vector3 _currentPoint = RoutePositions(0, p_currentRouteNum, m_currentLane, 0);

        //Interpolates through the segment to get the total distance
        for (float i = 0f; i < 1f; i += m_distanceCheckIncrement)
        {
            _nextPoint = RoutePositions(i, p_currentRouteNum, m_currentLane, 0);
            _cumulativeDistance += Vector3.Distance(_currentPoint, _nextPoint);
            _currentPoint = _nextPoint;
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: TotalDistanceOfSegment
    * Parameters: p_currentRouteNum, p_shortcutRouteNum
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Method to get distance of the current segment that the player is 
    *              traversing. Used to keep speed constant over interpolation. Overload version
    *              of the above function used for routes with 4 points but no curve.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 14/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    protected float TotalDistanceOfSegment(int p_currentRouteNum, int p_shortcutRouteNum)
    {
        float _cumulativeDistance = 0;
        Vector3 _nextPoint;
        Vector3 _currentPoint = RoutePositions(0, p_currentRouteNum, m_currentLane, p_shortcutRouteNum);
        for (float i = 0f; i < 1f; i += m_distanceCheckIncrement)
        {
            _nextPoint = RoutePositions(i, p_currentRouteNum, m_currentLane, p_shortcutRouteNum);
            _cumulativeDistance += Vector3.Distance(_currentPoint, _nextPoint);
            _currentPoint = _nextPoint;
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RemainingDistanceOfSegment
    * Parameters: p_currentRouteNum, p_currentInterpolation
    * Return: _cumulativeDistance
    *
    * Author: Dean Pearce
    *
    * Description: Method to get remaining distance of the current segment that the object is 
    *              traversing.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 24/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/

    protected float RemainingDistanceOfSegment(int p_currentRouteNum, float p_currentInterpolation)
    {
        float _cumulativeDistance = 0;
        Vector3 _nextPoint;
        Vector3 _currentPoint = RoutePositions(p_currentInterpolation, p_currentRouteNum, m_currentLane, 0);
        
        //Interpolates from given interpolation to the end of route to get the total distance
        //from that point
        for (float i = p_currentInterpolation; i < 1f; i += m_distanceCheckIncrement)
        {
            _nextPoint = RoutePositions(i, p_currentRouteNum, m_currentLane, 0);
            _cumulativeDistance += Vector3.Distance(_currentPoint, _nextPoint);
            _currentPoint = _nextPoint;
        }
        return _cumulativeDistance;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MoveObject
    * Parameters: p_moveObject
    *
    * Author: Dean Pearce
    *
    * Description: Moves the given object, and calculates distance of current segment so that interpolation speed is consistent despite varying distances
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 12/07/2021    DP          1.10        -Changed LookAt() to use a Vector3 variable 
    *                                       instead of a transform position
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    * 14/07/2021    DP          2.10        -Added condition to make use of overload 
    *                                       version of TotalDistanceOfSegment() for 
    *                                       shortcuts/4 point no curves
    * 24/07/2021    DP          2.20        -Made function virtual so it can be overriden in
    *                                       CarBehaviour class
    * 04/08/2021    DP          2.30        -Added extra tag conditionals to check if a
    *                                       4 point route should have no curve
    **************************************************************************************/
    protected virtual void MoveObject(Transform p_moveTarget)
    {
        float _distanceOfSegment;

        //Checking for NoCurve tag to use the correct function for distance calculation
        if (m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurve" || 
            m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeL" ||
            m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeR")
            _distanceOfSegment = TotalDistanceOfSegment(m_currentRouteNum, m_currentShortcutRoute);
        else
            _distanceOfSegment = TotalDistanceOfSegment(m_currentRouteNum);

        //Using distance to make the speed even on routes of varying distances
        m_interpolateAmount += (m_currentSpeed * Time.deltaTime) / _distanceOfSegment;

        p_moveTarget.LookAt(m_lookTargetPos);
        p_moveTarget.position = RoutePositions(m_interpolateAmount, m_currentRouteNum, m_currentLane, m_currentShortcutRoute);

        //Once interpolation hits 1, sets values for the next route
        if (m_interpolateAmount >= 1)
        {
            if (m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurve" || 
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeL" ||
                m_routesArray[m_currentRouteNum].transform.GetChild(m_currentLane).tag == "NoCurveNoChangeR")
            {
                m_currentShortcutRoute++;
                if (m_currentShortcutRoute > 2)
                {
                    m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;
                    m_currentShortcutRoute = 0;
                }
            }
            else
                m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;

            m_interpolateAmount = 0f;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: LaneSetup
    *
    * Author: Dean Pearce
    *
    * Description: Function for getting total routes in the current scene and adding them
    *              to an array
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 14/07/2021    CT          2.00        -Moved into a higher level of inheritence
    **************************************************************************************/
    protected void LaneSetup()
    {
        m_routeHolder = GameObject.Find("Routes");
        m_routesArray = new GameObject[m_routeHolder.transform.childCount];

        for (int i = 0; i < m_routesArray.Length; i++)
        {
            m_routesArray[i] = m_routeHolder.transform.GetChild(i).gameObject;
        }
    }
}
