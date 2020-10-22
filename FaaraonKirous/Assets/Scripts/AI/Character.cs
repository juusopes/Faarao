using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    #region User setting fields
    [Header("AI Class")]
    public AIClass classSettings;
    [Header("Patrol route")]
    [Tooltip("All my child gameObjects with component Waypoint and that is nav mesh reachable will be added to a list of patrol route.")]
    [SerializeField]
    private WaypointGroup waypointGroup = null;
    [Header("General")]
    public Transform sightPosition;
    [SerializeField]
    private GameObject fov = null;
    public LayerMask playerDetectLayerMask;
    public LayerMask distractionLayerMask;
    #endregion 

    #region Regular fields
    [HideInInspector]
    public Vector3 chaseTarget;
    [HideInInspector]
    public Vector3 lastSightTarget;
    [HideInInspector]
    public Vector3 lastSeenPosition;
    [HideInInspector]
    public Distraction currentDistraction;
    public Vector3 currentDistractionPos;
    private Waypoint currentWaypoint;
    private bool waypointFinished = false;
    private bool waypointTimer = false;
    private float impairedSightTimer;
    private float impairedFOVTimer;
    public float distractionTimer;
    [HideInInspector]
    public bool impairedSightRange;
    [HideInInspector]
    public bool impairedFOV;
    [HideInInspector]
    public bool isDistracted;
    private bool canDetectPlayer1;
    private bool canDetectPlayer2;

    private GameObject[] players = new GameObject[2];

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

    //Aid scripts
    private Navigator navigator;
    private StateMachine stateMachine;
    private SightDetection player1SightDetection;
    private SightDetection player2SightDetection;
    private OffMeshLinkMovement linkMovement;
    private GameObject distractionContainerGO;
    #endregion

    #region Expression bodies
    public GameObject Player1 => players[0];
    public GameObject Player2 => players[1];
    public float SightRange => impairedSightRange ? classSettings.impairedSightRange : classSettings.sightRange;
    public float FOV => impairedFOV ? classSettings.impairedFov : classSettings.fov;
    public bool CanDetectAnyPlayer => canDetectPlayer1 || canDetectPlayer2;

    #endregion

    #region Start and update
    void Awake()
    {
        stateMachine = new StateMachine(this);
        player1SightDetection = new SightDetection(gameObject, classSettings.lm, 0.1f);
        player2SightDetection = new SightDetection(gameObject, classSettings.lm, 0.1f);
        InitNavMeshAgent();
        linkMovement = new OffMeshLinkMovement(transform, navMeshAgent, classSettings.modelRadius, classSettings.navJumpHeight);      //TODO: Check radius and height
    }

    private void Start()
    {
        RefreshPlayers();
        InitNavigator();
        distractionContainerGO = GameObject.Find(DistractionSpawner.DISTRACTION_CONTAINER);
    }

    private void InitNavMeshAgent()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.updateRotation = true;

        navMeshAgent.speed = classSettings.navSpeed;
        navMeshAgent.angularSpeed = classSettings.navAngularSpeed;
        navMeshAgent.acceleration = classSettings.navAcceleration;
        navMeshAgent.stoppingDistance = classSettings.navStoppingDistance;
    }

    private void InitNavigator()
    {
        if (waypointGroup == null)
            Debug.Log("Character with no wp group!", this);
        navigator = new Navigator(this.transform, waypointGroup);
        currentWaypoint = navigator.GetFirstWaypoint();
    }

    private void Update()
    {
        stateMachine.UpdateSM();
        DetectDistractions();
        TestOffLink();
        RunImpairementCounters();
    }

    private void LateUpdate()
    {
        TryDetectPlayer();
    }

    #endregion

    #region Generic

    private void RefreshPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        player1SightDetection.ResetLineRenderer(Player1, classSettings.sightSpeed);
        player2SightDetection.ResetLineRenderer(Player2, classSettings.sightSpeed);
    }

    public void UpdateIndicator(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
    #endregion

    #region Detection
    private void TryDetectPlayer()
    {
        canDetectPlayer1 = CanDetectPlayer(Player1);
        canDetectPlayer2 = CanDetectPlayer(Player2);
        player1SightDetection.SimulateSightDetection(canDetectPlayer1);
        player2SightDetection.SimulateSightDetection(canDetectPlayer2);
    }

    /// <summary>
    /// Returns true if player is in sight, fov and can be raycasted. Allows sightline simulation to begin, which dictates actually seeing player.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool CanDetectPlayer(GameObject player)
    {
        return ObjectIsInRange(player) && ObjectIsInFov(player) && CanRayCastObject(player, playerDetectLayerMask, RayCaster.PLAYER_TAG);
    }

    /// <summary>
    /// Returns true if object is in sight, fov and can be raycasted. Allows sightline simulation to begin, which dictates actually seeing player.
    /// </summary>
    /// <param name="testObj"></param>
    /// <returns></returns>
    public bool CanDetectDistraction(GameObject testObj)
    {
        return ObjectIsInRange(testObj) && ObjectIsInFov(testObj) && CanRayCastObject(testObj, distractionLayerMask);
    }

    private bool ObjectIsInRange(GameObject testObj)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= SightRange;
    }

    private bool ObjectIsInHearingRange(GameObject testObj)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= classSettings.hearingRange;
    }

    private bool ObjectIsInFov(GameObject testObj)
    {
        Vector3 dirToObj = (testObj.transform.position - fov.transform.position).normalized;
        if (Vector3.Angle(fov.transform.forward, dirToObj) < FOV / 2f)
        {
            return true;
        }
        return false;
    }

    public bool CanRayCastObject(GameObject testObj, LayerMask layerMask, string tag = "")
    {
        RaycastHit hit = RayCaster.ToTarget(gameObject, testObj, SightRange, layerMask);
        if (RayCaster.HitObject(hit, tag))
        {
            if (tag == RayCaster.PLAYER_TAG)
                chaseTarget = hit.transform.position;
            return true;
        }

        return false;
    }

    #endregion

    #region Distractions

    private void DetectDistractions()
    {
        if (distractionContainerGO == null)
        {
            distractionContainerGO = GameObject.Find(DistractionSpawner.DISTRACTION_CONTAINER);
            Debug.LogWarning("Did not find distractionContainer");
            return;
        }

        for (int i = distractionContainerGO.transform.childCount - 1; i >= 0; i--)
        {
            Transform childTransform = distractionContainerGO.transform.GetChild(i);
            if (currentDistraction != null)
                if (childTransform == currentDistraction.transform)
                    return;

            Distraction distraction = childTransform.GetComponent<Distraction>();
            if (distraction)
            {
                if (IsDistractionDetectable(distraction))
                {
                    ReceiveDistraction(distraction);
                    return;
                }
            }
        }
    }
    private bool IsDistractionDetectable(Distraction distraction)
    {
        if (distraction.detectionType == DetectionType.hearing)
            if (ObjectIsInHearingRange(distraction.gameObject))
                return true;

        if (distraction.detectionType == DetectionType.sight)
            if (CanDetectDistraction(distraction.gameObject))
                return true;
        return false;
    }

    private void ReceiveDistraction(Distraction distraction)
    {
        Debug.Log("New distraction: " + distraction.option);
        //Always get blinded
        if (distraction.option == DistractionOption.BlindingLight)
            StartImpairSightRange(distraction.effectTime);
        currentDistractionPos = distraction.transform.position; 
        isDistracted = true;
        waypointFinished = true;
        currentDistraction = distraction;
        distractionTimer = distraction.effectTime;
    }

    public void ResetDistraction()
    {
        //impairedSightTimer = 0;
        //impairedSightRange = false;

        impairedFOVTimer = 0;
        impairedFOV = false;

        distractionTimer = 0;
        isDistracted = false;
    }

    private void RunImpairementCounters()
    {
        if (impairedSightTimer > 0)
            impairedSightTimer -= Time.deltaTime;
        else
            impairedSightRange = false;

        if (impairedFOVTimer > 0)
            impairedFOVTimer -= Time.deltaTime;
        else
            impairedFOV = false;

        if (distractionTimer > 0)
            distractionTimer -= Time.deltaTime;
        else
            isDistracted = false;
    }

    public void StartImpairSightRange(float time)
    {
        impairedSightTimer = time;
        impairedSightRange = true;
    }

    public void StartImpairFOV(float time)
    {
        impairedFOVTimer = time;
        impairedFOV = true;
    }
    #endregion    

    #region Movement

    public void PanicRunAround()
    {
        RotateCircle(Random.Range(0.0f, 1.0f) > 0.5f, Random.Range(100f, 300f));
        if (HasReachedDestination())
        {
            Vector3 dest = OnNavMesh.GetRandomPointSameHeight(transform.position, 3f);
            SetDestination(dest);
        }
    }

    public void PatrolRotate()
    {
        if (currentWaypoint == null)
            return;
        LerpMatchRotation(currentWaypoint.transform.rotation, currentWaypoint.GetRotationSpeed());
        RotateSight(currentWaypoint.GetHorizontalArc());
    }
    public void SearchRotate()
    {
        if (currentWaypoint == null)
            return;
        RotateSight(classSettings.alertHorizontalArc);
    }

    private void RotateSight(float harc)
    {
        //TODO:this
    }

    private void RotateCircle(bool clockWise, float rotateSpeed = 0)
    {
        //Rotates around like a FOOL
        int direction = clockWise ? 1 : -1;
        if (rotateSpeed == 0)
            rotateSpeed = classSettings.searchTurnSpeed;

        transform.Rotate(0, direction * rotateSpeed * Time.deltaTime, 0);
    }


    public void LerpLookAt(Vector3 target, float rotateSpeed = 0)
    {
        if (target == null)
            return;

        if (rotateSpeed == 0)
            rotateSpeed = classSettings.searchTurnSpeed;

        Vector3 direction = target - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
        LerpMatchRotation(toRotation, rotateSpeed);
    }

    private void LookAtInstantly(Transform target)
    {
        if (target == null)
            return;
        Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(targetPosition);
    }

    private void LerpMatchRotation(Quaternion targetRotation, float rotateSpeed = 0)
    {
        if (targetRotation == null)
            return;

        if (rotateSpeed == 0)
            rotateSpeed = classSettings.searchTurnSpeed;

        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles = new Vector3(0, eulerAngles.y, 0);
        Quaternion newRotation = Quaternion.Euler(eulerAngles);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, Time.deltaTime * rotateSpeed);
    }

    public void GetNextWaypoint()
    {
        if (navigator == null)
            return;
        waypointTimer = false;
        Waypoint nextWaypoint = navigator.GetNextWaypoint();
        if (nextWaypoint != null) 
            if(currentWaypoint.type == WaypointType.Climb  && nextWaypoint.type != WaypointType.Climb)
                navMeshAgent.enabled = true;
        if (nextWaypoint == null)
            navigator = null;
        else
            currentWaypoint = nextWaypoint;
    }

    public void OnWaypoint()
    {
        if (currentWaypoint == null)
            return;

        switch (currentWaypoint.type)
        {
            case WaypointType.WalkPast:
                waypointFinished = true;
                SetDestination(currentWaypoint.transform.position);
                break;
            case WaypointType.GuardForDuration:
                SetDestination(currentWaypoint.transform.position);
                if (HasReachedDestination())
                    PatrolRotate();

                if (HasReachedDestination() && waypointTimer == false && waypointFinished != false)
                {
                    waypointTimer = true;
                    waypointFinished = false;
                    StartCoroutine("StartWaypointTimer", currentWaypoint.GetGuardDuration());
                }
                break;
            case WaypointType.GuardForEver:
                SetDestination(currentWaypoint.transform.position);
                if (HasReachedDestination())
                    PatrolRotate();
                waypointFinished = false;
                break;
            case WaypointType.Climb:
                if (waypointFinished == true)
                {
                    waypointFinished = false;
                    StopNavigation();
                    navMeshAgent.enabled = false;
                }
                Climb();
                break;
        }
    }

    private void TestOffLink()
    {
        if (navMeshAgent.isOnOffMeshLink && linkMovement.CanStartLink())
        {
            OffMeshLinkRoute route = linkMovement.GetOffMeshLinkRoute();
            StartCoroutine(linkMovement.MoveAcrossNavMeshLink(route));
        }
    }

    private void Climb()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.transform.position, Time.deltaTime * navMeshAgent.speed);
        LerpMatchRotation(currentWaypoint.transform.rotation, currentWaypoint.GetRotationSpeed());

        if (Mathf.Approximately(Vector3.Distance(transform.position, currentWaypoint.transform.position), 0))
        {
            waypointFinished = true;
        }
    }


    private IEnumerator StartWaypointTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        waypointFinished = true;
    }
    #endregion

    #region Navmesh

    public void SetDestination(Vector3 position)
    {
        if (position == null || navMeshAgent.destination == position || !navMeshAgent.enabled)
            return;

        if (!OnNavMesh.IsReachable(transform, position))
            return;


        navMeshAgent.destination = position;
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = navMeshAgent.isOnOffMeshLink ? 0.05f : classSettings.navStoppingDistance;
    }

    public void StopNavigation()
    {
        if (!navMeshAgent.enabled)
            return;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    public bool HasFinishedWaypointTask()
    {
        return waypointFinished && HasReachedDestination();
    }

    public bool HasReachedDestination()
    {
        if (navMeshAgent.enabled == false)
            return true;
        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending;
    }
    #endregion

    #region Editor stuff
#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (waypointGroup != null)
            Handles.DrawDottedLine(transform.position, waypointGroup.transform.position, 4f);
    }
#endif
    #endregion
}
