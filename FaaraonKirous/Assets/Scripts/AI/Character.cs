using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    #region Fields
    [Header("AI Class")]
    public AIClass classSettings;

    [Header("Patrol Settings")]
    [Tooltip("\tThe order waypoints are travelled:\n\n" +
        "\t*InOrderOnce: \nTravel in order stop on last waypoint\n" +
        "\t*InOrderLoopBackAndForth : \nLoop 1-2-3-2-1-2-3...\n" +
        "\t*InOrderLoopCircle : \nLoop 1-2-3-1-2-3..\n" +
        "\t*ShorterOnce : \nChoose closest waypoint from the remaining path, stop on last\n" +
        "\t*ShorterBackAndForth : \nChoose closest waypoint from the remaining path, Loop: 1-x-5-x-1-x-5-x-1...\n" +
        "\t*Random : \nJust choose random waypoint")]
    public PatrolType patrolType;
    [Tooltip("All my child gameObjects with component Waypoint and that is nav mesh reachable will be added to a list of patrol route.")]
    [SerializeField]
    private UnityEngine.GameObject waypointGroup;
    private Navigator navigator;

    [Header("General")]
    public Transform sightPosition;

    [HideInInspector]
    public Vector3 chaseTarget;
    [HideInInspector]
    public Vector3 lastSeenPosition;
    private Waypoint currentWaypoint;
    private bool waypointFinished = true;
    private bool waypointTimer = false;

    [HideInInspector]
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    private StateMachine stateMachine;
    #endregion

    #region Generic

    void Awake()
    {
        stateMachine = new StateMachine(this);
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.updateRotation = true;
        InitNavigator();
    }
    private void InitNavigator()
    {
        navigator = new Navigator(this.transform, waypointGroup, patrolType);
        currentWaypoint = navigator.GetFirstWaypoint();
    }

    private void Start()
    {

    }


    private void Update()
    {
        stateMachine.UpdateSM();
    }

    public void UpdateIndicator(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }
    #endregion

    #region Sight

    public bool CanSeePlayer()
    {
        RaycastHit hit = RayCastEyes();
        if (RayCastHitPlayer(hit))
        {
            chaseTarget = hit.transform.position;
            return true;
        }

        return false;
    }

    public bool CanChasePlayer()
    {
        RaycastHit hit = RayCastEyesChaseTarget();
        if (RayCastHitPlayer(hit))
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
        if (nextWaypoint == null)
            navigator = null;
        else
            currentWaypoint = nextWaypoint;
    }

    public void OnWaypoint()
    {
        if (currentWaypoint == null)
            return;

        switch(currentWaypoint.type)
        {
            case WaypointType.WalkPast :
                waypointFinished = true;
                SetDestination(currentWaypoint.transform.position);
                break;
            case WaypointType.GuardForDuration:
                SetDestination(currentWaypoint.transform.position);
                if (HasReachedDestination())
                    SearchRotate();

                if (HasReachedDestination() && waypointTimer == false && waypointFinished != false)
                {
                    waypointTimer = true;
                    waypointFinished = false;
                    StartCoroutine("startWaypointTimer", currentWaypoint.GetGuardDuration());
                }
                break;
            case WaypointType.GuardForEver:
                SetDestination(currentWaypoint.transform.position);
                if (HasReachedDestination())
                    SearchRotate();
                waypointFinished = false;
                break;
        }
    }

    private IEnumerator startWaypointTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        waypointFinished = true;
    }
    #endregion

    #region Navmesh

    public void SetDestination(Vector3 position)
    {
        if (position != null && navMeshAgent.destination != position)
        {
            navMeshAgent.destination = position;
            navMeshAgent.isStopped = false;
        }
    }

    public void StopDestination()
    {
        navMeshAgent.isStopped = true;
    }

    public bool HasFinishedWaypointTask()
    {
        return waypointFinished && HasReachedDestination();
    }

    public bool HasReachedDestination()
    {
        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending;
    }
    #endregion

    #region RayCaster

    public RaycastHit RayCastEyesChaseTarget()
    {
        RaycastHit hit;
        Vector3 enemyToTarget = chaseTarget - sightPosition.transform.position;

        Debug.DrawRay(sightPosition.transform.position, enemyToTarget, Color.red, 1f);


        Physics.Raycast(sightPosition.transform.position, enemyToTarget, out hit, classSettings.sightRange);

        return hit;
    }

    public RaycastHit RayCastEyes()
    {
        Debug.DrawRay(sightPosition.transform.position, sightPosition.transform.forward * classSettings.sightRange, Color.green, 1f);

        RaycastHit hit;
        Physics.Raycast(sightPosition.transform.position, sightPosition.transform.forward, out hit, classSettings.sightRange);

        return hit;
    }

    
    public bool RayCastHitPlayer(RaycastHit hit)
    {
        if (hit.collider == null)
            return false;
        return hit.collider.CompareTag("Player");
    }

    #endregion
}