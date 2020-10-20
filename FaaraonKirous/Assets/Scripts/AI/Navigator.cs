using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigator
{
    private WaypointGroup wpGroup;
    private int currentWaypoint = 0;
    private int direction = 1; // -1 or 1
    private Transform startTrans;

    private PatrolType patrolType => wpGroup == null ? 0 : wpGroup.GetPatrolType();
    private int waypointCount => wpGroup == null ? 0 : wpGroup.GetWaypointCount();

    public Navigator(Transform startPos, WaypointGroup wpGroup)
    {
        this.wpGroup = wpGroup;
        this.startTrans = startPos;
    }

    public Waypoint GetFirstWaypoint()
    {
        if (waypointCount == 0)
            return null;
        if (patrolType == PatrolType.ShorterBackAndForth || patrolType == PatrolType.ShorterOnce)
            return wpGroup.GetWaypoint(GetClosestWaypoint(startTrans));
        return wpGroup.GetWaypoint(0);
    }

    public Waypoint GetNextWaypoint()
    {
        if (waypointCount == 0)
           return null;

        if (wpGroup.GetWaypoint(currentWaypoint).type == WaypointType.GuardForEver)
            return wpGroup.GetWaypoint(currentWaypoint);

        //Debug.Log("Before:" + currentWaypoint);

        switch (patrolType)
        {
            case PatrolType.InOrderOnce:
                if (currentWaypoint < waypointCount - 1)
                    currentWaypoint++;
                break;

            case PatrolType.InOrderLoopCircle:
                currentWaypoint = (currentWaypoint + 1) % waypointCount;
                break;

            case PatrolType.InOrderLoopBackAndForth:
                CheckDirection();
                currentWaypoint += direction;
                break;
            case PatrolType.ShorterOnce:
                currentWaypoint = GetClosestWaypoint(wpGroup.GetWaypoint(currentWaypoint).transform);
                break;
            case PatrolType.ShorterBackAndForth:
                CheckDirection();
                currentWaypoint = GetClosestWaypoint(wpGroup.GetWaypoint(currentWaypoint).transform);
                break;
            case PatrolType.Random:
                currentWaypoint = Random.Range(0, waypointCount);
                break;
            default:
                break;
        }

        //Debug.Log("After:" + currentWaypoint);

        Waypoint result = null;

        if (IsValidIndex())
            result = wpGroup.GetWaypoint(currentWaypoint);

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

        for(int i = currentWaypoint + direction; i >= 0 && i < waypointCount ; i  += direction)
        {
            Vector3 diff = wpGroup.GetWaypoint(i).transform.position - currentPosition;
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
        if (currentWaypoint == waypointCount - 1 && direction == 1)
            direction = -1;
        else if (currentWaypoint == 0 && direction == -1)
            direction = 1;
    }

    private bool IsValidIndex()
    {
        return currentWaypoint >= 0 && currentWaypoint < waypointCount;
    }
}
