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
    private int maxYIterations = 999;
    [SerializeField]
    private int maxXIterations = 999;
    [SerializeField]
    private bool debuggingOneFrame;     //Debug only one frame
    [SerializeField]
    private bool debuggingLogging;
    [SerializeField]
    private bool disableReSampling = false;
    [SerializeField]
    private bool disableXIterationLimit = false;
    [SerializeField]
    private bool drawShapesOnIgnoredSamples = false;
    [SerializeField]
    private bool drawEdges = false;
    [SerializeField]
    private bool testTime = false;

    private bool DebugRayCasts => debugMode == DebugMode.Raycasts || debugMode == DebugMode.All;
    private bool DebugCleanedVertexShapes => debugMode == DebugMode.CleanedVertexShapes || debugMode == DebugMode.AllVertices || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private bool DebugVertexShapes => debugMode == DebugMode.VertexShapes || debugMode == DebugMode.AllVertices || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private bool DebugRaypointShapes => debugMode == DebugMode.RaypointShapes || debugMode == DebugMode.AllShapes || debugMode == DebugMode.All;
    private GameObject cleanedPointsParent;
    private GameObject vertexPointsParent;
    private GameObject raySamplePointsParent;
    private GameObject randomPointsParent;

    private DeltaTimeTester timeTester;

    private float RayTime => debuggingOneFrame ? 1000f : 1f;

    private enum DebugMode
    {
        None,
        Raycasts,
        RaypointShapes,
        VertexShapes,
        CleanedVertexShapes,
        AllVertices,
        AllShapes,
        All
    }

    private void StartDebug()
    {
        if (DebugRaypointShapes)
        {
            raySamplePoints = new Vector4[yRayCount * xRayCount + 1];
            raySamplePoints[0] = Vector3.zero;
        }

        if (debuggingOneFrame)
            Invoke("UpdateViewCone", 0.1f);     //Call with delay, so objects can reset trans etc.

        timeTester = new DeltaTimeTester();
        TestMaxIterations();
    }

    private void TestMaxIterations()
    {
        for (int i = 0; i < xRayCount; i++)
        {
            float angle = X_FOV / 2 * xIterationCurve.Evaluate((float)i / (xRayCount - 1));
            if (angle < maxXAngle)       //Negatives are up angle{
            {
                Debug.Log("True maximum x iterations: " + i + " at angle: " + angle);
                return;
            }
        }
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

    IEnumerator Boxes(VertexPoint[] arr)
    {
        yield return new WaitForSeconds(0f);
        Destroy(vertexPointsParent);
        vertexPointsParent = new GameObject("Vertex Points");
        for (int i = 0; i < arr.Length; i++)
        {
            VertexPoint p = arr[i];
            GameObject cube = CreatePrimitive(p.vertex, PrimitiveType.Cube, new Vector3(0.2f, 0.2f, 0.2f), vertexPointsParent);
            Color color = GetVertexColor(p.sampleType);

            cube.GetComponent<Renderer>().material.color = color;
            if (p.pairNr == 0)
            {
                cube.name = "i: " + i + "\t y: " + p.y + "\t Vertex Point: " + p.sampleType + " ======================================================";
                cube.transform.localScale = new Vector3(0.2f, 1f, 0.2f);
            }
            else
                cube.name = "i: " + i + "\t n1: " + p.n1 + "\t n2: " + p.n2 + "\t y: " + p.y + "\t ISLAND: " + p.island + "\t Pair: " + p.pairNr + "    \tType: " + p.sampleType;


            //yield return new WaitForSeconds(50f / arr.Length);
        }
    }

    /* [ContextMenu("Update view cone")]
     public void DebugRefereshCone()
     {
         UpdateViewCone();
     }
    */
    void OnDrawGizmos()
    {
        if (drawEdges && Application.isPlaying && vertexPoints != null)
        {
            Vector3 offset = Vector3.up * 0.2f;
            for (int i = 0; i < vertexPoints.Count; i++)
            {
                VertexPoint p = vertexPoints[i];
                if (p.n1 != -1)
                    Gizmos.DrawLine(ConvertGlobal(p.vertex) + offset, ConvertGlobal(vertexPoints[p.n1].vertex) + offset / 2);
                if (p.n2 != -1)
                    Gizmos.DrawLine(ConvertGlobal(p.vertex) + offset, ConvertGlobal(vertexPoints[p.n2].vertex) + offset / 2);
                if (p.n1 != -1 && p.n2 != -1)
                    Gizmos.DrawLine(ConvertGlobal(vertexPoints[p.n1].vertex) + offset, ConvertGlobal(vertexPoints[p.n2].vertex) + offset / 2);
            }
        }
    }

    IEnumerator Capsules(Vector4[] arr)
    {
        yield return new WaitForSeconds(0f);
        Destroy(cleanedPointsParent);
        cleanedPointsParent = new GameObject("Cleaned Vertex Points");
        for (int i = 0; i < arr.Length; i++)
        {
            Vector4 vert = arr[i];
            GameObject cube = CreatePrimitive(vert, PrimitiveType.Capsule, new Vector3(0.2f, 0.2f, 0.2f), cleanedPointsParent);
            Color color = GetVertexColor((SampleType)vert.w);

            cube.GetComponent<Renderer>().material.color = color;
            cube.name = i + " Vertex Point: " + (SampleType)vert.w;
            //yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    void APuck(Vector3 localPos, Color? color = null)
    {
        if (DebugRaypointShapes)
        {
            GameObject sphere = CreatePrimitive(localPos, PrimitiveType.Cylinder, new Vector3(0.25f, 0.03f, 0.25f), randomPointsParent);
            sphere.GetComponent<Renderer>().material.color = color ?? Color.magenta;
            sphere.name = "Ignored ray point";
        }
    }

    void ACylinder(Vector3 localPos, Color? color = null)
    {
        if (DebugRaypointShapes)
        {
            GameObject sphere = CreatePrimitive(localPos, PrimitiveType.Cylinder, new Vector3(0.25f, 0.1f, 0.25f), randomPointsParent);
            sphere.GetComponent<Renderer>().material.color = color ?? Color.yellow;
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
    private Color GetVertexColor(SampleType st)
    {
        Color color;
        switch (st)
        {
            case SampleType.None:
                color = Color.black;
                break;
            case SampleType.Floor:
                color = Color.blue;
                break;
            case SampleType.FloorToDownFloor:
                color = Color.cyan;
                break;
            case SampleType.FloorToWallCorner:
                color = new Color(1.0f, 0.64f, 0.0f);
                break;
            case SampleType.LedgeStartCorner:
                color = new Color(1.0f, 0.75f, 0.8f);
                break;
            case SampleType.LedgeAtDownAngle:
                color = Color.green;
                break;
            case SampleType.LedgeAtUpAngle:
                color = Color.magenta;
                break;
            case SampleType.WallToFloorCorner:
                color = Color.white;
                break;
            case SampleType.WallToWall:
                color = Color.yellow;
                break;
            case SampleType.EndOfSightRange:
                color = Color.red;
                break;
            default:
                color = Color.gray;
                break;
        }

        return color;
    }
}

#endif