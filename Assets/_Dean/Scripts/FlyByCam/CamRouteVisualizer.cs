﻿using UnityEngine;

/**************************************************************************************
* Type: Class
* Name: CamRouteVisualizer
*
* Author: Dean Pearce
*
* Description: Class for showing the route that the fly by camera will travel, runs in edit mode
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 02/08/2021    DP          1.00        -Initial Created
**************************************************************************************/

[ExecuteInEditMode]
public class CamRouteVisualizer : MonoBehaviour
{
    private Transform[][] m_routePoint;
    private Vector3 m_gizmosPosition;
    [SerializeField]
    private float m_routeOrbSize;
    [SerializeField]
    [Tooltip("Displays orbs to visualize the route the camera will travel between points. Keep off unless editing the routes.")]
    private bool drawOrbs;

    /**************************************************************************************
	* Type: Function
	* 
	* Name: Start
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void Start()
    {
        RouteLanePointSetup();
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: OnValidate
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void OnValidate()
    {
        RouteLanePointSetup();
    }

    private void RouteLanePointSetup()
    {
        m_routePoint = new Transform[transform.childCount][];
        for (int i = 0; i < m_routePoint.Length; i++)
        {
            m_routePoint[i] = new Transform[transform.GetChild(i).childCount];
            for (int j = 0; j < m_routePoint[i].Length; j++)
            {
                m_routePoint[i][j] = transform.GetChild(i).GetChild(j);
            }
        }
    }

    //Re-used Lerp functions from main movement code
    private Vector3 QuadraticLerp(Vector3 p_pointA, Vector3 p_pointB, Vector3 p_pointC, float p_interpolateValue)
    {
        Vector3 _lerpedVector;
        Vector3 _pointAB = Vector3.Lerp(p_pointA, p_pointB, p_interpolateValue);
        Vector3 _pointBC = Vector3.Lerp(p_pointB, p_pointC, p_interpolateValue);
        _lerpedVector = Vector3.Lerp(_pointAB, _pointBC, p_interpolateValue);

        return _lerpedVector;
    }

    private Vector3 CubicLerp(Vector3 p_pointA, Vector3 p_pointB, Vector3 p_pointC, Vector3 p_pointD, float p_interpolateValue)
    {
        Vector3 _lerpedVector;
        Vector3 _pointAB_BC = QuadraticLerp(p_pointA, p_pointB, p_pointC, p_interpolateValue);
        Vector3 _pointBC_CD = QuadraticLerp(p_pointB, p_pointC, p_pointD, p_interpolateValue);
        _lerpedVector = Vector3.Lerp(_pointAB_BC, _pointBC_CD, p_interpolateValue);

        return _lerpedVector;
    }

    //Simplified logic of the route visualizer script
    private void OnDrawGizmos()
    {
        if (!drawOrbs)
            return;

        for (int i = 0; i < m_routePoint.Length; i++)
        {
            for (int j = 0; j < m_routePoint[i].Length; j++)
            {
                if (m_routePoint[i].Length == 4)
                {
                    for (float t = 0; t <= 1; t += 0.02f)
                    {
                        m_gizmosPosition = CubicLerp(m_routePoint[i][0].position, m_routePoint[i][1].position, m_routePoint[i][2].position, m_routePoint[i][3].position, t);
                        Gizmos.DrawSphere(m_gizmosPosition, m_routeOrbSize);
                    }
                }
                else if (m_routePoint[i].Length == 2)
                {
                    for (float t = 0; t <= 1; t += 0.05f)
                    {
                        m_gizmosPosition = Vector3.Lerp(m_routePoint[i][0].position, m_routePoint[i][1].position, t);
                        Gizmos.DrawSphere(m_gizmosPosition, m_routeOrbSize);
                    }
                }
            }
        }
    }
}
