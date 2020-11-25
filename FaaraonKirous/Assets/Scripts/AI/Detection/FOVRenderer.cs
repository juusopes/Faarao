using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

public partial class FOVRenderer : MonoBehaviour
{
    #region Const strings
    private const string TEXTURE = "_MainText";
    private const string TEXTURE_CONTRAST = "_TextureContrast";
    private const string TEXTURE_COLOR = "_TextureColor";
    private const string BACKGROUND_COLOR = "_MainBackgroundColor";
    private const string FILL_COLOR = "_FillColor";
    private const string FILL_SCALE = "_FillScale";
    private const string INNER_SUBSTRACTION_SCALE = "_InnerSubstractionScale";
    private const string INNER_IMAGE_SUBSTRACTION_SCALE = "_InnerImageSubstractionScale";
    private const string IMAGE_ALPHA = "_ImageAlpha";
    private const string BACKGROUND_ALPHA = "_BackGroundAlpha";
    private const string BLEND_OPACITY = "_BlendOpacity";
    private const string ALPHA_CUTOFF = "_AlphaCutoff";
    private const string POLAR = "_Polar";
    private const string POLAR_RADIALSCALE = "_PolarRadialScale";
    private const string POLAR_LENGHTSCALE = "_PolarLenghtScale";
    private const string MOVEMENT_SPEED = "_MovementSpeed";
    #endregion

    #region Fields and expressions ====================================================================================================================
    Mesh mesh;
    Material material;
    public Character character;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Quaternion lastLocalRotation;
    private Vector3 origin;
    private float yStartingAngle = 0;
    float yAngleIncrease;
    public Vector4[] raySamplePoints;
    private Vector3[] lastColumnSamplePoints;
    private RaycastHit[] lastColumnSampleRays;
    private List<VertexPoint> vertexPoints;
    public Vector2[] uv;
    public int[] triangles;
    int vertexIndex = 1;
    int triangleIndex = 0;
    SampleType lastSampleType = 0;
    int yIteration, xIteration, vertexPair, island;

    //Tweakable values
    public AnimationCurve xIterationCurve;
    public int yRayCount;// = (8) + 1;    // Horizontal angles ODD NUMBER 
    public int xRayCount;// = (8) + 1;    // Vertical angles ODD NUMBER 
    private const float maxXAngle = (400.0000001f) * -1;   //Negative number is up!
    private const float ledgeStep = 0.3f;           //How big is the iterative step for searching next floor collider
    private const float ledgeSightBlockingHeight = 0.3f;
    private const float enemySightHeight = 2.785f;
    private const float playerHeight = 2.785f;
    private const float yTolerance = 0.2f;                  //Difference between sampled corner and raycasted corner with added tolerance in case floor is not evenly shaped
    private const float cornerCheckAngle = 80f;             //In what x angle is corner check operated (the rounder ledge edges the smaller value)
    private const float directionTolerance = 0.995f;

    private const float verticalThreshold = 0.1f;
    private const float horizontalThreshold = 0.1f;
    private const float distanceThreshold = 0.5f;
    private const float SlopeTolerance = 0.5f;         //Dotproduct for hitnormal
    private const float vertexYOffset = 0.1f;   //0.1f how much the mesh is raised from the ground
    private const float mergeDistanceThreshold = 5f;

    private enum Looking
    {
        Down,
        ZeroAngle,
        Up
    }

    private enum SampleType
    {
        None,               //Black
        Floor,              //Blue
        FloorToDownFloor,   //Cyan
        FloorToWallCorner,  //Orange
        WallToWall,         //Yellow
        WallToFloorCorner,  //White
        EndOfSightRange,    //Red
        LedgeStartCorner,   //Pink   
        LedgeAtDownAngle,   //Green   
        LedgeAtUpAngle      //Magenta
    }

    [System.Serializable]
    private class VertexPoint
    {
        public Vector3 vertex;
        public Vector2 uv;
        public SampleType sampleType;
        public int n1 = -1; //Neighbours for triangle creation
        public int n2 = -1; //Neighbours for triangle creation
        public int pairNr;
        public int island;
        public int y;
    }

    private float Y_FOV => character.FOV;
    private float X_FOV => 140f;
    private float SightRange => character.SightRange;
    private float SightRangeCrouching => character.SightRangeCrouching;
    private Vector3 ConvertGlobal(Vector3 inVec) => transform.TransformPoint(inVec);
    private Vector3 ConvertLocal(Vector3 inVec) => transform.InverseTransformPoint(inVec);
    private Vector3 LastAddedVertex => vertexPoints.Count > 0 ? LastAddedVertexPoint.vertex : Vector3.positiveInfinity;
    private VertexPoint LastAddedVertexPoint => vertexPoints.Count > 0 ? vertexPoints[vertexPoints.Count - 1] : null;
    private Vector3 FirstVertex => vertexPoints.Count > 0 ? vertexPoints[0].vertex : Vector3.positiveInfinity;
    private Vector3 MeshOrigin => Vector3.zero - Vector3.up * playerHeight;

    private bool IsStartingType(SampleType st) => st == SampleType.None || st == SampleType.Floor || st == SampleType.FloorToDownFloor || st == SampleType.WallToFloorCorner || st == SampleType.LedgeStartCorner;
    private bool IsEndingType(SampleType st) => st == SampleType.EndOfSightRange || st == SampleType.FloorToWallCorner || st == SampleType.LedgeAtDownAngle || st == SampleType.LedgeAtUpAngle || st == SampleType.WallToWall;

    #endregion

    #region Start and run =========================================================================================================
    void Awake()
    {
        Assert.IsNotNull(character, "Character is not set!");
        if (yRayCount % 2 == 0)
            yRayCount += 1;
        if (xRayCount % 2 == 0)
            xRayCount += 1;
        Assert.IsTrue(yRayCount % 2 == 1, "Raycount must be odd number");
        Assert.IsTrue(xRayCount % 2 == 1, "Raycount must be odd number");
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.identity;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        material = GetComponent<Renderer>().material;
        origin = Vector3.zero;
        lastColumnSamplePoints = new Vector3[xRayCount];
        lastColumnSampleRays = new RaycastHit[xRayCount];
#if UNITY_EDITOR
        if (DebugRaypointShapes)
        {
            raySamplePoints = new Vector4[yRayCount * xRayCount + 1];
            raySamplePoints[0] = Vector3.zero;
        }
#endif

#if UNITY_EDITOR
        if (debuggingOneFrame)
            Invoke("UpdateViewCone", 0.1f);     //Call with delay, so objects can reset trans etc.

        timeTester = new DeltaTimeTester();
#endif

        //SaveAsset();
        /*
        float[] values = new float[xRayCount];
        float value, angle;
        for (int i = 0; i < xRayCount; i++)
        {
            float per = (float)i / (xRayCount - 1);
            //if (i == xRayCount / 2 + 1 )
            //   value = 0;
            //else
            value = xIterationCurve.Evaluate((float)i / (xRayCount - 1));
            angle = X_FOV / 2 * value;
            Debug.Log(angle);
            //Debug.Log(value + " " + per + " " + i);
        }*/
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (debuggingOneFrame)
            return;
#endif
        if (ShouldUpdateViewCone())
            UpdateViewCone();

        RefreshRendReasons();
    }

    private void RefreshRendReasons()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastLocalRotation = transform.localRotation;
    }

    private void UpdateViewCone()
    {
#if UNITY_EDITOR
        Destroy(randomPointsParent);
        randomPointsParent = new GameObject("Random test points");

        if (testTime)
            timeTester.StopWatch();
#endif
        SetOrigin(transform.position);
        SetAimDirection(transform.forward, Y_FOV, X_FOV);
        UpdateMesh();
    }

    private bool ShouldUpdateViewCone()
    {
        return lastPosition != transform.position || !lastRotation.EqualsQuaternion(transform.rotation) || !lastLocalRotation.EqualsQuaternion(transform.localRotation);
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection, float yFovIn, float xFovIn)
    {
        yStartingAngle = transform.rotation.eulerAngles.y + yFovIn / -2f;
        //xStartingAngle = xFovIn / 2f;
        yAngleIncrease = yFovIn / (yRayCount - 1);
        //Debug.Log(xStartingAngle+" "+   xAngleIncrease);
    }

    #endregion

    #region Create viewcone =========================================================================================================================================================

    void UpdateMesh()
    {
        InitMesh();
        InitIterations();
        IterateY();
        CreateMesh();
    }

    private void InitMesh()
    {
        vertexPoints = new List<VertexPoint>();
        //AddVertexPoint(MeshOrigin, SampleType.None);
        //Debug.Log(MeshOrigin);
    }

    private void InitIterations()
    {
        vertexIndex = 1;
        triangleIndex = 0;
    }

    private void AddVertexPoint(Vector3 sample, SampleType sampleType)
    {
#if UNITY_EDITOR
        if (debuggingLogging) Debug.Log("<i><size=10>\t\t\t\t\t\t\tAdded vertex: " + sample + " with type: <b><size=12>" + sampleType + "</size></b></size></i>" + " with index: " + vertexPoints.Count);
#endif
        lastSampleType = sampleType;

        VertexPoint vertNew = new VertexPoint();
        VertexPoint vertPrev = LastAddedVertexPoint;
        vertNew.vertex = sample;
        vertNew.uv = GetVertexUV(yIteration, sample);
        vertNew.sampleType = sampleType;
        vertNew.y = yIteration;

        AddNeighbourVertices(sample, sampleType, vertNew, vertPrev);

        vertexPoints.Add(vertNew);
    }

    private void AddNeighbourVertices(Vector3 sample, SampleType sampleType, VertexPoint vertNew, VertexPoint vertPrev)
    {
        if (vertPrev != null && IsEndingType(sampleType) && AreSimilarHeight(sample, vertPrev.vertex) && IsStartingType(vertPrev.sampleType))
        {
            vertexPair++;
            island++;
            vertNew.pairNr = vertexPair + yIteration * xRayCount;
            vertPrev.pairNr = vertexPair + yIteration * xRayCount;
            vertNew.island = island;
            vertPrev.island = island;

            if (yIteration > 0)
            {
                int v1Prev, v2Prev;
                if (GetMathingPairNr(yIteration - 1, vertPrev, vertNew, out v1Prev, out v2Prev))
                {
                    vertexPoints[v1Prev].n1 = v2Prev;
                    vertexPoints[v1Prev].n2 = vertexPoints.Count - 1;
                    vertexPoints[v2Prev].n1 = vertexPoints.Count;
                    vertexPoints[v2Prev].n2 = vertexPoints.Count - 1;
                    vertPrev.island = vertexPoints[v1Prev].island;
                    vertNew.island = vertexPoints[v2Prev].island;
                }
                else
                {
                    Debug.Log("no pair");
                }
            }
        }
    }

    private void ReplaceVertexPointVertex(VertexPoint vertexPoint, Vector3 sample)
    {
#if UNITY_EDITOR
        if (debuggingLogging) Debug.Log("<i><size=10>\t\t\t\t\t\t\tReplaced vertexpoint: " + vertexPoint + " vertex: " + vertexPoint.vertex + " with : <b><size=12>" + sample + "</size></b></size></i>");
#endif
        vertexPoint.vertex = sample;
        vertexPoint.uv = GetVertexUV(yIteration, sample);
    }

    private bool GetMathingPairNr(int y, VertexPoint v1, VertexPoint v2, out int v1Prev, out int v2Prev)
    {
        for (int i = vertexPoints.Count - 1; i > 0; i--)
        {
            if (vertexPoints[i].y < y)
               break;

            if (vertexPoints[i].y > y)
                continue;

            if (vertexPoints[i].pairNr == 0)
                continue;

            if (vertexPoints[i - 1].pairNr != vertexPoints[i].pairNr)
                continue;

            //Debug.Log("MOi" + AreSimilarLenght(vertexPoints[i - 1].vertex, v1.vertex, 2f) + " " + AreSimilarLenght(vertexPoints[i].vertex, v2.vertex, 2f));

            if (ArePairEdges(vertexPoints[i - 1], vertexPoints[i], v1, v2, i))
            {
                v1Prev = i - 1;
                v2Prev = i;
                return true;
            }
        }
        v1Prev = 0;
        v2Prev = 0;
        return false;
    }

    private bool ArePairEdges(VertexPoint v1prev, VertexPoint v2prev, VertexPoint v1, VertexPoint v2, int i)
    {
        bool lenght = AreSimilarLenght(v1prev.vertex, v1.vertex, mergeDistanceThreshold) || AreSimilarLenght(v2prev.vertex, v2.vertex, mergeDistanceThreshold);
        bool height = AreSimilarHeight(v1prev.vertex, v1.vertex) && AreSimilarHeight(v2prev.vertex, v2.vertex);
        //Debug.Log("Index: " + i + " Pair: " + v1prev.pairNr + " " + v1.pairNr + " " + v2prev.pairNr + " " +v2.pairNr + " same lenght: " + lenght + " same height: " + height);;
        return lenght && height;
    }

    private Vector2 GetVertexUV(int y, Vector3 vertex)
    {
        vertex.y = 0;
        float uvLenght = Vector3.Magnitude(vertex) / SightRange;
        //Create Cone shaped uvs
        float uvAngle = y * yAngleIncrease;
        float rad = uvAngle * Mathf.Deg2Rad;
        Vector2 vertexUV = new Vector2(Mathf.Sin(rad) * uvLenght, Mathf.Cos(rad) * uvLenght);           //Uv map as unit circle starting from (1,0) rotating based on yAngleIncrease, so first is y=0 and then it increases with unit cicle
        return vertexUV;
    }

    private void CreateTriangleCone(int index)
    {
        if (index > 2)
        {
            triangles[triangleIndex + 0] = 0;
            triangles[triangleIndex + 1] = index - 1;
            triangles[triangleIndex + 2] = index;

            triangleIndex += 3;
        }
    }

    private void CreateTriangleQuad(int index)
    {
        if (index % 2 == 1)
        {
            triangles[triangleIndex + 0] = index - 1;
            triangles[triangleIndex + 1] = index;
            triangles[triangleIndex + 2] = index + 1;
        }
        else
        {
            triangles[triangleIndex + 0] = index - 1;
            triangles[triangleIndex + 1] = index + 1;
            triangles[triangleIndex + 2] = index;
        }
        triangleIndex += 3;
    }

    private void CreateTriangle(int index)
    {
        if (vertexPoints[index].n1 != -1 && vertexPoints[index].n2 != -1)
        {
            triangles[triangleIndex + 0] = index;
            triangles[triangleIndex + 1] = vertexPoints[index].n1;
            triangles[triangleIndex + 2] = vertexPoints[index].n2;
        }

        triangleIndex += 3;
    }

    private bool IsSecondPair(SampleType st1, SampleType st2)
    {
        //if(st1 == SampleType.None || st1 == SampleType.Floor && st2 == SampleType.FloorToWallCorner || st2 == SampleType.LedgeAtDownAngle)
        if (st1 == SampleType.WallToFloorCorner || st1 == SampleType.LedgeStartCorner
            && st2 == SampleType.EndOfSightRange || st2 == SampleType.FloorToWallCorner || st2 == SampleType.LedgeAtDownAngle || st2 == SampleType.LedgeAtUpAngle || st2 == SampleType.WallToWall)
            return true;

        return false;
    }


    private void CreateMesh()
    {
        mesh.Clear();
        /* List<Vector4> vertexList = new List<Vector4>();
         List<Vector4> vertexList2 = new List<Vector4>();

         vertexList.Add(vertexPoints[0]);
         vertexPoints.RemoveAt(0);
         float firstY = vertexList[0].y;
         for (int i = 0; i < vertexPoints.Count; i++)
         {
             if (AreSimilarFloat(firstY, vertexPoints[i].y, verticalThreshold))
             {
                 vertexList.Add(vertexPoints[i]);
                 vertexPoints.RemoveAt(i);
                 i--;
             }
         }

         for (int i = 1; i < vertexList.Count; i++)
         {
             if (IsSecondPair((SampleType)vertexList[i - 1].w, (SampleType)vertexList[i].w))
             {
                 vertexList2.Add(vertexPoints[i - 1]);
                 vertexList2.Add(vertexPoints[i]);
                 vertexList.RemoveAt(i - 1);
                 vertexList.RemoveAt(i);
                 i--;
                 i--;
             }
         }

         */
        //Debug.Log(vertexList.Count);

        //if (vertexList.Count < 3)
        //    Debug.LogWarning("Lil len");

        int arrSize = vertexPoints.Count;

        Vector3[] vertices = new Vector3[arrSize];
        uv = new Vector2[arrSize];
        //uv[0] = Vector2.zero;
        triangles = new int[arrSize * 3];


        for (int i = 0; i < arrSize; i++)
        {
            vertices[i] = vertexPoints[i].vertex + Vector3.up * vertexYOffset;
            CreateTriangle(i);
            uv[i] = vertexPoints[i].uv;
        }

        /*
        for (int i = 0; i < vertices.Length; i++)
        {
            //uv[i] = new Vector2(1, 1);
            //normals[i] = Vector3.up;

            if (i > 0 && i < vertices.Length - 1)
            {
               // CreateTriangleQuad(i);

                /*if (i == 1)
                    CreateTriangleQuad(i);
                else if (AreSimilarLenght(vertices[i], vertices[i - 2], 2f))
                    CreateTriangleQuad(i);
                else
                    Debug.Log("Index " + i + " Ignored" + vertices[i].magnitude + " " + vertices[i - 2].magnitude);*/
        // }

        //}



#if UNITY_EDITOR
        if (DebugRaypointShapes)
            StartCoroutine(Balls(raySamplePoints));
        if (DebugVertexShapes)
            StartCoroutine(Boxes(vertexPoints.ToArray()));
        //if (DebugCleanedVertexShapes)
        //    StartCoroutine(Capsules(vertexList.ToArray()));

        if (testTime)
            timeTester.StopWatch();
#endif

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        //mesh.RecalculateNormals();
    }


    #endregion

    #region Shader values ==================================================================================================================================

    public void UpdateMaterialProperties(LineType background, LineType fill, float percentage)
    {
        //material.SetFloat(THICKNESS, outerThickness);
    }

    #endregion

    #region 2D version =====================================================================================================================================

    /*
    void UpdateMesh2D()
    {
        yAngle = yStartingAngle;
        vertices[0] = Vector3.zero;
        uv[0] = Vector2.zero;



        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= yRayCount; i++)
        {
            Vector3 vertex;
            Vector3 direction = Quaternion.Euler(0, yAngle, 0) * Vector2.right;
            //Debug.DrawRay(origin, direction * viewDistance, Color.green, 10f);
            RaycastHit raycastHit;
            float uvLenght;

            if (Physics.Raycast(origin, direction, out raycastHit, SightRange, RayCaster.viewConeLayerMask))
            {
                vertex = ConvertLocal(origin + direction * raycastHit.distance);
                uvLenght = raycastHit.distance / SightRange;
            }
            else
            {
                vertex = ConvertLocal(origin + direction * SightRange);
                uvLenght = 1;
            }


            //Create Cone shaped uvs
            float uvAngle = i * yAngleIncrease;
            float rad = uvAngle * Mathf.Deg2Rad;
            uv[vertexIndex] = new Vector2(Mathf.Sin(rad) * uvLenght, Mathf.Cos(rad) * uvLenght);


            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            yAngle -= yAngleIncrease;
        }

        //foreach (Vector3 vert in vertices)
        //{
        //   GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //   sphere.transform.position = vert + transform.position;
        //}

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }
    */

    #endregion

}