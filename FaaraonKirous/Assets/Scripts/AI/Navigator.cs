using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigator
{
    private List<Waypoint> waypoints;
    private PatrolType patrolType;
    private int currentWaypoint = 0;
    private int direction = 1; // -1 or 1
    private Transform startTrans;

    public Navigator(Transform startPos, GameObject wpGroup, PatrolType pt)
    {
        this.waypoints = new List<Waypoint>();
        this.patrolType = pt;
        this.startTrans = startPos;

        for (int i = 0; i < wpGroup.transform.childCount; i++)
        {
            Transform trans = wpGroup.transform.GetChild(i);
            if (trans != null)
            {
                if (NavMesh.CalculatePath(startTrans.position, trans.position, NavMesh.AllAreas, new NavMeshPath()))
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
    }

    public Waypoint GetFirstWaypoint()
    {
        if (waypoints.Count == 0)
            return null;
        if (patrolType == PatrolType.ShorterBackAndForth || patrolType == PatrolType.ShorterOnce)
            return waypoints[GetClosestWaypoint(startTrans)];
        return waypoints[0];
    }

    public Waypoint GetNextWaypoint()
    {
        if (waypoints[currentWaypoint].type == WaypointType.GuardForEver)
            return waypoints[currentWaypoint];

        //Debug.Log("Before:" + currentWaypoint);

        switch (patrolType)
        {
            case PatrolType.InOrderOnce:
                if (currentWaypoint < waypoints.Count - 1)
                    currentWaypoint++;
                break;

            case PatrolType.InOrderLoopCircle:
                currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
                break;

            case PatrolType.InOrderLoopBackAndForth:
                CheckDirection();
                currentWaypoint += direction;
                break;
            case PatrolType.ShorterOnce:
                currentWaypoint = GetClosestWaypoint(waypoints[currentWaypoint].transform);
                break;
            case PatrolType.ShorterBackAndForth:
                CheckDirection();
                currentWaypoint = GetClosestWaypoint(waypoints[currentWaypoint].transform);
                break;
            case PatrolType.Random:
                currentWaypoint = Random.Range(0, waypoints.Count);
                break;
            default:
                break;
        }

        //Debug.Log("After:" + currentWaypoint);

        Waypoint result = null;

        if (IsValidIndex())
            result = waypoints[currentWaypoint];

        return result;
    }

    private int GetClosestWaypoint(Transform testSubject)
    {
        if (!IsValidIndex())
        {
            return -1;
        }

        int closest = -1;
        float distance = Mathf.Infinity;
        Vector3 currentPosition = testSubject.position;

        for(int i = currentWaypoint + direction; i >= 0 && i < waypoints.Count ; i  += direction)
        {
            Vector3 diff = waypoints[i].transform.position - currentPosition;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = i;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void CheckDirection()
    {
        if (currentWaypoint == waypoints.Count - 1 && direction == 1)
            direction = -1;
        else if (currentWaypoint == 0 && direction == -1)
            direction = 1;
    }

    private bool IsValidIndex()
    {
        return currentWaypoint >= 0 && currentWaypoint < waypoints.Count;
    }
}
