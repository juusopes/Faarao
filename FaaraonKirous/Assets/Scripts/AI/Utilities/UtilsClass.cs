using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilsClass 
{ 
    public static Vector3 GetMinVector()
    {
        return new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    public static bool IsMinimumVector(Vector3 testVector)
    {
        return testVector == new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        //angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }

    public static bool EqualsQuaternion(this Quaternion quatA, Quaternion value, float? acceptableRange = null)
    {
        if (!acceptableRange.HasValue)
            acceptableRange = 1 - Mathf.Epsilon;
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange.Value;
    }

    //Todo: test this ..
    public static Quaternion Closer(this Quaternion orig, Quaternion quatA, Quaternion quatB)
    {
        float a = Quaternion.Dot(orig, quatA);
        float b = Quaternion.Dot(orig, quatB);
        return a > b ? quatA : quatB;
    }
}
