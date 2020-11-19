using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.AI;

public class Navigator
{
    private WaypointGroup wpGroup;
    private int currentWaypoint = 0;
    private int direction = 1; // -1 or 1
    private Transform parentTrans;


    private PatrolType patrolType => wpGroup == null ? 0 : wpGroup.GetPatrolType();
    private int waypointCount => wpGroup == null ? 0 : wpGroup.GetWaypointCount();
    private bool IsValidIndex(int index) => wpGroup.IsValidIndex(index);

    public Navigator(Transform parentTrans, WaypointGroup wpGroup)
    {
        this.wpGroup = wpGroup;
        this.parentTrans = parentTrans;
    }

    public Waypoint GetFirstWaypoint()
    {
        if (waypointCount == 0)
            return null;
        //if (patrolType == PatrolType.ShorterBackAndForth || patrolType == PatrolType.ShorterOnce)
        //    return wpGroup.GetWaypoint(GetClosestWaypoint(parentTrans));
        return wpGroup.GetWaypoint(0);
    }

    public Waypoint GetCurrentWaypoint()
    {
        return wpGroup.GetWaypoint(currentWaypoint);
    }

    public Waypoint GetNextWaypoint()
    {
        if (waypointCount == 0)
           return null;
        //Debug.Log("Before:" + currentWaypoint);


        if (IsValidIndex(currentWaypoint))
        {
            if (wpGroup.GetWaypoint(currentWaypoint).type == WaypointType.GuardForEver)
                return wpGroup.GetWaypoint(currentWaypoint);
        }

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
                if (currentWaypoint < waypointCount - 1)
                    currentWaypoint = GetClosestWaypoint(parentTrans);
                break;
            case PatrolType.ShorterBackAndForth:
                CheckDirection();
                currentWaypoint = GetClosestWaypoint(parentTrans);
                break;
            case PatrolType.Random:
                currentWaypoint = Random.Range(0, waypointCount);
                break;
            default:
                break;
        }
        //Debug.Log("After:" + currentWaypoint);


        Waypoint result = null;

        if (IsValidIndex(currentWaypoint))
        {
            result = wpGroup.GetWaypoint(currentWaypoint);
        }


        return result;
    }

    private int GetClosestWaypoint(Transform testSubject)
    {
        if (!IsValidIndex(currentWaypoint))
        {
            return -1;
        }

        int closest = -1;
        float distance = Mathf.Infinity;
        Vector3 currentPosition = testSubject.position;

        for(int i = currentWaypoint + direction; i >= 0 && i < waypointCount ; i  += direction)
        {
            //Debug.Log(i);
            Vector3 diff = wpGroup.GetWaypoint(i).transform.position - currentPosition;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = i;
                distance = curDistance;
            }
        }
        //Debug.Log("Closest" + closest);
        return closest;
    }

    private void CheckDirection()
    {
        if (currentWaypoint == waypointCount - 1 && direction == 1)
            direction = -1;
        else if (currentWaypoint == 0 && direction == -1)
            direction = 1;
    }

    public int[] CopyValues()
    {
        return new int[] { currentWaypoint, direction };
    }
    public void PasteValues(int[] values)
    {
        if (values.Length != 2)
            return;
        currentWaypoint = values[0];
        direction = values[1];
    }

    public Vector3[] GetVisualizedPath()
    {
        //TODO:
        return null;
    }
}
