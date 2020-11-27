﻿// RandomPointOnNavMesh
using UnityEngine;
using UnityEngine.AI;

public static class OnNavMesh
{
    /// <summary>
    /// Get random point from center to range from same height level. Keep range small!
    /// </summary>
    /// <param name="center"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointSameHeight(Vector3 center, float range)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * range;
            Vector3 endPoint = center + new Vector3(randomPoint.x, 0, randomPoint.y);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(endPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }


    /// <summary>
    /// Returns a point with a fixed distance of radius from a random flat angle around center.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointOnUnitCircleSameHeight(Vector3 center, float radius)
    {
        float angle = Random.Range(0f, Mathf.PI * 2);

        for (int i = 0; i < 30; i++)
        {
            Vector2 randomPoint = new Vector2(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius);
            Vector3 endPoint = center + new Vector3(randomPoint.x, 0, randomPoint.y);

            if (IsReachable(center, endPoint))
            {
                return endPoint;
            }
        }
            return center;
    }

    /// <summary>
    /// Get random point from center to range as sphere. Keep range small!
    /// </summary>
    /// <param name="center"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Vector3 GetRandomPointSphere(Vector3 center, float range)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + (Random.insideUnitSphere * range);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return center;
    }

    /// <summary>
    /// Returns true if navObject can traverse to testPosition with any means.
    /// </summary>
    /// <param name="navObject"></param>
    /// <param name="testPosition"></param>
    /// <returns></returns>
    public static bool IsReachable(Transform navObject, Vector3 testPosition)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(testPosition, navObject.position, NavMesh.AllAreas, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Returns true if navObject can traverse to testPosition with any means.
    /// </summary>
    /// <param name="navObject"></param>
    /// <param name="testPosition"></param>
    /// <returns></returns>
    public static bool IsReachable(Vector3 startPosition, Vector3 testPosition)
    {
        return NavMesh.CalculatePath(testPosition, startPosition, NavMesh.AllAreas, new NavMeshPath());
    }
}