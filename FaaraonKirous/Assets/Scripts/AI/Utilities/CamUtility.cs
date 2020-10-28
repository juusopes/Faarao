using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CamUtility
{
    public static bool IsMouseOverGameWindow()
    {
        return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y);
    }
}
