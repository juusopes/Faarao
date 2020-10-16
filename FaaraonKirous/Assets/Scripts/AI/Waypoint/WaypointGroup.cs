using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEditor;
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
    private PatrolType patrolType;
    private List<Waypoint> waypoints = new List<Waypoint>();
    public int ai = 0;

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
                if (NavMesh.CalculatePath(navTestTrans.position, trans.position, NavMesh.AllAreas, new NavMeshPath()))
                {
                    Waypoint wp = trans.GetComponent<Waypoint>();
                    if (wp != null)
                    {
                        waypoints.Add(wp);
                    }
                    else
                    {
                        Debug.LogWarning("Ignored a waypoint without Waypoint component: " + trans.position + " name: " + trans.name + ". Click while playing to select!", trans.gameObject);
                        //Object.Destroy(trans.gameObject); Don't destroy so we can select it.
                    }
                }
                else
                {
                    Debug.LogWarning("Ignored a waypoint navmesh found unreachable: " + trans.position + " name: " + trans.name + ". Click while playing to select!", trans.gameObject);
                    //Object.Destroy(trans.gameObject); Don't destroy so we can select it.
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
        return waypoints[index];
    }

    public int GetWaypointCount()
    {
        return waypoints.Count;
    }

    public PatrolType GetPatrolType()
    {
        return patrolType;
    }
}

