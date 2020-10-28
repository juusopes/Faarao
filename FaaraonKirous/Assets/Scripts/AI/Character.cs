using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

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
    [SerializeField]
    private StateIndicators stateIndicators;
    [SerializeField]
    private SpriteRenderer stateVisualizer = null;
    public Transform sightPosition;
    [SerializeField]
    private GameObject fieldOfViewGO = null;
    public GameObject clickSelector = null;
    public GameObject selectionIndicator = null;
    [SerializeField]
    private PathVisualizer pathVisualizer = null;
    #endregion 

    #region Regular fields
    [HideInInspector]
    public Vector3 chaseTarget;
    [HideInInspector]
    public Vector3 lastSeenPosition;
    [HideInInspector]
    public Vector3 additionalTarget;
    [HideInInspector]
    public Distraction currentDistraction;
    [HideInInspector]
    public Vector3 currentDistractionPos;
    private Waypoint currentWaypoint;
    private bool waypointFinished = false;
    private bool waypointTimer = false;
    private float impairedSightTimer;
    private float impairedFOVTimer;
    private float distractionTimer;
    [HideInInspector]
    public bool impairedSightRange;
    [HideInInspector]
    public bool impairedFOV;
    [HideInInspector]
    public bool isDistracted;
    private bool couldDetectPlayer1;
    private bool couldDetectPlayer2;
    private DeathScript deathScript;
    [HideInInspector]
    public bool isPosessed;

    private GameObject player1ref;
    private GameObject player2ref;
    private PlayerController playerController1ref;
    private PlayerController playerController2ref;
    private Transform distractionContainer;

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    private EnemyNetManager enemyNetManager;

    //Aid scripts (create with new)
    private Navigator navigator;                    //new create
    private StateMachine stateMachine;              //new create
    private SightDetection player1SightDetection;   //new create
    private SightDetection player2SightDetection;   //new create
    private SightDetection testSightDetection;      //new create
    private OffMeshLinkMovement linkMovement;       //new create

    #endregion

    #region Expression bodies
    public GameObject Player1 => player1ref == null ? RefreshPlayer(1) : player1ref;
    public GameObject Player2 => player2ref == null ? RefreshPlayer(2) : player2ref;
    public PlayerController Player1Controller => playerController1ref == null ? Player1.GetComponent<PlayerController>() : playerController1ref;
    public PlayerController Player2Controller => playerController2ref == null ? Player2.GetComponent<PlayerController>() : playerController2ref;
    public float SightRange => impairedSightRange ? classSettings.impairedSightRange : classSettings.sightRange;
    public float SightRangeCrouching => impairedSightRange ? classSettings.impairedSightRange : classSettings.sightRangeCrouching;
    public float FOV => impairedFOV ? classSettings.impairedFov : classSettings.fov;
    public bool CanDetectAnyPlayer => couldDetectPlayer1 || couldDetectPlayer2;
    public bool IsDead => deathScript == null ? false : deathScript.isDead;
    public bool IsHost => NetworkManager._instance.IsHost;
    public bool ShouldSendToClient => NetworkManager._instance.ShouldSendToClient;
    public int Id => enemyNetManager.Id;

    #endregion

    #region Start and update
    void Awake()
    {
        enemyNetManager = GetComponent<EnemyNetManager>();
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        deathScript = GetComponent<DeathScript>();

        if (IsHost)
        {
            stateMachine = new StateMachine(this);
            linkMovement = new OffMeshLinkMovement(transform, navMeshAgent, classSettings.modelRadius, classSettings.navJumpHeight);      //TODO: Check radius and height
            InitNavMeshAgent();
            Assert.IsNotNull(deathScript, "Me cannut dai!");
        }
        else
        {
            Destroy(navMeshAgent);
            Destroy(deathScript);
        }

        Assert.IsNotNull(enemyNetManager, "Can't touch this.");
        Assert.IsNotNull(fieldOfViewGO, "Me cannut fow!");
        Assert.IsNotNull(clickSelector, "Me cannut klik!");
        Assert.IsNotNull(stateVisualizer, "Me cannut indi!");
        Assert.IsNotNull(stateIndicators, "Me cannut indi!");

        player1SightDetection = new SightDetection(gameObject, classSettings.lm, 0.2f, classSettings.sightSpeed);
        player2SightDetection = new SightDetection(gameObject, classSettings.lm, 0.2f, classSettings.sightSpeed);
        testSightDetection = new SightDetection(gameObject, classSettings.lm, 0.2f, 1000f);
    }

    private void Start()
    {
        if (IsHost)
        {
            InitNavigator();
            if (AbilitySpawner.Instance)
                distractionContainer = AbilitySpawner.Instance.transform;
            if (!distractionContainer)
                Debug.LogWarning("Did not find DistractionSpawner");
        }
    }

    private void InitNavMeshAgent()
    {
        navMeshAgent.updateRotation = true;

        navMeshAgent.speed = classSettings.navSpeed;
        navMeshAgent.angularSpeed = classSettings.navAngularSpeed;
        navMeshAgent.acceleration = classSettings.navAcceleration;
        navMeshAgent.stoppingDistance = classSettings.navStoppingDistance;
    }

    private void InitNavigator()
    {
        //if (waypointGroup == null)
            //Debug.Log("Character with no wp group!", this);
        navigator = new Navigator(this.transform, waypointGroup);
        currentWaypoint = navigator.GetFirstWaypoint();
    }

    private void Update()
    {
        if (IsHost)
        {
            if (IsDead)
            {
                if (ShouldSendToClient)
                    ServerSend.EnemyDied(Id);
                Die();
                return;
            }
            stateMachine.UpdateSM();
            TestOffLink();
            DetectDistractions();
            RunImpairementCounters();
        }
    }

    private void LateUpdate()
    {
        if (IsHost)
        {
            TryDetectPlayers();
        }
    }

    #endregion

    #region Generic

    public void SetSightVisuals(bool enable)
    {
        fieldOfViewGO.SetActive(enable);
    }

    public void Die()
    {
        Debug.Log("Enemy " + gameObject.name + " diededed");
        //Own components
        Destroy(navMeshAgent);
        Destroy(deathScript);

        //Child objects
        foreach (Transform c in transform)
            Destroy(c.gameObject);

        player1SightDetection.DestroyLine();
        player2SightDetection.DestroyLine();
        testSightDetection.DestroyLine();
        gameObject.tag = "DeadEnemy";
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider c in colliders)
            c.isTrigger = true;
        Destroy(this);
    }
    private GameObject RefreshPlayer(int i)
    {
        if (i == 1)
        {
            player1ref = GameManager._instance.Pharaoh;
            StartCoroutine(player1SightDetection.ResetLineRenderer(Player1));
            return player1ref;
        }
        else if (i == 2)
        {
            player2ref = GameManager._instance.Priest;
            StartCoroutine(player2SightDetection.ResetLineRenderer(Player2));
            return player2ref;
        }
        return null;
    }

    public void UpdateStateIndicator(StateOption stateOption)
    {
        if (stateIndicators == null || stateVisualizer == null)
            return;

        if (ShouldSendToClient)
            ServerSend.StateChanged(Id, stateOption);

        stateVisualizer.sprite = stateIndicators.GetIndicator(stateOption);
    }
    #endregion

    #region Detection
    private void TryDetectPlayers()
    {
        couldDetectPlayer1 = CouldDetectPlayer(Player1, Player1Controller);
        couldDetectPlayer2 = CouldDetectPlayer(Player2, Player2Controller);
        if (player1SightDetection.SimulateSightDetection(couldDetectPlayer1))
        {
            DeathScript ds = Player1.GetComponent<DeathScript>();
            if (ds && !ds.isDead)
            {
                ds.damage = 1;
                StartCoroutine(player1SightDetection.ResetLineRenderer(Player1));
                stateMachine.PlayerDied();
                Debug.Log("Killed player 1");
            }
        }
        if (player2SightDetection.SimulateSightDetection(couldDetectPlayer2))
        {
            DeathScript ds = Player2.GetComponent<DeathScript>();
            if (ds && !ds.isDead)
            {
                ds.damage = 1;
                StartCoroutine(player2SightDetection.ResetLineRenderer(Player2));
                stateMachine.PlayerDied();
                Debug.Log("Killed player 2");
            }
        }
    }


    /// <summary>
    /// Returns true if player is in sight, fov and can be raycasted. Allows sightline simulation to begin, which dictates actually seeing player.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool CouldDetectPlayer(GameObject player, PlayerController playerController)
    {
        if (IsPlayerDeadOrInvisible(playerController))   //Call this first so we dont mark targets
            return false;
        if (isPosessed)
            return false;
        float testRange = playerController != null && playerController.isCrouching ? SightRangeCrouching : SightRange;
        return TestDetection(player, testRange, RayCaster.playerDetectLayerMask, RayCaster.PLAYER_TAG);
    }

    public bool IsPlayerDeadOrInvisible(PlayerController playerController)
    {
        if (playerController == null)
            return true;
        if (playerController.IsDead || playerController.isInvisible)
            return true;

        return false;
    }

    public bool CouldDetectDistraction(GameObject testObj)
    {
        return TestDetection(testObj, SightRange, RayCaster.distractionLayerMask, RayCaster.DISTRACTION_TAG);
    }

    /// <summary>
    /// Returns true if object is in sight, fov and can be raycasted.
    /// </summary>
    /// <param name="testObj"></param>
    /// <returns></returns>
    private bool TestDetection(GameObject testObj, float range, LayerMask layermask, string tag)
    {
        return ObjectIsInRange(testObj, range) && ObjectIsInFov(testObj) && CanRayCastObject(testObj, layermask, tag);
    }

    private bool ObjectIsInRange(GameObject testObj, float range)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= range;
    }

    private bool ObjectIsInFov(GameObject testObj)
    {
        Vector3 dirToObj = (testObj.transform.position - fieldOfViewGO.transform.position).normalized;
        if (Vector3.Angle(fieldOfViewGO.transform.forward, dirToObj) < FOV / 2f)
        {
            return true;
        }
        return false;
    }
    private bool ObjectIsInHearingRange(GameObject testObj)
    {
        return Vector3.Distance(testObj.transform.position, transform.position) <= classSettings.hearingRange;
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
        //TODO: fix raycast position
        if (distractionContainer == null)
            return;

        bool distractionAssigned = false;
        bool testerAssigned = false;

        for (int i = distractionContainer.childCount - 1; i >= 0; i--)
        {
            Transform childTransform = distractionContainer.GetChild(i);
            if (currentDistraction != null)
                if (childTransform == currentDistraction.transform)
                    return;

            Distraction distraction = childTransform.GetComponent<Distraction>();
            if (distraction)
            {
                if (IsDistractionDetectable(distraction))
                {
                    if (distraction.option == AbilityOption.TestSight)
                    {
                        if (!testerAssigned)
                        {
                            testerAssigned = true;
                            testSightDetection.DisplaySightTester(true, distraction.transform.position + Vector3.up, LineType.White);
                        }
                    }
                    else
                    {
                        if (!distractionAssigned)
                        {
                            distractionAssigned = true;
                            ReceiveDistraction(distraction);
                        }

                    }
                }
            }
        }

        if (!testerAssigned && !distractionAssigned)
            testSightDetection.DisplaySightTester(false, Vector3.zero, LineType.White);
    }

    private bool IsDistractionDetectable(Distraction distraction)
    {
        if (distraction.detectionType == DetectionType.hearing)
            if (ObjectIsInHearingRange(distraction.gameObject))
                return true;

        if (distraction.detectionType == DetectionType.sight)
            if (CouldDetectDistraction(distraction.gameObject))
            {
                return true;
            }

        return false;
    }

    private void ReceiveDistraction(Distraction distraction)
    {
        navMeshAgent.speed = classSettings.navSpeed;

        //if (distraction.detectionType == DetectionType.sight)
        // {
        //   testSightDetection.DisplaySightTester(true, distraction.transform.position + Vector3.up, LineType.White);
        // StartCoroutine(testSightDetection.DisableSightTesterTimed());
        //}

        Debug.Log("New distraction: " + distraction.option);
        //Always get blinded
        if (distraction.option == AbilityOption.DistractBlindingLight)
            StartImpairSightRange(distraction.effectTime);
        currentDistractionPos = distraction.transform.position;
        isDistracted = true;
        waypointFinished = true;
        currentDistraction = distraction;
        distractionTimer = distraction.effectTime;
    }

    public void ResetDistraction()
    {
        //Commented -- Sight should be impaired even if new distraction appear
        //impairedSightTimer = 0;
        //impairedSightRange = false;

        impairedFOVTimer = 0;
        impairedFOV = false;

        distractionTimer = 0;
        isDistracted = false;

        navMeshAgent.speed = classSettings.navSpeed;
        SendToClient_SightChanged();
    }

    private void RunImpairementCounters()
    {
        if (impairedSightTimer > 0)
        {
            impairedSightTimer -= Time.deltaTime;
        }
        else if (impairedSightRange)
        {
            impairedSightRange = false;
            SendToClient_SightChanged();
        }

        if (impairedFOVTimer > 0)
        {
            impairedFOVTimer -= Time.deltaTime;
        }
        else if (impairedFOV)
        {
            impairedFOV = false;
            SendToClient_SightChanged();
        }

        if (distractionTimer > 0)
            distractionTimer -= Time.deltaTime;
        else if (isDistracted)
            isDistracted = false;
    }

    public void StartImpairSightRange(float time)
    {
        impairedSightTimer = time;
        impairedSightRange = true;
        SendToClient_SightChanged();
    }

    public void StartImpairFOV(float time)
    {
        impairedFOVTimer = time;
        impairedFOV = true;
        SendToClient_SightChanged();
    }
    #endregion

    #region Other player shenanigans

    public void PossessAI(Vector3 position)
    {
        stateMachine.PlayerTakesControl();
        additionalTarget = position;
    }

    public void VisualizePath()
    {
        if (pathVisualizer)
            pathVisualizer.Visualize(navigator.GetVisualizedPath());
    }

    #endregion

    #region Movement

    public void PanicRunAround()
    {
        RotateCircle(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f, UnityEngine.Random.Range(100f, 300f));
        if (HasReachedDestination())
        {
            navMeshAgent.speed = classSettings.navSpeed + (UnityEngine.Random.Range(0.0f, classSettings.navSpeed * 1.5f));
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
            if (currentWaypoint.type == WaypointType.Climb && nextWaypoint.type != WaypointType.Climb)
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

    #region Networking
    private void SendToClient_SightChanged()
    {
        if (ShouldSendToClient)
            ServerSend.SightChanged(enemyNetManager.Id, impairedSightRange, impairedFOV);
    }

    #endregion

    #region Editor stuff
#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        if (waypointGroup != null)
            UnityEditor.Handles.DrawDottedLine(transform.position, waypointGroup.transform.position, 4f);
    }
#endif
    #endregion
}
