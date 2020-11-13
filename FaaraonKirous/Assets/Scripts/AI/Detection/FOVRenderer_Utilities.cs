﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

public partial class FOVRenderer
{
    #region General Utilities ===============================================================================================================================

    /// <summary>
    /// RAYCASTS IN GLOBAL SPACE !! MESH CALCULATIONS ARE IN LOCAL SCAPE!! Returns local space position. Position is raycast end point at range or hit point. 
    /// </summary>
    /// <param name="globalStart"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range)
    {
        RaycastHit rayCastHit = new RaycastHit();
        return GetSamplePoint(globalStart, direction, range, out rayCastHit);
    }

    /// <summary>
    /// RAYCASTS IN GLOBAL SPACE !! MESH CALCULATIONS ARE IN LOCAL SCAPE!! Returns local space position. Position is raycast end point at range or hit point.
    /// </summary>
    /// <param name="globalStart"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range, out RaycastHit rayCastHitReturn)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
        {
            Color color = direction.y == 0 ? Color.yellow : Color.green;
            Debug.DrawRay(globalStart, direction * range, color, 1000f);
        }

#endif

        RaycastHit raycastHit;

        if (Physics.Raycast(globalStart, direction, out raycastHit, range, RayCaster.viewConeLayerMask))
        {
            rayCastHitReturn = raycastHit;
            return ConvertLocal(globalStart + direction * raycastHit.distance);
        }
        else
        {
            rayCastHitReturn = new RaycastHit();
            return ConvertLocal(globalStart + direction * range);
        }
    }

    private Vector3 GetClosestPointOnCollider(RaycastHit testRayHit, Vector3 reSampleTest)
    {
        if (testRayHit.collider == null || reSampleTest == Vector3.zero)
        {
            Debug.LogWarning("No can do closest!" + testRayHit.collider);
            return Vector3.zero;
        }

        return ConvertLocal(testRayHit.collider.ClosestPoint(ConvertGlobal(reSampleTest)));
    }

    /// <summary>
    /// Input local space sample and check if ground exists in downRange
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="downRange"></param>
    /// <returns></returns>
    private Vector3 GetSpecificColliderHitPoint(Vector3 localSampleStart, Vector3 direction, float range, Collider specificCollider)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(ConvertGlobal(localSampleStart), direction * range, Color.blue, 1000f);
#endif
        if (specificCollider != null)
        {
            Ray ray = new Ray(ConvertGlobal(localSampleStart), direction);
            RaycastHit hit;
            specificCollider.Raycast(ray, out hit, range);
            if (hit.collider)
                return hit.point;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Input local space sample and check if ground exists in range
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool CheckColliderExists(Vector3 localSampleStart, Vector3 direction, float range)
    {
        RaycastHit rayCastHit = new RaycastHit();
        return CheckColliderExists(localSampleStart, direction, range, out rayCastHit);
    }

    /// <summary>
    /// Input local space sample and check if ground exists in range
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool CheckColliderExists(Vector3 localSampleStart, Vector3 direction, float range, out RaycastHit rayCastHitReturn)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(ConvertGlobal(localSampleStart), direction * range, Color.blue, 1000f);
#endif
        return Physics.Raycast(ConvertGlobal(localSampleStart), direction, out rayCastHitReturn, range, RayCaster.viewConeLayerMask);
    }

    private Looking GetVerticalDirection(float xAngle)
    {
        if (xAngle == 0)
            return Looking.ZeroAngle;
        if (xAngle < 0)
            return Looking.Up;
        return Looking.Down;
    }



    #endregion

    #region Comparisons
    public bool IsFloorToFloor(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsUpFacing(raycastHit1) && HitPointIsUpFacing(raycastHit2);
    }

    public bool IsFloorToWall(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsUpFacing(raycastHit1) && HitPointIsSideFacing(raycastHit2);
    }
    public bool IsWallToFloor(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsSideFacing(raycastHit1) && HitPointIsUpFacing(raycastHit2);
    }
    public bool IsWallToWall(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsSideFacing(raycastHit1) && HitPointIsSideFacing(raycastHit2);
    }

    public bool HitPointIsUpFacing(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null)
            return false;
        return Vector3.Dot(raycastHit.normal, Vector3.up) > SlopeTolerance;
    }

    public bool HitPointIsSideFacing(RaycastHit raycastHit)
    {
        if (raycastHit.collider == null)
            return false;
        float dot = Vector3.Dot(raycastHit.normal, Vector3.up);
        return dot > -SlopeTolerance && dot < SlopeTolerance;
    }

    public bool IsClearlyLonger(Vector3 start, Vector3 end)
    {
        return end.magnitude - start.magnitude > distanceThreshold;
    }

    public bool IsClearlyHigher(Vector3 start, Vector3 end)
    {
        return end.y - start.y > verticalThreshold;
    }

    public bool IsClearlyLower(Vector3 start, Vector3 end)
    {
        //Debug.Log((end.y - start.y < -verticalThreshold) +" " +start.y + "-" + end.y + "=" + (end.y - start.y));
        return end.y - start.y < -verticalThreshold;
    }

    public bool AreVerticallyAligned(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold
        && Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }
    public bool AreSimilarOnX(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold;
    }
    public bool AreSimilarOnZ(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }

    public bool AreSimilarHeight(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.y - sample2.y) < verticalThreshold;
    }

    public bool AreSimilarLenght(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.magnitude - sample2.magnitude) < horizontalThreshold;
    }

    public bool AreSimilarLenght(Vector3 sample1, float comparison)
    {
        return Mathf.Abs(sample1.magnitude - comparison) < horizontalThreshold;
    }

    public bool AreSimilarYDirection(Vector3 sample1, Vector3 sample2)
    {
        sample1 = sample1.normalized;
        sample2 = sample2.normalized;
        Vector2 test1 = new Vector2(sample1.x, sample1.z);
        Vector2 test2 = new Vector2(sample2.x, sample2.z);
        Debug.Log("Similarity is " + Vector2.Dot(test1, test2));
        return Vector2.Dot(test1, test2) > directionTolerance;
    }
    #endregion
}