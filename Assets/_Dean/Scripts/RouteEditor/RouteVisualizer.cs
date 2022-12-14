using UnityEngine;

/**************************************************************************************
* Type: Class
* Name: RouteVisualizer
*
* Author: Dean Pearce
*
* Description: Class for showing the route that the vehicles will travel, runs in edit mode
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* ??/07/2021    DP          1.00        -Initial Created
**************************************************************************************/
[ExecuteInEditMode]
public class RouteVisualizer : MonoBehaviour
{
    private Transform[][][] m_routeLanePoint;
    private Vector3 m_gizmosPosition;
    [SerializeField]
    private float m_routeOrbSize;
    [SerializeField]
    [Tooltip("Displays orbs to visualize the route created between the points. Keep turned off unless editing the routes, as it incurs a huge performance hit.")]
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
        //Setting up a jagged-array so that point can easily be accessed by route, then lane, then point
        m_routeLanePoint = new Transform[transform.childCount][][];
        for (int i = 0; i < m_routeLanePoint.Length; i++)
        {
            m_routeLanePoint[i] = new Transform[transform.GetChild(i).childCount][];
            for (int j = 0; j < m_routeLanePoint[i].Length; j++)
            {
                m_routeLanePoint[i][j] = new Transform[transform.GetChild(i).GetChild(j).childCount];
                for (int k = 0; k < m_routeLanePoint[i][j].Length; k++)
                {
                    m_routeLanePoint[i][j][k] = transform.GetChild(i).GetChild(j).GetChild(k);
                }
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

    private void OnDrawGizmos()
    {
        if (!drawOrbs)
            return;

        for (int i = 0; i < m_routeLanePoint.Length; i++)
        {
            for (int j = 0; j < m_routeLanePoint[i].Length; j++)
            {
                if (m_routeLanePoint[i][j].Length == 4)
                {
                    for (float t = 0; t <= 1; t += 0.02f)
                    {
                        //Checks whether the route is a 4-point route with no curve, originally intended for shortcut functionality
                        if (transform.GetChild(i).GetChild(j).gameObject.tag == "NoCurve" || 
                            transform.GetChild(i).GetChild(j).gameObject.tag == "NoCurveNoChangeL" ||
                            transform.GetChild(i).GetChild(j).gameObject.tag == "NoCurveNoChangeR")
                        {
                            for (int p = 0; p < 3; p++)
                            {
                                if (p == 0)
                                {
                                    m_gizmosPosition = Vector3.Lerp(m_routeLanePoint[i][j][0].position, m_routeLanePoint[i][j][1].position, t);
                                }
                                else if (p == 1)
                                    m_gizmosPosition = Vector3.Lerp(m_routeLanePoint[i][j][1].position, m_routeLanePoint[i][j][2].position, t);
                                else if (p == 2)
                                    m_gizmosPosition = Vector3.Lerp(m_routeLanePoint[i][j][2].position, m_routeLanePoint[i][j][3].position, t);
                                Gizmos.DrawSphere(m_gizmosPosition, m_routeOrbSize);
                            }
                        }
                        else
                        {
                            m_gizmosPosition = CubicLerp(m_routeLanePoint[i][j][0].position, m_routeLanePoint[i][j][1].position, m_routeLanePoint[i][j][2].position, m_routeLanePoint[i][j][3].position, t);
                            Gizmos.DrawSphere(m_gizmosPosition, m_routeOrbSize);
                        }
                    }
                }
                else if (m_routeLanePoint[i][j].Length == 2)
                {
                    for (float t = 0; t <= 1; t += 0.05f)
                    {
                        m_gizmosPosition = Vector3.Lerp(m_routeLanePoint[i][j][0].position, m_routeLanePoint[i][j][1].position, t);
                        Gizmos.DrawSphere(m_gizmosPosition, m_routeOrbSize);
                    }
                }
            }
        }
    }
}
