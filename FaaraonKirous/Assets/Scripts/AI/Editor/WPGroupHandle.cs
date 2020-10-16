//Create a folder and call it "Editor" if one doesn't already exist. Place this script in it.

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

// Create a 180 degrees wire arc with a ScaleValueHandle attached to the disc
// lets you visualize some info of the transform

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


        Handles.BeginGUI();
        if (GUILayout.Button("Reset Area", GUILayout.Width(100)))
        {
             handleExample.ai = 5;
        }
        Handles.EndGUI();

        /*
        Handles.DrawWireArc(handleExample.transform.position,
            handleExample.transform.up,
            -handleExample.transform.right,
            180,
            handleExample.shieldArea);
        handleExample.shieldArea =
            Handles.ScaleValueHandle(handleExample.shieldArea,
                handleExample.transform.position + handleExample.transform.forward * handleExample.shieldArea,
                handleExample.transform.rotation,
                1,
                Handles.ConeHandleCap,
                1);

        */

    }
}