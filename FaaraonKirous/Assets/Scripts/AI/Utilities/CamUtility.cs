using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CamUtility
{
    public static bool IsMouseOverGameWindow()
    {
        //return Window.HasCursorFocus();
        //return !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y || Screen.width < Input.mousePosition.x || Screen.height < Input.mousePosition.y);
        return !(-1 >= Input.mousePosition.x || -1 >= Input.mousePosition.y || 1 + Screen.width <= Input.mousePosition.x || 1 + Screen.height <= Input.mousePosition.y);
        //Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        //return screenRect.Contains(Input.mousePosition);
//#if UNITY_EDITOR
//        return Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= UnityEditor.Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= UnityEditor.Handles.GetMainGameViewSize().y - 1;
//#else
//        return Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1);
//#endif

    }
}
