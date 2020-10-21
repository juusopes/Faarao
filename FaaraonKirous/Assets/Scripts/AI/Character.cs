using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    #region Mono Fields
    [Header("AI Class")]
    public AIClass classSettings;
    [Header("Patrol route")]
    [Tooltip("All my child gameObjects with component Waypoint and that is nav mesh reachable will be added to a list of patrol route.")]
    [SerializeField]
    private WaypointGroup waypointGroup = null;
    [Header("General")]
    public Transform sightPosition;
    #endregion 

    #region Regular fields
    [HideInInspector]
    public Vector3 chaseTarget;
    [HideInInspector]
    public Vector3 lastSeenPosition;
    private Waypoint currentWaypoint;
    private bool waypointFinished = false;
    private bool waypointTimer = false;

    private GameObject[] players = new GameObject[2];

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;

    //Aid scripts
    private Navigator navigator;
    private StateMachine stateMachine;
    private SightDetection player1SightDetection;
    private SightDetection player2SightDetection;
    private OffMeshLinkMovement linkMovement;
    #endregion

    #region Expression bodies
    public GameObject Player1 => players[0];
    public GameObject Player2 => players[1];
    #endregion

    #region Start and update
    void Awake()
    {
        stateMachine = new StateMachine(this);
        player1SightDetection = new SightDetection(gameObject, classSettings.lm, 0.1f);
        player2SightDetection = new SightDetection(gameObject, classSettings.lm, 0.1f);
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.updateRotation = true;
        linkMovement = new OffMeshLinkMovement(transform, navMeshAgent, navMeshAgent.radius, navMeshAgent.height);      //TODO: Check radius and height
    }

    private void Start()
    {
        RefreshPlayers();
        InitNavigator();
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

    #region Sight

    private void TryDetectPlayer()
    {
        player1SightDetection.SimulateSightDetection(CanDetectPlayer(Player1));
        player2SightDetection.SimulateSightDetection(CanDetectPlayer(Player2));
    }

    /// <summary>
    /// Returns true if player is in sight, fov and can be raycasted. Allows sightline simulation to begin, which dictates actually seeing player.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool CanDetectPlayer(GameObject player)
    {
        return PlayerIsInRange(player) && PlayerIsInFov(player) && CanRayCastPlayer(player);
    }

    private bool PlayerIsInRange(GameObject player)
    {
        return Vector3.Distance(player.transform.position, transform.position) <= classSettings.sightRange;
    }

    private bool PlayerIsInFov(GameObject player)
    {
        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, dirToPlayer) < classSettings.fov / 2f)
        {
            return true;
        }
        return false;
    }

    public bool CanRayCastPlayer(GameObject player)
    {
        RaycastHit hit = RayCaster.ToTarget(gameObject, player, classSettings.sightRange);
        if (RayCaster.HitPlayer(hit))
        {
            chaseTarget = hit.transform.position;
            return true;
        }

        return false;
    }

    public void LostTrackOfPlayer()
    {
        lastSeenPosition = chaseTarget;
    }

    #endregion

    #region Movement

    public void SearchRotate()
    {
        if (currentWaypoint == null)
            return;
        MatchRotation(currentWaypoint.transform.rotation, currentWaypoint.GetRotationSpeed());
    }

    private void RotateCircle()
    {
        //Rotates around like a FOOL
        //transform.Rotate(0, classSettings.searchTurnSpeed * Time.deltaTime, 0);
    }

    private void RotateTowards(Transform target)
    {
        if (target == null)
            return;
        Vector3 targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
        transform.LookAt(targetPosition);
    }
    private void MatchRotation(Quaternion targetRotation, float rotateSpeed = 1)
    {
        if (targetRotation == null)
            return;

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
                TestOffLink();
                break;
            case WaypointType.GuardForDuration:
                SetDestination(currentWaypoint.transform.position);
                if (HasReachedDestination())
                    SearchRotate();

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
                    SearchRotate();
                waypointFinished = false;
                break;
            case WaypointType.Climb:
                if (waypointFinished == true)
                {
                    waypointFinished = false;
                    StopDestination();
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
        MatchRotation(currentWaypoint.transform.rotation, currentWaypoint.GetRotationSpeed());

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
        if (position != null && navMeshAgent.destination != position && navMeshAgent.enabled)
        {
            navMeshAgent.destination = position;
            navMeshAgent.isStopped = false;
            navMeshAgent.stoppingDistance = navMeshAgent.isOnOffMeshLink ? 0.05f : classSettings.navStoppingDistance;
        }
    }

    public void StopDestination()
    {
        if (navMeshAgent.enabled)
            navMeshAgent.isStopped = true;
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
