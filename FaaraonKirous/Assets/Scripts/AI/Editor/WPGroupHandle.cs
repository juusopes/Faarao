//Create a folder and call it "Editor" if one doesn't already exist. Place this script in it.

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(WaypointGroup))]
class LabelHandle : Editor
{
    void OnSceneGUI()
    {
        WaypointGroup handleExample = (WaypointGroup)target;
        if (handleExample == null)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.fontSize = 32;
        style.alignment = TextAnchor.MiddleCenter;
        Handles.color = Color.blue;
        string message = "How to travel waypoints: " + (int)handleExample.GetPatrolType() + " (" + Enum.GetName(typeof(PatrolType), handleExample.GetPatrolType()) + ")";
        Handles.Label(handleExample.transform.position + Vector3.up * 1.5f, message, style) ;
        }
}