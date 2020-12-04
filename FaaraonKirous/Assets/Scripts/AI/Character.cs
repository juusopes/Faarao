using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

[SelectionBase]
public class Character : MonoBehaviour
{
    #region User setting fields
    [Header("AI Class")]
    [SerializeField]
    private AIClass classSettings;
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
    public GameObject fieldOfViewGO = null;
    public GameObject fieldOfView2DGO = null;
    public GameObject clickSelector = null;
    public GameObject selectionIndicator = null;
    [SerializeField]
    private PathVisualizer pathVisualizer = null;
    #endregion

    #region Distraction fields
    [HideInInspector]
    public bool isPosessed;
    [HideInInspector]
    public bool impairedSightRange;
    [HideInInspector]
    public bool impairedFOV;
    [HideInInspector]
    public bool isDistracted;

    private float impairedSightTimer;
    private float impairedFOVTimer;
    private float distractionTimer;

    [HideInInspector]
    public Vector3 possessedGoToTarget = UtilsClass.GetMinVector();
    [HideInInspector]
    public Vector3 currentDistractionPos;
    [HideInInspector]
    public Distraction currentDistraction;
    #endregion

    #region Regular fields
    [HideInInspector]
    public Vector3 chaseTarget;
    public Vector3 lastSeenPosition;
    private Waypoint currentWaypoint;
    private bool waypointFinished = false;
    private bool waypointTimer = false;

    public bool couldDetectPlayer1;
    public bool couldDetectPlayer2;

    private bool isDead;

    public StateOption PreviousStateOption { get; private set; } = StateOption.PatrolState;
    public StateOption CurrentStateOption { get; private set; } = StateOption.PatrolState;
    public AnimationState CurrentAnimationState { get; private set; } = AnimationState.Idle;

    private GameObject player1ref;
    private GameObject player2ref;
    private PlayerController playerController1ref;
    private PlayerController playerController2ref;

    private bool need2DFOV = false;

    //Components

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    private EnemyObjectManager enemyNetManager;
    private DeathScript deathScript;

    //Aid scripts (create with new)
    public Navigator navigator;                    //new create
    private StateMachine stateMachine;              //new create
    private SightRenderer player1SightRenderer;   //new create
    private SightRenderer player2SightRenderer;   //new create
    public SightDetection testSightDetection;      //new create
    public Detector detector;                       //new create
    private OffMeshLinkMovement linkMovement;       //new create
    private CharacterAnimations charAnims;       //new create

    #endregion

    #region Expression bodies
    public GameObject Player1 => player1ref == null ? RefreshPlayer(1) : player1ref;
    public GameObject Player2 => player2ref == null ? RefreshPlayer(2) : player2ref;
    public PlayerController Player1Controller => playerController1ref == null ? playerController1ref = Player1.GetComponent<PlayerController>() : playerController1ref;
    public PlayerController Player2Controller => playerController2ref == null ? playerController2ref = Player2.GetComponent<PlayerController>() : playerController2ref;

    public float HearingRange => classSettings.hearingRange;
    public float MaxSightRange => classSettings.sightRange;
    public float SightRange => impairedSightRange ? classSettings.impairedSightRange : classSettings.sightRange;
    public float SightRangeCrouching => impairedSightRange ? classSettings.impairedSightRange : classSettings.sightRangeCrouching;
    public float FOV => impairedFOV ? classSettings.impairedFov : classSettings.fov;
    public float SearchingDuration => classSettings.searchingDuration;
    public float DetectionLostReactionDelay => classSettings.detectionLostReactionDelay;
    public float ControlledDuration => classSettings.controlledDuration;
    public float SightSpeed => classSettings.sightSpeed;
    public bool IsDead => deathScript == null ? false : deathScript.isDead;
    public bool IsHost => NetworkManager._instance.IsHost;
    public bool ShouldSendToClient => NetworkManager._instance.ShouldSendToClient;
    public int Id => enemyNetManager.Id;
    public bool CouldDetectAnyPlayer => couldDetectPlayer1 || couldDetectPlayer2;
    public float DetectionPercentage => detector.detectionPercentage;

    #endregion

    #region Start and run
    void Awake()
    {
        enemyNetManager = GetComponent<EnemyObjectManager>();
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
        }

        Assert.IsNotNull(enemyNetManager, "Can't touch this.");
        Assert.IsNotNull(fieldOfViewGO, "Me cannut fow!");
        Assert.IsNotNull(clickSelector, "Me cannut klik!");
        Assert.IsNotNull(stateVisualizer, "Me cannut indi!");
        Assert.IsNotNull(stateIndicators, "Me cannut indi!");

        player1SightRenderer = new SightRenderer(gameObject, classSettings.lm, 0.2f, MaxSightRange);
        player1SightRenderer.ResetLineRenderer(Player1);
        player2SightRenderer = new SightRenderer(gameObject, classSettings.lm, 0.2f, MaxSightRange);
        player2SightRenderer.ResetLineRenderer(Player2);
        //player2SightDetection = new SightDetection(gameObject, classSettings.lm, 0.2f, SightSpeed);
        testSightDetection = new SightDetection(gameObject, classSettings.lm, 0.2f, 1000f);
        lastSeenPosition = UtilsClass.GetMinVector();

        need2DFOV = SceneManager.GetActiveScene().buildIndex == 4;
    }

    private void Start()
    {
        if (IsHost)
        {
            // ForceStraigthen();
            detector = new Detector(this);
            InitNavigator();
        }
        charAnims = new CharacterAnimations(this);
    }

    private void ForceStraigthen()
    {
        Quaternion q = transform.rotation;
        q.eulerAngles = new Vector3(0, q.eulerAngles.y, 0);
        transform.rotation = q;
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

#if UNITY_EDITOR
        if (isDebuggingDummy) return;
#endif
        if (IsDead)
        {
            Die();
            return;
        }

        if (IsHost)
        {
            detector.RunDetection();
            stateMachine.UpdateSM();
            TestOffLink();
            RunImpairementCounters();
            ManageAnimations();
        }
    }

    #endregion

    #region Generic

    public void ManageAnimations()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f)
            SetAnimation(AnimationState.Walk);
        else
            SetAnimation(AnimationState.Idle);
    }

    public void SetAnimation(AnimationState state)
    {
        if (charAnims.currentState != state)
        {
            CurrentAnimationState = state;
            charAnims.SetAnimationState(state);
            SendToClient_AnimationChanged();
        }
    }

    public void LostTrackOfPlayer()
    {
        lastSeenPosition = OnNavMesh.GetRandomPointOnUnitCircleSameHeight(chaseTarget, 2f);
    }

    private GameObject RefreshPlayer(int i)
    {
        if (i == 1)
        {
            player1ref = GameManager._instance.Pharaoh;
            //StartCoroutine(player1SightDetection.ResetLineRenderer(Player1));
            return player1ref;
        }
        else if (i == 2)
        {
            player2ref = GameManager._instance.Priest;
            //StartCoroutine(player2SightDetection.ResetLineRenderer(Player2));
            return player2ref;
        }
        return null;
    }

    public void UpdateSightVisuals(float percentage, LineType lineType)
    {
        player1SightRenderer.DrawSightDetection(percentage, lineType, Player1);
        player2SightRenderer.DrawSightDetection(percentage, lineType, Player2);
    }

    public void SetSightVisuals(bool enable)
    {
        EnableFov(false);
    }

    public void EnableFov(bool enable)
    {
        if(need2DFOV)
            fieldOfView2DGO.SetActive(enable);
        else
            fieldOfViewGO.SetActive(enable);
    }

    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            //Debug.Log("Enemy " + gameObject.name + " dieded");
            SetAnimation(AnimationState.Death);

            //Own components
            Destroy(navMeshAgent);
            Destroy(deathScript);
            Destroy(GetComponent<Outline>());
            //Child objects
            foreach (Transform c in transform)
            {
                if (c.GetComponent<Animator>() == null)
                    Destroy(c.gameObject);
            }

            player1SightRenderer.DestroyLine();
            player2SightRenderer.DestroyLine();
            testSightDetection.DestroyLine();
            gameObject.tag = "DeadEnemy";
            Collider[] colliders = GetComponents<Collider>();
            foreach (Collider c in colliders)
                c.isTrigger = true;
            Destroy(this);
        }
    }
    public void KillPlayer(GameObject player)
    {
        DeathScript ds = player.GetComponent<DeathScript>();
        if (ds && !ds.isDead)
        {
            ds.damage = 1;
            //StartCoroutine(player2SightDetection.ResetLineRenderer(Player2)); //TODO: CHECK
            stateMachine.PlayerDied();      //TODO: CHECK
            Debug.Log("Killed player " + player.name);
        }
    }

    public void SetState(StateOption stateOption)
    {
        stateMachine.SetState(stateOption);
    }

    public void UpdateStateIndicator(StateOption stateOption)
    {
        if (stateIndicators == null || stateVisualizer == null)
            return;
        PreviousStateOption = CurrentStateOption;
        CurrentStateOption = stateOption;

        SendToClient_StateChanged();

        stateVisualizer.sprite = stateIndicators.GetIndicator(stateOption);
    }
    #endregion


    #region Distractions

    public void ReceiveDistraction(Distraction distraction)
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

    #endregion

    #region Other player shenanigans

    public void PossessAI(Vector3 position)
    {
        Debug.Log("Going to " + position);
        stateMachine.PlayerTakesControl();
        possessedGoToTarget = position;
    }

    public void VisualizePath()
    {
        if (pathVisualizer)
            pathVisualizer.Visualize(navigator.GetVisualizedPath());
    }

    #endregion

    #region Movement

    public void SetTrackingMovementSpeed(bool enable)
    {
        navMeshAgent.speed = enable ? classSettings.navTrackSpeed : classSettings.navSpeed;
    }

    public void SearchLastSeenPosition()
    {
        if (UtilsClass.IsMinimumVector(lastSeenPosition))
            return;

        SetDestination(lastSeenPosition);

    }

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
        if (nextWaypoint == null)
            return;
        if (currentWaypoint.type == WaypointType.Climb && nextWaypoint.type != WaypointType.Climb)
            navMeshAgent.enabled = true;

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

        //if (isPosessed) Debug.Log("Pos" + position);

        //if (!OnNavMesh.IsPartiallyReachable(transform, position))
         //   return;

        //if(isPosessed) Debug.Log("Pos now" + position);

        navMeshAgent.destination = position;
        navMeshAgent.isStopped = false;
        navMeshAgent.stoppingDistance = navMeshAgent.isOnOffMeshLink ? 0.05f : classSettings.navStoppingDistance;
        //if (isPosessed) Debug.Log("Pos noweeeee" + navMeshAgent.destination);
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

    #region SaveAndLoad

    public void SaveLoaded(Vector3 lastSeenPos, int currentWaypointIndex)
    {
        lastSeenPosition = lastSeenPos;
        currentWaypoint = navigator.GetSavedWaypoint(currentWaypointIndex);
    }

    #endregion

    #region Networking

    public void SendToClient_SightChanged()
    {
        if (ShouldSendToClient)
            ServerSend.SightChanged(enemyNetManager.Id, impairedSightRange, impairedFOV);
    }

    private void SendToClient_StateChanged()
    {
        if (ShouldSendToClient)
            ServerSend.StateChanged(Id, CurrentStateOption);
    }
    private void SendToClient_AnimationChanged()
    {
        if (ShouldSendToClient)
            ServerSend.AnimationChanged(Id, CurrentAnimationState);
    }

    #endregion

    #region Editor stuff
#if UNITY_EDITOR

    public bool isDebuggingDummy = false;

    void OnDrawGizmos()
    {
        DebugWaypoints();
        DrawFovDebug();
    }

    private void DrawFovDebug()
    {
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, 0, 0) * fieldOfViewGO.transform.forward * SightRange);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, FOV / 2, 0) * fieldOfViewGO.transform.forward * SightRange);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.Euler(0, FOV / 2 * -1, 0) * fieldOfViewGO.transform.forward * SightRange);
    }

    private void DebugWaypoints()
    {
        if (waypointGroup != null)
            UnityEditor.Handles.DrawDottedLine(transform.position, waypointGroup.transform.position, 4f);
    }
#endif
    #endregion
}
