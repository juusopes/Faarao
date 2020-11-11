using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

#if UNITY_EDITOR

public partial class FOVRenderer
{

    [SerializeField]
    private DebugMode debugMode = DebugMode.All;
    [SerializeField]
    private Material ballMat;
    private bool DebugRayCasts => debugMode == DebugMode.Raycasts || debugMode == DebugMode.All;
    private bool DebugVertexShapes => debugMode == DebugMode.VertexShapes || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private bool DebugRaypointShapes => debugMode == DebugMode.RaypointShapes || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;

    private enum DebugMode
    {
        None,
        Raycasts,
        RaypointShapes,
        VertexShapes,
        AllShapes,
        All
    }

    IEnumerator Balls(Vector3[] arr)
    {
        yield return new WaitForSeconds(0f);
        foreach (Vector3 vert in arr)
        {
            GameObject sphere = CreatePrimitive(vert, PrimitiveType.Sphere, new Vector3(0.4f, 0.4f, 0.4f));
            sphere.GetComponent<Renderer>().material = ballMat;
            yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    IEnumerator Boxes(Vector3[] arr)
    {
        yield return new WaitForSeconds(0f);
        foreach (Vector3 vert in arr)
        {
            GameObject cube = CreatePrimitive(vert, PrimitiveType.Cube, new Vector3(0.35f, 0.35f, 0.35f));
            cube.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.5f);// Color.red;
            yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    void ACylinder(Vector3 localPos, Color color)
    {
        if (DebugRaypointShapes)
        {
            GameObject sphere = CreatePrimitive(localPos, PrimitiveType.Cylinder, new Vector3(0.5f, 0.25f, 0.5f));
            sphere.GetComponent<Renderer>().material.color = color;
        }
    }

    private GameObject CreatePrimitive(Vector3 vert, PrimitiveType type, Vector3 scale)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.transform.position = ConvertGlobal(vert);
        primitive.transform.localScale = scale;
        primitive.layer = 2;
        return primitive;
    }



}

#endif