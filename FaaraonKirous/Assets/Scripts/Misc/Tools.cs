using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static string TypeToString(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.pharaoh:
                return "Pharaoh";
            case ObjectType.priest:
                return "Priest";
            default:
                return "placeholder";
        }
    }
}
