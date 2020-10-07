using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilsClass 
{ 
    public static Vector3 GetVectorFromAngle(float angle)
    {
        //angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static bool Approximately(this Quaternion quatA, Quaternion value, float acceptableRange)
    {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
    }

    //Todo: test this ..
    public static Quaternion Closer(this Quaternion orig, Quaternion quatA, Quaternion quatB)
    {
        float a = Quaternion.Dot(orig, quatA);
        float b = Quaternion.Dot(orig, quatB);
        return a > b ? quatA : quatB;
    }
}
