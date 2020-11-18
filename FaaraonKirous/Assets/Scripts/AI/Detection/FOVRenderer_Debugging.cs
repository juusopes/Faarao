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
    [Header("Debugging (Editor only)")]
    [SerializeField]
    private DebugMode debugMode = DebugMode.All;
    [SerializeField]
    private Material ballMat;
    [SerializeField]
    private Material ballMat2;
    [SerializeField]
    private int maxXIterations = 999;
    [SerializeField]
    private bool debuggingOneFrame;     //Debug only one frame
    [SerializeField]
    private bool debuggingLogging;
    [SerializeField]
    private bool disableReSampling = true;
    [SerializeField]
    private bool drawShapesOnIgnoredSamples = false;

    private bool DebugRayCasts => debugMode == DebugMode.Raycasts || debugMode == DebugMode.All;
    private bool DebugVertexShapes => debugMode == DebugMode.VertexShapes || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private bool DebugRaypointShapes => debugMode == DebugMode.RaypointShapes || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private GameObject vertexPointsParent;
    private GameObject raySamplePointsParent;
    private GameObject randomPointsParent;



    private float rayTime => debuggingOneFrame ? 1000f : 1f;

    private enum DebugMode
    {
        None,
        Raycasts,
        RaypointShapes,
        VertexShapes,
        AllShapes,
        All
    }

    IEnumerator Balls(Vector4[] arr)
    {
        yield return new WaitForSeconds(0f);
        Destroy(raySamplePointsParent);
        raySamplePointsParent = new GameObject("Ray sample points");
        foreach (Vector4 vert in arr)
        {
            GameObject sphere = CreatePrimitive(vert, PrimitiveType.Sphere, new Vector3(0.25f, 0.25f, 0.25f), raySamplePointsParent);
            sphere.GetComponent<Renderer>().material = (int)vert.w == 0 ? ballMat2 : ballMat;
            sphere.name = "Ray sample point";
            // yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    IEnumerator Boxes(Vector4[] arr)
    {
        yield return new WaitForSeconds(0f);
        Destroy(vertexPointsParent);
        vertexPointsParent = new GameObject("Vertex Points");
        foreach (Vector4 vert in arr)
        {
            GameObject cube = CreatePrimitive(vert, PrimitiveType.Cube, new Vector3(0.2f, 0.2f, 0.2f), vertexPointsParent);
            Color color;
            switch (vert.w)
            {
                case (float)SampleType.None:
                    color = Color.black;
                    break;
                case (float)SampleType.Floor:
                    color = Color.blue;
                    break;
                case (float)SampleType.FloorToDownFloor:
                    color = Color.cyan;
                    break;
                case (float)SampleType.FloorToWallCorner:
                    color = new Color(1.0f, 0.64f, 0.0f);
                    break;
                case (float)SampleType.LedgeStartCorner:
                    color = new Color(1.0f, 0.75f, 0.8f);
                    break;
                case (float)SampleType.LedgeAtDownAngle:
                    color = Color.green;
                    break;
                case (float)SampleType.LedgeAtUpAngle:
                    color = Color.magenta;
                    break;
                case (float)SampleType.WallToFloorCorner:
                    color = Color.white;
                    break;
                case (float)SampleType.WallToWall:
                    color = Color.yellow;
                    break;
                case (float)SampleType.EndOfSightRange:
                    color = Color.red;
                    break;
                default:
                    color = Color.gray;
                    break;
            }


            cube.GetComponent<Renderer>().material.color = color;
            cube.name = "Vertex Point: " + (SampleType)vert.w;
            //yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    void ACylinder(Vector3 localPos, Color? color = null)
    {
        if (DebugRaypointShapes)
        {
            GameObject sphere = CreatePrimitive(localPos, PrimitiveType.Cylinder, new Vector3(0.25f, 0.03f, 0.25f), randomPointsParent);
            sphere.GetComponent<Renderer>().material.color = color ?? Color.magenta;
            sphere.name = "Random test point";
        }
    }

    private GameObject CreatePrimitive(Vector3 vert, PrimitiveType type, Vector3 scale, GameObject parentObj)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.transform.SetParent(parentObj.transform, true);
        primitive.transform.position = ConvertGlobal(vert);
        primitive.transform.localScale = scale;
        primitive.layer = 2;
        return primitive;
    }
}

#endif