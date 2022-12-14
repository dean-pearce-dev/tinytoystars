#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**************************************************************************************
* Type: Class
* 
* Name: ModularRouteController
*
* Author: Dean Pearce 
*
* Description: Make editing and tweaking tracks/routes from inspector/editor easier
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 25/06/2021    DP          1.00        -Initial Created
* 13/07/2021    DP          1.10        -Added tooltips to serialized variables
* 13/07/2021    DP          1.20        -Added AutoNameChange() Function
* 13/07/2021    DP          1.30        -Added condition to only run if play mode is not active
**************************************************************************************/


[ExecuteInEditMode]
public class ModularRouteController : MonoBehaviour
{
    //Route Position Variables
    [SerializeField]
    [Tooltip("Toggle for making the track a circuit. Only tick this if the track is a circuit, and the last route has been placed. Otherwise, if still adding routes, untick this.")]
    private bool m_isCircuit;

    private Transform[][][] m_routeLanePoint;
    private Transform m_routeHolder;
    private int m_totalRoutes;
    private int m_currentRouteNum;

    //Anchor Variables
    [SerializeField]
    [Tooltip("The route segment to be edited.")]
    private int m_routeToEdit;

    [SerializeField]
    [Tooltip("The point of the route to be edited, where Point A is the start point.")]
    private i_offsetPoints m_pointToEdit;

    [SerializeField]
    [Tooltip("The offset which determines the distance between each lane point.")]
    private Vector3 m_currentPointOffset;

    [SerializeField]
    [Tooltip("The rotation of the lane points around the anchor point.")]
    private float m_anchorRotation;

    [SerializeField]
    [Tooltip("Quick way of selecting the group of points currently being edited.")]
    private bool m_selectPoints = false;

    [SerializeField]
    [Tooltip("Quick way of renaming routes to match their order in the list.")]
    private bool m_renameRoutes = false;

    private Vector3[][] m_anchorOffsets;
    private float[][] m_anchorRotationArray;
    private enum i_offsetPoints { PointA, PointB, PointC, PointD };
    private float m_prevAnchorRotation;
    private bool m_editableAnchorIsSet = false;
    private int m_prevRouteToEditOffset;
    private i_offsetPoints m_prevOffsetPointToEdit;


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
        AnchorSetup();
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: Update
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void Update()
    {
        if (!Application.isPlaying)
        {
            AnchorEditCheck();
            OffsetInspectorDisplay();
            AnchorMatch();
            SelectPointObjects();
        }
    }

    /**************************************************************************************
	* Type: Function
	* 
	* Name: LateUpdate
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void LateUpdate()
    {
        if (!Application.isPlaying)
            MatchPoints();
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
        if (!Application.isPlaying)
        {
            RouteLanePointSetup();
            AnchorSetup();
            NotifyUserOfNoChange();
            AutoNameChange();
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetLaneObject
    * Parameters: p_routeToGet, p_laneToGet
    * Return: _tempLaneObj
    *
    * Author: Dean Pearce
    *
    * Description: Shorthand function for getting the game object of a specific lane.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 03/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private Transform GetLaneObject(int p_routeToGet, int p_laneToGet)
    {
        Transform _tempLaneObj = m_routeHolder.GetChild(p_routeToGet).GetChild(p_laneToGet);
        return _tempLaneObj;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: GetRouteObject
    * Parameters: p_routeToGet
    * Return: _tempRouteObject
    *
    * Author: Dean Pearce
    *
    * Description: Shorthand function for getting the game object of a specific route.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 03/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private Transform GetRouteObject(int p_routeToGet)
    {
        Transform _tempRouteObject = m_routeHolder.GetChild(p_routeToGet);
        return _tempRouteObject;
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: OffsetInspectorDisplay
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Makes inspector values consistent, clamps the values to prevent out of bounds errors,
    * and makes a check which prevents offset values being carried to wrong objects
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 04/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void OffsetInspectorDisplay()
    {
        m_routeToEdit = Mathf.Clamp(m_routeToEdit, 1, m_routeHolder.childCount);
        int _enumValue = (int)m_pointToEdit;
        _enumValue = Mathf.Clamp(_enumValue, 0, GetLaneObject(m_routeToEdit - 1, 0).childCount - 1);
        m_pointToEdit = (i_offsetPoints)_enumValue;
        if (m_editableAnchorIsSet)
        {
            m_anchorRotationArray[m_routeToEdit - 1][(int)m_pointToEdit] = m_anchorRotation;
            m_anchorOffsets[m_routeToEdit - 1][(int)m_pointToEdit] = m_currentPointOffset;
        }
        else if (!m_editableAnchorIsSet)
        {
            m_anchorRotation = m_anchorRotationArray[m_routeToEdit - 1][(int)m_pointToEdit];
            m_currentPointOffset = m_anchorOffsets[m_routeToEdit - 1][(int)m_pointToEdit];
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RouteLanePointSetup
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Sets up a jagged array which can be used to access a specific point,
    * on a specific lane, on a specific route
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 17/08/2021    CT          1.01        -Made m_routeHolder equal its own transform
    *                                       rather than finding itself as additive
    *                                       scene loading caused issues and non ending
    *                                       errors
    **************************************************************************************/
    private void RouteLanePointSetup()
    {
        m_routeHolder = transform;
        m_totalRoutes = m_routeHolder.childCount;
        m_routeLanePoint = new Transform[m_totalRoutes][][];
        for (int i = 0; i < m_totalRoutes; i++)
        {
            m_routeLanePoint[i] = new Transform[m_routeHolder.GetChild(i).childCount][];

            for (int j = 0; j < m_routeLanePoint[i].Length; j++)
            {
                m_routeLanePoint[i][j] = new Transform[GetLaneObject(i, j).childCount];

                for (int k = 0; k < m_routeLanePoint[i][j].Length; k++)
                {
                    m_routeLanePoint[i][j][k] = m_routeHolder.GetChild(i).GetChild(j).GetChild(k);
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MatchPoints
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Makes the start points of a route match the end points of the next route
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 29/06/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void MatchPoints()
    {
        for (int i = 0; i < m_totalRoutes; i++)
        {
            int _lanesInRoute = m_routeLanePoint[i].Length;
            for (int j = 0; j < _lanesInRoute; j++)
            {
                if (i != 0)
                {
                    int _pointsInPrevRouteLane = m_routeLanePoint[i - 1][j].Length;
                    m_routeLanePoint[i][j][0].position = m_routeLanePoint[i - 1][j][_pointsInPrevRouteLane - 1].position;
                }
                if (i == m_totalRoutes - 1 && m_isCircuit)
                {
                    int _pointsInCurrentRouteLane = m_routeLanePoint[i][j].Length;
                    m_routeLanePoint[i][j][_pointsInCurrentRouteLane - 1].position = m_routeLanePoint[0][j][0].position;
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AnchorSetup
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Sets up array which holds offset values for each of the routes/points to adjust the
    * distance in between lane points
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 06/07/2021    DP          1.01        -Removed a nested for loop to fix bug
    **************************************************************************************/
    private void AnchorSetup()
    {
        m_anchorOffsets = new Vector3[m_totalRoutes][];
        m_anchorRotationArray = new float[m_totalRoutes][];

        for (int i = 0; i < m_totalRoutes; i++)
        {
            m_anchorOffsets[i] = new Vector3[GetLaneObject(i, 0).childCount];
            m_anchorRotationArray[i] = new float[GetLaneObject(i, 0).childCount];
            for (int k = 0; k < m_anchorOffsets[i].Length; k++)
            {
                m_anchorOffsets[i][k] = m_routeLanePoint[i][0][k].position - m_routeLanePoint[i][1][k].position;
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AnchorMatch
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Adjusts the lane points based on the m_anchorOffsets array, which is editable in
    * the inspector
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/07/2021    DP          1.00        -Initial Created
    * 05/07/2021    DP          1.10        -Added .RotateAround() to make lane points pivot around anchor
    * 13/07/2021    DP          1.20        -Added logic to stop anchor matching on shortcut routes
    * 14/07/2021    DP          1.30        -Made shortcut condition cleaner
    **************************************************************************************/
    private void AnchorMatch()
    {
        for (int i = 0; i < m_totalRoutes; i++)
        {
            for (int j = 0; j < GetRouteObject(i).childCount; j++)
            {
                for (int k = 0; k < GetLaneObject(i, j).childCount; k++)
                {
                    if (j > 0)
                    {
                        if (m_routeHolder.GetChild(i).gameObject.tag != "Shortcut")
                        {
                            m_routeLanePoint[i][j][k].position = m_routeLanePoint[i][j - 1][k].position - m_anchorOffsets[i][k];
                            m_routeLanePoint[i][j][k].RotateAround(m_routeLanePoint[i][j - 1][k].position, Vector3.up, m_anchorRotationArray[i][k]);
                        }
                    }
                }
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AnchorEditCheck
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Makes checks to determine if values in the inspector have been changed in order
    * to protect wrong values from being changed.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 05/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void AnchorEditCheck()
    {
        if (m_prevOffsetPointToEdit == m_pointToEdit && m_prevRouteToEditOffset == m_routeToEdit)
            m_editableAnchorIsSet = true;
        else if (m_prevOffsetPointToEdit != m_pointToEdit || m_prevRouteToEditOffset != m_routeToEdit)
        {
            m_editableAnchorIsSet = false;
            if (m_prevRouteToEditOffset != m_routeToEdit)
                m_prevRouteToEditOffset = m_routeToEdit;
            if (m_prevOffsetPointToEdit != m_pointToEdit)
                m_prevOffsetPointToEdit = m_pointToEdit;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: SelectPointObjects
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Allows the user to toggle a bool which will automatically select the desired points in the editor
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 07/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void SelectPointObjects()
    {
        if (m_selectPoints)
        {
            GameObject[] _objectsToSelect = new GameObject[GetRouteObject(m_routeToEdit - 1).childCount];
            for (int i = 0; i < _objectsToSelect.Length; i++)
            {
                _objectsToSelect[i] = GetLaneObject(m_routeToEdit - 1, i).GetChild((int)m_pointToEdit).gameObject;
            }
            Selection.objects = _objectsToSelect;
            m_selectPoints = false;
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: NotifyUserOfNoChange
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Lets the user in the editor know why they can't change particular route points.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 05/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void NotifyUserOfNoChange()
    {
        if (m_routeToEdit == m_totalRoutes)
            if (m_isCircuit)
                Debug.Log("<color=red>Warning: </color>You are trying to edit the last route while isCircuit is true. The points are fixed between the previous and next route, so no changes can be made. <color=orange>Route " + m_routeToEdit + " " + m_pointToEdit + "</color>");
            else if (!m_isCircuit && m_pointToEdit == i_offsetPoints.PointA)
                Debug.Log("<color=red>Warning: </color>You are trying to edit the start points of a route that is linked to the previous routes end points. No changes can be made. Try using the end points instead. <color=orange>Route " + m_routeToEdit + " " + m_pointToEdit + "</color>");
        if (m_routeToEdit != 1 && m_routeToEdit != m_totalRoutes)
        {
            if (m_pointToEdit == i_offsetPoints.PointA)
                Debug.Log("<color=red>Warning: </color>You are trying to edit the start points of a route that is linked to the previous routes end points. No changes can be made. Try using the end points instead. <color=orange>Route " + m_routeToEdit + " " + m_pointToEdit + "</color>");
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: AutoNameChange
    * Parameters: None
    * Return: None
    *
    * Author: Dean Pearce
    *
    * Description: Allows the user to toggle a bool which will automatically name route game objects.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 13/07/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void AutoNameChange()
    {
        if (m_renameRoutes)
        {
            for (int i = 0; i < m_totalRoutes; i++)
            {
                if (m_routeHolder.GetChild(i).gameObject.name != "Route" + (i + 1) + "S")
                    m_routeHolder.GetChild(i).gameObject.name = "Route" + (i + 1);
            }
            m_renameRoutes = false;
        }
    }
}

#endif