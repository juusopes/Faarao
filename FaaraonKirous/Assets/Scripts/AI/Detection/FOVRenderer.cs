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
    private Vector3 origin;
    private float yStartingAngle = 0;
    private float xStartingAngle = 0;
    float yAngleIncrease;
    float xAngleIncrease;
    public Vector3[] raySamplePoints;
    private Vector3[] lastColumnSamplePoints;
    private RaycastHit[] lastColumnSampleRays;
    public List<Vector4> vertexPoints;
    public Vector2[] uv;
    public int[] triangles;
    int vertexIndex = 1;
    int triangleIndex = 0;
    SampleType lastSampleType = 0;

    //Tweakable values
    private const int yRayCount = (0) + 1;    // Horizontal angles ODD NUMBER 
    private const int xRayCount = (10) + 1;    // Vertical angles ODD NUMBER 
    private const float maxXAngle = (90.0000001f) * -1;   //Negative number is up!
    private const float ledgeStep = 0.25f;           //How big is the iterative step for searching next floor collider
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
        LedgeAtDownAngle,   //Green   
        LedgeAtUpAngle      //Magenta
    }


    private float yFOV => character.FOV;
    private float xFOV => 90f;
    private float SightRange => character.SightRange;
    private float SightRangeCrouching => character.SightRangeCrouching;
    private Vector3 ConvertGlobal(Vector3 inVec) => transform.TransformPoint(inVec);
    private Vector3 ConvertLocal(Vector3 inVec) => transform.InverseTransformPoint(inVec);
    private Vector3 LastAddedVertex => vertexPoints[vertexPoints.Count - 1];

    private Vector3 MeshOrigin => Vector3.zero - Vector3.up * playerHeight;
    #endregion

    #region Start and run =========================================================================================================
    void Awake()
    {
        Assert.IsNotNull(character, "Character is not set!");
        Assert.IsTrue(yRayCount % 2 == 1, "Raycount must be odd number");
        Assert.IsTrue(xRayCount % 2 == 1, "Raycount must be odd number");
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        material = GetComponent<Renderer>().material;
        origin = Vector3.zero;
        lastColumnSamplePoints = new Vector3[xRayCount];
        lastColumnSampleRays = new RaycastHit[xRayCount];
#if UNITY_EDITOR
        if (DebugRaypointShapes)
        {
            raySamplePoints = new Vector3[yRayCount * xRayCount + 1];
            raySamplePoints[0] = Vector3.zero;
        }
#endif

        UpdateViewCone();
        //SaveAsset();
    }

    private void LateUpdate()
    {
        //  UpdateViewCone();
    }

    private void UpdateViewCone()
    {
        SetOrigin(transform.position);
        SetAimDirection(transform.forward, yFOV, xFOV);
        UpdateMesh();
    }
    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection, float yFovIn, float xFovIn)
    {
        yStartingAngle = transform.rotation.eulerAngles.y + yFovIn / -2f;
        xStartingAngle = xFovIn / 2f;
        yAngleIncrease = yFovIn / (yRayCount - 1);
        xAngleIncrease = xFovIn / (xRayCount - 1);
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
        vertexPoints = new List<Vector4>();
        AddVertexPoint(MeshOrigin, SampleType.None);
    }

    private void InitIterations()
    {
        vertexIndex = 1;
        triangleIndex = 0;
    }

    private void AddVertexPoint(Vector3 sample, SampleType sampleType)
    {
        Debug.Log("Added vertex: " + sample + " with type: " + sampleType);
        lastSampleType = sampleType;

#if UNITY_EDITOR
        vertexPoints.Add(new Vector4(sample.x, sample.y, sample.z, (int)sampleType));
#else
        vertexPoints.Add(sample);
#endif
    }

    private Vector2 GetVertexUV(int y, Vector3 vertex)
    {
        float uvLenght = Vector3.Magnitude(vertex) / SightRange;
        //Create Cone shaped uvs
        float uvAngle = y * yAngleIncrease;
        float rad = uvAngle * Mathf.Deg2Rad;
        Vector2 vertexUV = new Vector2(Mathf.Sin(rad) * uvLenght, Mathf.Cos(rad) * uvLenght);           //Uv map as unit circle starting from (1,0) rotating based on yAngleIncrease, so first is y=0 and then it increases with unit cicle
        return vertexUV;
    }

    private void CreateTriangle()
    {
        if (vertexIndex > 1)
        {
            triangles[triangleIndex + 0] = 0;
            triangles[triangleIndex + 1] = vertexIndex - 1;
            triangles[triangleIndex + 2] = vertexIndex;

            triangleIndex += 3;
        }
    }

    private void CreateMesh()
    {
        mesh.Clear();

        Vector3[] vertices = new Vector3[vertexPoints.Count];
        uv = new Vector2[vertices.Length];
        uv[0] = Vector2.zero;
        triangles = new int[vertices.Length * 3];

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertexPoints[i];
        }

#if UNITY_EDITOR
        if (DebugRaypointShapes)
            StartCoroutine(Balls(raySamplePoints));
        if (DebugVertexShapes)
            StartCoroutine(Boxes(vertexPoints.ToArray()));
#endif

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
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