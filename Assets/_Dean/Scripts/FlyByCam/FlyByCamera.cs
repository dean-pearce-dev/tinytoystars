using UnityEngine;
using UnityEngine.UI;

/**************************************************************************************
* Type: Class
* 
* Name: FlyByCamera
*
* Author: Dean Pearce
*
* Description: Class for fly by camera before the race starts
*
* Change Log:
* Date          Initials    Version     Comments
* ----------    --------    -------     ----------------------------------------------
* 02/08/2021    DP          1.00        -Initial Created
**************************************************************************************/

public class FlyByCamera : MonoBehaviour
{
    private Transform m_mainCam;
    private CameraController m_camController;
    private LevelManager m_levelManager;
    [SerializeField]
    [Tooltip("The speed of the camera.")]
    private float m_flySpeed = 0.5f;
    [SerializeField]
    [Tooltip("The speed to fade the screen to black in between route changes.")]
    private float m_fadeSpeed = 0.5f;
    [SerializeField]
    [Tooltip("The point along the route to begin fading the screen. 0 is the start, and 1 is the end.")]
    private float m_pointToStartFading = 0.8f;
    private float m_interpolateAmount = 0f;
    private int m_currentRouteNum = 0;
    private Transform m_targetHolder;
    private Transform[] m_camLookTarget;
    private Transform[] m_routesArray;
    private Image m_blackScreen;
    private bool m_isFading = false;
    private RaceStartController m_raceController;
    private bool m_carPositionsSet = false;

    /**************************************************************************************
	* Type: Function
	* 
	* Name: Start
	*
	* Author: Dean Pearce
	**************************************************************************************/
    void Start()
    {
        m_mainCam = GameObject.Find("Main Camera").transform;
        m_camController = m_mainCam.GetComponent<CameraController>();
        m_levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        m_targetHolder = GameObject.Find("FlyByCamTargets").transform;
        m_routesArray = new Transform[transform.GetChild(0).childCount];
        m_camLookTarget = new Transform[m_targetHolder.childCount];
        m_blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();
        m_raceController = GameObject.Find("RaceStartController").GetComponent<RaceStartController>();
        m_raceController.SetFlyByCam(this);
        for (int i = 0; i < m_routesArray.Length; i++)
        {
            m_routesArray[i] = transform.GetChild(0).GetChild(i);
        }
        for (int i = 0; i < m_camLookTarget.Length; i++)
        {
            m_camLookTarget[i] = m_targetHolder.GetChild(i);
        }

        //To make it fade in from black
        m_blackScreen.color = Color.black;
        m_blackScreen.canvasRenderer.SetAlpha(1.0f);
        FadeFromBlack();

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
        if (!m_carPositionsSet)
        {
            m_raceController.SetPositions();
        }
        MoveCamera();
    }

    //Fade camera functions below from https://answers.unity.com/questions/193954/fade-camera.html
    public void FadeToBlack()
    {
        m_blackScreen.color = Color.black;
        m_blackScreen.canvasRenderer.SetAlpha(0.0f);
        m_blackScreen.CrossFadeAlpha(1.0f, m_fadeSpeed, false);
    }

    private void FadeFromBlack()
    {
        m_blackScreen.color = Color.black;
        m_blackScreen.canvasRenderer.SetAlpha(1.0f);
        m_blackScreen.CrossFadeAlpha(0.0f, m_fadeSpeed, false);
    }

    //Re-used Lerp functions from movement code
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

    /**************************************************************************************
    * Type: Function
    * 
    * Name: MoveCamera
    *
    * Author: Dean Pearce
    *
    * Description: Function to move the camera along interpolated routes
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private void MoveCamera()
    {
        if (!m_raceController.GetFlyByStatus())
        {
            m_interpolateAmount += (m_flySpeed * Time.deltaTime);
            m_mainCam.position = RoutePositions(m_interpolateAmount, m_currentRouteNum);
            m_mainCam.LookAt(m_camLookTarget[m_currentRouteNum]);

            if (m_interpolateAmount > m_pointToStartFading && !m_isFading)
            {
                m_isFading = true;
                FadeToBlack();
            }
            if (m_interpolateAmount >= 1 && m_currentRouteNum < m_routesArray.Length - 1)
            {
                m_isFading = false;
                m_currentRouteNum = (m_currentRouteNum + 1) % m_routesArray.Length;
                m_interpolateAmount = 0f;
                FadeFromBlack();
            }
            if (m_interpolateAmount >= 1 && m_currentRouteNum == m_routesArray.Length - 1)
            {
                m_camController.AssignTargetRacer(m_levelManager.GetPlayer());
                FadeFromBlack();
                m_raceController.SetFlyByStatus(true);
            }
        }
    }

    /**************************************************************************************
    * Type: Function
    * 
    * Name: RoutePositions
    * Returns: _routePos
    *
    * Author: Dean Pearce
    *
    * Description: Modified RoutePositions function from main movement code, modified for the fly by
    *              camera system.
    *
    * Change Log:
    * Date          Initials    Version     Comments
    * ----------    --------    -------     ----------------------------------------------
    * 02/08/2021    DP          1.00        -Initial Created
    **************************************************************************************/
    private Vector3 RoutePositions(float p_interpolateValue, int p_currentRouteNum)
    {
        Vector3 _routePos = new Vector3(0, 0, 0);

        if (m_routesArray[p_currentRouteNum].transform.childCount == 4)
            _routePos = CubicLerp(m_routesArray[p_currentRouteNum].GetChild(0).position, m_routesArray[p_currentRouteNum].GetChild(1).position, m_routesArray[p_currentRouteNum].GetChild(2).position, m_routesArray[p_currentRouteNum].GetChild(3).position, p_interpolateValue);
        else if (m_routesArray[p_currentRouteNum].transform.childCount == 2)
            _routePos = Vector3.Lerp(m_routesArray[p_currentRouteNum].GetChild(0).position, m_routesArray[p_currentRouteNum].GetChild(1).position, p_interpolateValue);

        return _routePos;
    }


    public void SetCarPositionStatus(bool p_carPositionsSet)
    {
        m_carPositionsSet = p_carPositionsSet;
    }

}
