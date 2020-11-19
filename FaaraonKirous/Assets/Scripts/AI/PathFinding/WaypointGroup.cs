using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using System;

public class WaypointGroup : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("\tThe order waypoints are travelled:\n\n" +
    "\t*InOrderOnce: \nTravel in order stop on last waypoint\n" +
    "\t*InOrderLoopBackAndForth : \nLoop 1-2-3-2-1-2-3...\n" +
    "\t*InOrderLoopCircle : \nLoop 1-2-3-1-2-3..\n" +
    "\t*ShorterOnce : \nChoose closest waypoint from the remaining path, stop on last\n" +
    "\t*ShorterBackAndForth : \nChoose closest waypoint from the remaining path, Loop: 1-x-5-x-1-x-5-x-1...\n" +
    "\t*Random : \nJust choose random waypoint")]
    [SerializeField]
    private PatrolType patrolType = 0;
    private List<Waypoint> waypoints = new List<Waypoint>();

    // Start is called before the first frame update
    void Awake()
    {
        Transform navTestTrans = GameObject.FindObjectOfType<NavMeshAgent>().transform;
        Assert.IsNotNull(navTestTrans);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform trans = transform.GetChild(i);
            if (trans != null)
            {
                Waypoint wp = trans.GetComponent<Waypoint>();
                if (wp != null)
                {
                    if (OnNavMesh.IsReachable(navTestTrans, trans.position) || wp.type == WaypointType.Climb)
                    {
                        waypoints.Add(wp);
                    }
                    else
                    {
                        Debug.LogWarning("Ignored a waypoint navmesh found unreachable: " + trans.position + " name: " + trans.name + ". Click while playing to select!", trans.gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning("Ignored a waypoint without Waypoint component: " + trans.position + " name: " + trans.name + ". Click while playing to select!", trans.gameObject);
                }

            }
        }

        if(GetWaypointCount() == 0) 
        {
            Debug.LogWarning("Waypoint group contains no suitable elements: " + transform.position + " name: " + transform.name + ". Click while playing to select!", transform.gameObject);
        }
    }

    public Waypoint GetWaypoint(int index)
    {
        if (waypoints.Count == 0)
            return null;
        if (!IsValidIndex(index))
            return null;
        return waypoints[index];
    }

    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < waypoints.Count;
    }

    public int GetWaypointCount()
    {
        return waypoints.Count;
    }

    public PatrolType GetPatrolType()
    {
        return patrolType;
    }

    public void SetGroupId(int index)
    {
        //WaypointGroupManager .id = index
    }
}

