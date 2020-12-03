using System;
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
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range, Color? color = null)
    {
        RaycastHit rayCastHit = new RaycastHit();
        return GetSamplePoint(globalStart, direction, range, out rayCastHit, color);
    }

    /// <summary>
    /// RAYCASTS IN GLOBAL SPACE !! MESH CALCULATIONS ARE IN LOCAL SCAPE!! Returns local space position. Position is raycast end point at range or hit point.
    /// </summary>
    /// <param name="globalStart"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range, out RaycastHit rayCastHitReturn, Color? color = null)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
        {
            Color colorDir = AreSimilarFloat(direction.y, 0, 0.01f) ? Color.yellow : Color.green;
            if (color.HasValue)
                colorDir = color.Value;
            Debug.DrawRay(globalStart, direction * range, colorDir, RayTime);
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
        //We cannot use closest point on concave colliders, so use raycasting as a show must go on option 
        if (testRayHit.collider.GetType() == typeof(MeshCollider))
        {
            return Vector3.zero;

            //Debug.Log(testRayHit.collider.gameObject);
            Vector3 rayStart = reSampleTest + Vector3.up * -0.2f;
            //ACylinder(rayStart, Color.black);
            Vector3 dir = testRayHit.point - ConvertGlobal(reSampleTest);
            float lenght = dir.magnitude * 1.1f;
            dir.y = 0;
            dir = dir.normalized;
            Vector3 res = ConvertLocal(GetHitPointOnSpecificCollider(rayStart, dir, lenght, testRayHit.collider));
            //ACylinder(res);
            return res;
        }

        return ConvertLocal(testRayHit.collider.ClosestPoint(ConvertGlobal(reSampleTest)));
    }

    private Vector3 GetClosestPointOnColliderWithinYDirection(RaycastHit raycastHit, Vector3 previousVertex, Vector3 sample)
    {
        sample.y = previousVertex.y;
        Vector3 closestOnCollider = GetClosestPointOnCollider(raycastHit, sample);
        if(closestOnCollider == Vector3.zero)
        {
            Debug.Log("Warning did not find closest point!");
            return Vector3.zero;
        }
        //ACylinder(closestOnCollider, Color.blue);
        //if the first approximation was good enough, just let it be let it be let it be
        if (AreVerticallyAligned(sample.normalized * closestOnCollider.magnitude, closestOnCollider)
            && AreSimilarHeight(closestOnCollider, previousVertex))
            return closestOnCollider;

        //Did not find good enough let's use old closest point to get new closest point, then create line from it
        Vector3 sampleRecalculated = sample.normalized * closestOnCollider.magnitude;

        Vector3 closestOnColliderReCalculated = GetClosestPointOnCollider(raycastHit, sampleRecalculated);

        if (closestOnColliderReCalculated == Vector3.zero)
        {
            Debug.Log("Warning did not find closest point!");
            return Vector3.zero;
        }

        if (AreVerticallyAligned(closestOnCollider, closestOnColliderReCalculated))
            return new Vector3(closestOnColliderReCalculated.x, previousVertex.y, closestOnColliderReCalculated.z);

        //ACylinder(closestOnColliderReCalculated, Color.blue);
        Vector3 intersection;
        Vector3 testStartPoint1 = new Vector3(closestOnCollider.x, 0, closestOnCollider.z);
        Vector3 testStartPoint2 = Vector3.zero;
        Vector3 testVec1 = (closestOnColliderReCalculated - closestOnCollider) * 10f;
        Vector3 testVec2 = sample;
        testVec1.y = 0;     //2d
        testVec2.y = 0;     //2d
        //Debug.Log("hit" + raycastHit.collider);
        //ACylinder(testVec1, Color.white);
        //ACylinder(testVec2, Color.yellow);
        //ACylinder(testStartPoint1, Color.black);
        //ACylinder(testStartPoint2, Color.cyan);

        //Get the 2D flattened intersection of sample and out two last closest points
        if (Math3d.LineLineIntersection(out intersection, testStartPoint1, testVec1, testStartPoint2, testVec2))
        {
            return new Vector3(intersection.x, previousVertex.y, intersection.z);
        }
        else
        {
            Debug.LogWarning("Could not find intersection");
        }

        //Debug.Log("Could not find intersection");

        return Vector3.zero;
    }

    /// <summary>
    /// Input local space sample and check if ground exists in downRange
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="downRange"></param>
    /// <returns></returns>
    private Vector3 GetHitPointOnSpecificCollider(Vector3 localSampleStart, Vector3 direction, float range, Collider specificCollider)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(ConvertGlobal(localSampleStart), direction * range, Color.cyan, RayTime);
#endif
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("Direction vector was zero!");
            return Vector3.zero;
        }

        if (specificCollider != null)
        {
            Ray ray = new Ray(ConvertGlobal(localSampleStart), direction);
            RaycastHit hit;
            specificCollider.Raycast(ray, out hit, range);
            if (hit.collider)
                return 
                    hit.point;
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
        return CheckColliderExists(localSampleStart, direction, range, out rayCastHit, Color.white);
    }

    /// <summary>
    /// Input local space sample and check if ground exists in range
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool CheckColliderExists(Vector3 localSampleStart, Vector3 direction, float range, out RaycastHit rayCastHitReturn, Color? color = null)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(ConvertGlobal(localSampleStart), direction * range, color ?? Color.blue, RayTime);
#endif
        return Physics.Raycast(ConvertGlobal(localSampleStart), direction, out rayCastHitReturn, range, RayCaster.viewConeLayerMask);
    }

    private Looking GetVerticalDirection(float xAngle)
    {
        if (AreSimilarFloat(xAngle, 0, 0.01f))
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

    private bool HasHit(RaycastHit raycastHit)
    {
        return raycastHit.collider != null;
    }

    private bool HasNotHit(RaycastHit raycastHit)
    {
        return raycastHit.collider == null;
    }
    private bool AreHittingSameCollider(RaycastHit previousRaycastHit, RaycastHit raycastHit)
    {
        if (raycastHit.collider == null || previousRaycastHit.collider == null)
            return false;
        return raycastHit.collider == previousRaycastHit.collider;
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

    public bool IsClearlyLonger(Vector3 sample1, Vector3 sample2, float tolerance = distanceThreshold)
    {
        return sample2.magnitude - sample1.magnitude > tolerance;
    }

    public bool IsClearlyHigher(Vector3 sample1, Vector3 sample2, float tolerance = verticalThreshold)
    {
        return sample2.y - sample1.y > tolerance;
    }

    public bool IsClearlyLower(Vector3 sample1, Vector3 sample2, float tolerance = verticalThreshold)
    {
        //Debug.Log((end.y - start.y < -verticalThreshold) +" " +start.y + "-" + end.y + "=" + (end.y - start.y));
        return sample2.y - sample1.y < -tolerance;
    }

    public bool AreVerticallyAligned(Vector3 sample1, Vector3 sample2, float tolerance = horizontalThreshold)
    {
        return Mathf.Abs(sample1.x - sample2.x) < tolerance
        && Mathf.Abs(sample1.z - sample2.z) < tolerance;
    }
    public bool AreSimilarOnX(Vector3 sample1, Vector3 sample2, float tolerance = horizontalThreshold)
    {
        return Mathf.Abs(sample1.x - sample2.x) < tolerance;
    }
    public bool AreSimilarOnZ(Vector3 sample1, Vector3 sample2, float tolerance = horizontalThreshold)
    {
        return Mathf.Abs(sample1.z - sample2.z) < tolerance;
    }

    public bool AreSimilarHeight(Vector3 sample1, Vector3 sample2, float tolerance = verticalThreshold)
    {
        return Mathf.Abs(sample1.y - sample2.y) < tolerance;
    }

    public bool AreSimilarLenght(Vector3 sample1, Vector3 sample2, float tolerance = horizontalThreshold)
    {
        return Mathf.Abs(sample1.magnitude - sample2.magnitude) < tolerance;
    }

    public bool AreSimilarLenght(Vector3 sample1, float comparison, float tolerance = horizontalThreshold)
    {
        return Mathf.Abs(sample1.magnitude - comparison) < tolerance;
    }
    public bool AreSimilarFloat(float a, float b, float tolerance)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    public bool AreSimilarYDirection(Vector3 sample1, Vector3 sample2, float tolerance = directionTolerance)
    {
        Vector2 test1 = new Vector2(sample1.x, sample1.z);
        Vector2 test2 = new Vector2(sample2.x, sample2.z);
        Debug.Log("Similarity is " + Vector2.Dot(test1.normalized, test2.normalized));
        return Vector2.Dot(test1, test2) > tolerance;
    }
    #endregion
}