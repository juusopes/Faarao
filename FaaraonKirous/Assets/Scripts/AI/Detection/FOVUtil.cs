using UnityEngine;

public static class FOVUtil
{
    private static float verticalThreshold = 0.1f;
    private static float horizontalThreshold = 0.1f;
    private static float stepThreshold = 0.1f;

    private static float SlopeTolerance = 0.5f;         //Dotproduct for hitnormal

    public static bool IsFloorToFloor(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsUpFacing(raycastHit1) && HitPointIsUpFacing(raycastHit2);
    }

    public static bool IsFloorToWall(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsUpFacing(raycastHit1) && HitPointIsSideFacing(raycastHit2);
    }
    public static bool IsWallToFloor(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsSideFacing(raycastHit1) && HitPointIsUpFacing(raycastHit2);
    }
    public static bool IsWallToWall(RaycastHit raycastHit1, RaycastHit raycastHit2)
    {
        return HitPointIsSideFacing(raycastHit1) && HitPointIsSideFacing(raycastHit2);
    }

    public static bool HitPointIsUpFacing(RaycastHit raycastHit)
    {
        return Vector3.Dot(raycastHit.normal, Vector3.up) > SlopeTolerance;
    }

    public static bool HitPointIsSideFacing(RaycastHit raycastHit)
    {
        float dot = Vector3.Dot(raycastHit.normal, Vector3.up);
        return dot > -SlopeTolerance && dot < SlopeTolerance;
    }

    public static bool IsClearlyLonger(Vector3 start, Vector3 end)
    {
        return end.magnitude - start.magnitude > stepThreshold;
    }

    public static bool IsClearlyHigher(Vector3 start, Vector3 end)
    {
        return end.y - start.y > verticalThreshold;
    }

    public static bool IsClearlyLower(Vector3 start, Vector3 end)
    {
        //Debug.Log((end.y - start.y < -verticalThreshold) +" " +start.y + "-" + end.y + "=" + (end.y - start.y));
        return end.y - start.y < -verticalThreshold;
    }

    public static bool AreVerticallyAligned(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold
        && Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }
    public static bool AreSimilarOnX(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold;
    }
    public static bool AreSimilarOnZ(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }

    public static bool AreSimilarHeight(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.y - sample2.y) < verticalThreshold;
    }

    public static bool AreSimilarLenght(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.magnitude - sample2.magnitude) < horizontalThreshold;
    }

    public static bool AreSimilarLenght(Vector3 sample1, float comparison)
    {
        return Mathf.Abs(sample1.magnitude - comparison) < horizontalThreshold;
    }
}



