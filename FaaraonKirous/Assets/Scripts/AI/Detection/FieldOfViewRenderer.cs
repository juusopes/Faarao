using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

public class FieldOfViewRenderer : MonoBehaviour
{
<<<<<<< Updated upstream
=======
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

    #region Fields and expressions
>>>>>>> Stashed changes
    Mesh mesh;
    public Character character;
    private Vector3 origin;
    private float yStartingAngle = 0;
    private float xStartingAngle = 0;
    float yAngleIncrease;
    float xAngleIncrease;
    public Vector3[] raySamplePoints;
    private Vector3[] lastColumnSamplePoints;
    private RaycastHit[] lastColumnSampleRays;
    public List<Vector3> vertexPoints;
    public Vector2[] uv;
    public int[] triangles;
    int vertexIndex = 1;
    int triangleIndex = 0;
    SampleType lastSampleType = 0;

    //Tweakable values
    private const int yRayCount = (10) + 1;    // ODD NUMBER 
    private const int xRayCount = (10) + 1;    // ODD NUMBER 
    private const float verticalThreshold = 0.1f;
    private const float horizontalThreshold = 0.1f;
    private const float stepThreshold = 0.1f;
    private const float ledgeStep = 0.2f;
    private const float ledgeSightBlockingHeight = 0.3f;
    private const float enemySightHeight = 2.785f;
    private const float playerHeight = 2.785f;
    private const float yTolerance = 0.2f;                  //Difference between sampled corner and raycasted corner with added tolerance in case floor is not evenly shaped
    private const float floorSlopeTolerance = 0.8f;         //Dotproduct for hitnormal
    private const float cornerCheckAngle = 80f;             //In what x angle is corner check operated (the rounder ledge edges the smaller value)

    private enum Looking
    {
        Down,
        Flat,
        Up
    }

    private enum SampleType
    {
        None,
        Floor,
        WallToFloorCorner,
        FloorToWallCorner,
        EndOfSightRange,
        LedgeAtUpAngle
    }


    private float yFOV => character.FOV;
    private float xFOV => 90f;
    private float SightRange => character.SightRange;
    private float SightRangeCrouching => character.SightRangeCrouching;

    private Vector3 MeshOrigin => Vector3.zero - Vector3.up * playerHeight;
    #endregion

    #region Start and run
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
        origin = Vector3.zero;
<<<<<<< Updated upstream
        vertices = new Vector3[rayCount + 1 + 1];
        vertices[0] = origin;
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];
=======
        lastColumnSamplePoints = new Vector3[xRayCount];
        lastColumnSampleRays = new RaycastHit[xRayCount];
#if UNITY_EDITOR
        if (DebugRaypointShapes)
        {
            raySamplePoints = new Vector3[yRayCount * xRayCount + 1000];
            raySamplePoints[0] = Vector3.zero;
        }
#endif

        UpdateViewCone();
        //SaveAsset();
>>>>>>> Stashed changes
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

    #region Create viewcone

    void UpdateMesh()
    {
        InitMesh();
        InitIterations();
        IterateY();
        CreateMesh();
    }

    private void InitIterations()
    {
        vertexIndex = 1;
        triangleIndex = 0;
    }

    private void InitMesh()
    {
        vertexPoints = new List<Vector3>();
        vertexPoints.Add(MeshOrigin);
    }

    private void IterateY()
    {
        float yAngle = yStartingAngle;
        for (int y = 0; y < yRayCount; y++)
        {
            IterateX(y, yAngle);
            //Debug.Log(y + " " + yAngle);
            yAngle += yAngleIncrease;
        }
    }

    private void IterateX(int y, float yAngleIn)
    {
        float xAngleSample = xStartingAngle;
        float yAngleSample = yAngleIn;
        Vector3 previousSample = vertexPoints[0];
        Vector3 secondPreviousSample = Vector3.negativeInfinity;
        bool hasResampled = false;

        for (int x = 0; x < xRayCount; x++)
        {
            Vector3 direction = Quaternion.Euler(xAngleSample, yAngleSample, 0) * Vector3.forward;
            //Debug.Log(xAngle + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);

            RaycastHit raycastHit;
            Vector3 sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);

            if(!hasResampled)
                hasResampled = TryReTargetingSamplingAngle(x, y, xAngleSample, yAngleIn, raycastHit, ref yAngleSample, ref sample);

            //Iterate down facing
            if (GetVerticalDirection(xAngleSample) == Looking.Down)      //Is looking DOWN (YES negatives are up)
            {
                if (!AreAboveEachother(previousSample, sample))        //Ignore any hits that are on the same floor as previous
                {
                    //Debug.Log(IsClearlyLower(previousSample, sample));
                    if (IsClearlyLower(previousSample, sample))             //When dropping to lower level 
                    {
                        TryCreateFloorVertex(raycastHit, sample);
                    }
                    else if (IsClearlyHigher(previousSample, sample))       //Floor turns into wall, calculate corner
                    {
                        if (HitPointIsUpFacing(raycastHit))
                            TryCreateWallToFloorCornerVertex(previousSample, sample);
                        else
                            TryCreateFloorToWallCornerVertex(yAngleSample, previousSample, sample);
                        //else if (lastSampleType != SampleType.FloorToWallCorner)
                        //   TryCreateDownLedgeVertices(yAngle, previousSample, sample);

                    }
                }
            }
            //Iterate flat facing
            else if (GetVerticalDirection(xAngleSample) == Looking.Flat)    //Is looking straight eye level
            {
                if (AreSimilarLenght(sample, SightRange))
                {
                    TryCreateVertexToEndOfSightRange(sample);
                }
            }
            //Iterate up facing
            else if (GetVerticalDirection(xAngleSample) == Looking.Up)     //Is looking UP
            {
                if (!AreAboveEachother(previousSample, sample))
                {
                    if (IsClearlyLonger(previousSample, sample))
                        TryCreateUpLedgeVertices(yAngleSample, previousSample, sample);
                }
            }

            //uv[vertexIndex] = GetVertexUV(y, sample);
#if UNITY_EDITOR
            if (DebugRaypointShapes)
                raySamplePoints[vertexIndex] = sample;
#endif
            lastColumnSamplePoints[x] = sample;
            lastColumnSampleRays[x] = raycastHit;
            //CreateTriangle();

            secondPreviousSample = previousSample;
            previousSample = sample;
            vertexIndex++;
            xAngleSample -= xAngleIncrease;

        }
    }

    private bool TryReTargetingSamplingAngle(int x, int y, float xAngle, float yAngleIn, RaycastHit raycastHit, ref float yAngleOut, ref Vector3 sampleOut)
    {
        if (y > 0)
        {


            TODO sample whole new sample in "missing spot", then add closest point as bonus to the second one

            Vector3 preColumnSample = lastColumnSamplePoints[x];
            int retargeting = ShouldRetargetY(preColumnSample, sampleOut);

            if (retargeting != 0)       //Left is missing
            {
                bool shouldMoveColumn = y < yRayCount - 1;
                bool shouldMoveRow = !Mathf.Approximately(xAngle, 0);

                Vector3 reSampleTest = retargeting == 1 ? preColumnSample.normalized * sampleOut.magnitude : sampleOut.normalized * preColumnSample.magnitude;
                Vector3 compareSample = retargeting == 1 ? sampleOut : preColumnSample;
                RaycastHit testRayHit = retargeting == 1 ? raycastHit : lastColumnSampleRays[x];
                if (testRayHit.collider != null)
                {
                    Vector3 closestPoint = testRayHit.collider.ClosestPoint(transform.TransformPoint(reSampleTest));
                    ACylinder(transform.InverseTransformPoint(closestPoint), Color.magenta);
                    Quaternion newY = Quaternion.LookRotation(closestPoint, Vector3.up);
                    CheckColliderExists(raySamplePoints[0], closestPoint, SightRange);
                    float testYAngle = newY.eulerAngles.y;
                    float lastYAngle = yAngleIn - yAngleIncrease;
                    float currentYAngle = yAngleIn;
                    Debug.Log(lastYAngle + " " + testYAngle + " " + currentYAngle);
                    if (shouldMoveColumn && testYAngle > lastYAngle && testYAngle < currentYAngle && retargeting == 1)
                    {
                        yAngleOut = testYAngle;
                        sampleOut = closestPoint;

                        return true;
                    }
                    //Todo: add extra samples for where should not be movement
                }
                /*
                ACylinder(reSampleTest, retargeting == 1 ? Color.white : Color.black);
                ACylinder(compareSample, Color.red);
                */

                //Vector3 relativePos = (transform.TransformPoint(compareSample) - transform.TransformPoint(reSampleTest));
                //Debug.Log("Relative " + relativePos);
                //float range = relativePos.magnitude + 0.5f;
                //int yRetargetAng = retargeting == 1 ? 1 : -1;
                //Quaternion lookAt = Quaternion.LookRotation(relativePos, Vector3.up);// * Quaternion.Euler(0, yRetargetAng, 0);
                //float testYAngle = transform.rotation.eulerAngles.y + yRetargetAng;
                //Vector3 cornerDirectionCheck = relativePos;// lookAt * Vector3.forward;

                //Vector3 reSample = GetSamplePoint(transform.TransformPoint(reSampleTest), relativePos, 1f);

                /*

                if (CheckColliderExists(reSampleTest, cornerDirectionCheck, 0.5f))
                {

                }

                ds
                float yReIterationMin = (y - 1) * yStartingAngle;
                float yReIterationMax = yAngleIn;

                float yReIterationStart = retargeting == 1 ? yReIterationMin : yReIterationMax;
                float yReIterationEnd = retargeting == 1 ? yReIterationMax : yReIterationMin;
                int yReIterationDir = retargeting == 1 ? 1 : -1;
                float yNewAngle;


                int iterations = Mathf.CeilToInt((yReIterationMax - yReIterationMin) / yAngleIncrease / 4);
                for (int i = 1; i <= iterations; i++)
                {
                    float iterationBonus = i * ledgeStep;
                    float startDistance = Mathf.Clamp(corner.magnitude + iterationBonus, yReIterationMin, yReIterationMax);



                    for (int i = yReIterationStart + yReIterationDir; i >= 0 && i < waypointCount; i += direction)
                {

                }


                    if (shouldMoveColumn)
                        yAngle = yNewAngle;

               direction = Quaternion.Euler(xAngle, yNewAngle, 0) * Vector3.forward;
                //Debug.Log(xAngle + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);

                sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);
                */
            }
        }
        return false;
    }

    private int ShouldRetargetY(Vector3 preColumnSample, Vector3 sample)
    {
        bool shouldRetarget = AreSimilarLenght(preColumnSample, SightRange) && !AreSimilarLenght(sample, SightRange)       //If one of them is full and other not
                            || !AreSimilarLenght(preColumnSample, SightRange) && AreSimilarLenght(sample, SightRange);       //If one of them is full and other not
        // || !AreSimilarOnX(preColumnSample, sample) && !AreSimilarOnZ(preColumnSample, sample);

        //TODO: MORE CASES

        bool leftIsMissing = preColumnSample.magnitude > sample.magnitude;
        bool rightIsMissing = preColumnSample.magnitude < sample.magnitude;
        int ret;

        if (shouldRetarget && leftIsMissing)
            ret = 1;
        else if (shouldRetarget && rightIsMissing)
            ret = 2;
        else
            ret = 0;

        return ret;
    }

    private void TryCreateVertexToEndOfSightRange(Vector3 sample)
    {
        Vector3 downSample = GetSamplePoint(transform.TransformPoint(sample), Vector3.down, playerHeight * 2f);
        if (sample.y - downSample.y < playerHeight + yTolerance)
        {
            lastSampleType = SampleType.EndOfSightRange;
            vertexPoints.Add(downSample);
        }
    }

    private void TryCreateFloorVertex(RaycastHit raycastHit, Vector3 sample)
    {
        if (HitPointIsUpFacing(raycastHit))          //Hit ground-like flatish position
        {
            lastSampleType = SampleType.Floor;
            vertexPoints.Add(sample);
        }
    }

    private void TryCreateWallToFloorCornerVertex(Vector3 previousSample, Vector3 sample)
    {
        Vector3 wallCorner = new Vector3(previousSample.x, sample.y, previousSample.z);     //Simulate position of corner
        lastSampleType = SampleType.WallToFloorCorner;
        vertexPoints.Add(wallCorner);
    }

    private void TryCreateFloorToWallCornerVertex(float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        Vector3 wallCorner = new Vector3(sample.x, previousSample.y, sample.z);     //Simulate position of corner
        Vector3 cornerDirectionCheck = Quaternion.Euler(100, yAngleIn, 0) * Vector3.forward;
        if (CheckColliderExists(wallCorner + Vector3.up * 0.2f, cornerDirectionCheck, 0.5f))
        {
            lastSampleType = SampleType.FloorToWallCorner;
            vertexPoints.Add(wallCorner);
        }
    }

    private void TryCreateDownLedgeVertices(float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        Debug.Log("DO ME");

        ACylinder(sample, Color.cyan);
        /*
        Vector3 corner = GetLedgeCorner(previousSample, sample);
        float cornerRotX = Quaternion.LookRotation(corner, Vector3.up).eulerAngles.x;
        float xRadCornerAngle = cornerRotX * Mathf.Deg2Rad;     //Calculate x Angle of character looking at corner in radians

        //{
        float maxSightRangeOverLedge = GetMaxSightRangeOverLedge(xRadCornerAngle, yAngleIn, sample, corner);

        Vector3 ledgeEnd = TryGetEndOfLedge(corner, xRadCornerAngle, maxSightRangeOverLedge);

        if (!ledgeEnd.Equals(Vector3.zero))
        {
            vertexPoints.Add(corner);
            vertexPoints.Add(ledgeEnd);
            lastSampleType = SampleType.LedgeAtUpAngle;
        }
        else
            Debug.Log("nope");
        */
    }

    private void TryCreateUpLedgeVertices(float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        //Debug.Log("try app vert");
        Vector3 corner = GetLedgeCorner(yAngleIn, previousSample, sample);
        float cornerRotX = Quaternion.LookRotation(corner, Vector3.up).eulerAngles.x;
        float xRadCornerAngle = cornerRotX * Mathf.Deg2Rad;     //Calculate x Angle of character looking at corner in radians

        //{
        float maxSightRangeOverLedge = GetMaxSightRangeOverLedge(xRadCornerAngle, yAngleIn, sample, corner);

        Vector3 ledgeEnd = TryGetEndOfLedge(corner, xRadCornerAngle, maxSightRangeOverLedge);

        if (!ledgeEnd.Equals(Vector3.zero))
        {
            vertexPoints.Add(corner);
            vertexPoints.Add(ledgeEnd);
            lastSampleType = SampleType.LedgeAtUpAngle;
        }
        else
            Debug.Log("nope");
    }

    private Vector3 TryGetEndOfLedge(Vector3 corner, float xRadCornerAngle, float maxSightRangeOverLedge)
    {
        Vector3 ledgeEnd = Vector3.zero;
        int emptyRaysInRow = 0;
        //Debug.Log(maxSightRangeOverLedge);
        //Iterate from 0 to max looking down to see if we actually have a ledge
        int iterations = Mathf.CeilToInt((maxSightRangeOverLedge - corner.magnitude) / ledgeStep);
        for (int i = 1; i <= iterations; i++)
        {
            float iterationBonus = i * ledgeStep;
            float startDistance = Mathf.Min(corner.magnitude + iterationBonus, maxSightRangeOverLedge);
            float downRange = Mathf.Abs(Mathf.Sin(xRadCornerAngle) * iterationBonus) + yTolerance;
            if (i < 3)
                downRange += 0.2f;      //Compensate rounded corners
            Vector3 sampleStart = corner.normalized * startDistance;
            //Debug.Log(i + " " + startDistance + " " + downRange + "  " + sampleStart);
            if (CheckColliderExists(sampleStart, Vector3.down, downRange))
            {
                emptyRaysInRow = 0;
                ledgeEnd = new Vector3(sampleStart.x, corner.y, sampleStart.z);
            }
            else
            {
                emptyRaysInRow++;
                if (emptyRaysInRow > 2)
                    break;
            }
        }
        return ledgeEnd;
    }


    /// <summary>
    /// Input local space sample and check if ground exists in downRange
    /// </summary>
    /// <param name="localSampleStart"></param>
    /// <param name="downRange"></param>
    /// <returns></returns>
    private bool CheckColliderExists(Vector3 localSampleStart, Vector3 direction, float downRange)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(transform.TransformPoint(localSampleStart), Vector3.down * downRange, Color.blue, 1000f);
#endif
        return Physics.Raycast(transform.TransformPoint(localSampleStart), direction, downRange, RayCaster.viewConeLayerMask);
    }

    /// <summary>
    /// Get the closest corner on the ledge edge comparing to sample vector.
    /// </summary>
    /// <param name="previousSample"></param>
    /// <param name="sample"></param>
    /// <returns></returns>
    private Vector3 GetLedgeCorner(float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        float approximateY = (sample.normalized * previousSample.magnitude).y;
        Vector3 approximateCorner = new Vector3(previousSample.x, approximateY, previousSample.z);                                   //Corner position that is on same x and z as real corner but y is on sample vector (raycast vector)
        //Vector3 direction = Vector3.down + sample.normalized * 0.2f;                                                //Get direction of ledge
        Vector3 direction = Quaternion.Euler(cornerCheckAngle, yAngleIn, 0) * Vector3.forward;
        float cornerY = GetSamplePoint(transform.TransformPoint(approximateCorner), direction, SightRange).y;       //Raycast almost straight down towards ledge to determine y height
        if (cornerY < previousSample.y)                                                                          //If ray failed, use approximate corner
            return approximateCorner;

        return new Vector3(approximateCorner.x, cornerY, approximateCorner.z);
    }

    /// <summary>
    /// Returns the shortest sigth range looking over the ledge. Compares player height and any obstuctions and initial ray lenght inheriting character Sight range.
    /// </summary>
    /// <param name="xAngle"></param>
    /// <param name="yAngle"></param>
    /// <param name="sample"></param>
    /// <param name="corner"></param>
    /// <returns></returns>
    private float GetMaxSightRangeOverLedge(float xRadAngleIn, float yAngleIn, Vector3 sample, Vector3 corner)
    {
        float maxPlayerSightDistance = GetMaxStandingLedgeRange(xRadAngleIn, corner);
        float maxSightDistance = GetMaxLedgeObstructedSighting(xRadAngleIn, yAngleIn, sample, corner);
        return Mathf.Min(maxPlayerSightDistance, maxSightDistance);
    }


    /// <summary>
    /// Returns max sight range over ledge considering obstructions
    /// </summary>
    /// <param name="xAngle"></param>
    /// <param name="yAngle"></param>
    /// <param name="sample"></param>
    /// <param name="corner"></param>
    /// <returns></returns>
    private float GetMaxLedgeObstructedSighting(float xRadAngleIn, float yAngleIn, Vector3 sample, Vector3 corner)
    {
        //Get max sight hypotenuse taking into account if we hit wall or not    
        float hypotenuse = sample.magnitude - corner.magnitude;                                                                                                                 //Length of the sight after the corner with original y angle                                                                                
        float maxNearCathetusLenght = Mathf.Abs((Mathf.Cos(xRadAngleIn) * hypotenuse));                                                                                           //Lenght of the flat area on ground that is from the triangle representing sight after the corner       (Absolute because of negative angles)

        //Get max distance that can be seen on ledge when raycasted horizontally (in case there is some blockage we cannot get with initial raycast)
        Vector3 yDirection = Quaternion.Euler(0, yAngleIn, 0) * Vector3.forward;                                                                                                //The original sight rotated to go horizontally flat (direction of Near Cathetus)
        float ledgeHorizontalEnd = GetSamplePoint(transform.TransformPoint(corner) + Vector3.up * ledgeSightBlockingHeight, yDirection, maxNearCathetusLenght).magnitude;       //Get sample point looking at xDirection to see if the is something obstructing sight on ledge floor within the distance of Near Cathetus

        float hypotenuseSightLimited = Mathf.Abs(ledgeHorizontalEnd / Mathf.Cos(xRadAngleIn));                                                                                    //Recalculate Sight range over the corner considering obstuctions       (Absolute because of negative angles)
        return hypotenuseSightLimited; // + corner.magnitude; do not add corner, for this lenght is already character to end point because of global to local conversion        //Return max sight range
    }

    /// <summary>
    /// Returns max sight range that character could see a standing player over a ledge
    /// </summary>
    /// <param name="yAngle"></param>
    /// <param name="corner"></param>
    /// <returns></returns>
    private float GetMaxStandingLedgeRange(float xRadAngleIn, Vector3 corner)
    {
        //Get distance player can be seen standing
        float hypotenusePlayerStanding = Mathf.Abs(playerHeight / Mathf.Sin(xRadAngleIn));                                                                           //Trignometric triangle calculation where playerHeight is Far Cathetus and hypothenus represents player sight range after corner      (Absolute because of negative angles)
        return hypotenusePlayerStanding + corner.magnitude;       //How far in theory could you see standing player if not hitting wall
    }

    /// <summary>
    /// RAYCASTS IN GLOBAL SPACE !! MESH CALCULATIONS ARE IN LOCAL SCAPE!! Returns local space position. Position is raycast end point at range or hit point. 
    /// </summary>
    /// <param name="globalStart"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range)
    {
        RaycastHit rayCastHit = new RaycastHit();
        return GetSamplePoint(globalStart, direction, range, out rayCastHit);
    }

    /// <summary>
    /// RAYCASTS IN GLOBAL SPACE !! MESH CALCULATIONS ARE IN LOCAL SCAPE!! Returns local space position. Position is raycast end point at range or hit point.
    /// </summary>
    /// <param name="globalStart"></param>
    /// <param name="direction"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    private Vector3 GetSamplePoint(Vector3 globalStart, Vector3 direction, float range, out RaycastHit rayCastHitReturn)
    {
#if UNITY_EDITOR
        if (DebugRayCasts)
            Debug.DrawRay(globalStart, direction * range, Color.green, 1000f);
#endif

        RaycastHit raycastHit;

        if (Physics.Raycast(globalStart, direction, out raycastHit, range, RayCaster.viewConeLayerMask))
        {
            rayCastHitReturn = raycastHit;
            return transform.InverseTransformPoint(globalStart + direction * raycastHit.distance);
        }
        else
        {
            rayCastHitReturn = new RaycastHit();
            return transform.InverseTransformPoint(globalStart + direction * range);
        }
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
            StartCoroutine(Boxes(vertices));
#endif

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }


    #endregion

    #region Comparisons
    private Looking GetVerticalDirection(float xAngle)
    {
        if (xAngle == 0)
            return Looking.Flat;
        if (xAngle < 0)
            return Looking.Up;
        return Looking.Down;
    }

    private bool HitPointIsUpFacing(RaycastHit raycastHit)
    {
        return Vector3.Dot(raycastHit.normal, Vector3.up) > floorSlopeTolerance;
    }

    private bool IsClearlyLonger(Vector3 start, Vector3 end)
    {
        return end.magnitude - start.magnitude > stepThreshold;
    }

    private bool IsClearlyHigher(Vector3 start, Vector3 end)
    {
        return end.y - start.y > verticalThreshold;
    }

    private bool IsClearlyLower(Vector3 start, Vector3 end)
    {
        //Debug.Log((end.y - start.y < -verticalThreshold) +" " +start.y + "-" + end.y + "=" + (end.y - start.y));
        return end.y - start.y < -verticalThreshold;
    }

    private bool AreAboveEachother(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold
        && Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }
    private bool AreSimilarOnX(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.x - sample2.x) < horizontalThreshold;
    }
    private bool AreSimilarOnZ(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.z - sample2.z) < horizontalThreshold;
    }

    private bool AreSimilarHeight(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.y - sample2.y) < verticalThreshold;
    }

    private bool AreSimilarLenght(Vector3 sample1, Vector3 sample2)
    {
        return Mathf.Abs(sample1.magnitude - sample2.magnitude) < horizontalThreshold;
    }

    private bool AreSimilarLenght(Vector3 sample1, float comparison)
    {
        return Mathf.Abs(sample1.magnitude - comparison) < horizontalThreshold;
    }

    #endregion

    #region Shader values

    public void UpdateMaterialProperties(LineType background, LineType fill, float percentage)
    {
        //material.SetFloat(THICKNESS, outerThickness);
    }

    #endregion

    #region 2D version

    /*
    void UpdateMesh2D()
    {
        yAngle = yStartingAngle;
        vertices[0] = Vector3.zero;
<<<<<<< Updated upstream
=======
        uv[0] = Vector2.zero;



>>>>>>> Stashed changes
        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= yRayCount; i++)
        {
            Vector3 vertex;
            Vector3 direction = Quaternion.Euler(0, yAngle, 0) * Vector2.right;
            //Debug.DrawRay(origin, direction * viewDistance, Color.green, 10f);
            RaycastHit raycastHit;
            if (Physics.Raycast(origin, direction, out raycastHit, SightRange, RayCaster.viewConeLayerMask))
            {
                vertex = transform.InverseTransformPoint(origin + direction * raycastHit.distance);
            }
            else
            {
                vertex = transform.InverseTransformPoint(origin + direction * SightRange);
            }
<<<<<<< Updated upstream
                
=======


            //Create Cone shaped uvs
            float uvAngle = i * yAngleIncrease;
            float rad = uvAngle * Mathf.Deg2Rad;
            uv[vertexIndex] = new Vector2(Mathf.Sin(rad) * uvLenght, Mathf.Cos(rad) * uvLenght);

>>>>>>> Stashed changes

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = vertexIndex - 1;
                triangles[triangleIndex + 1] = 0;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            yAngle -= yAngleIncrease;
        }

<<<<<<< Updated upstream
        /*foreach (Vector3 vert in vertices)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vert + transform.position;
        }*/
=======
        //foreach (Vector3 vert in vertices)
        //{
        //   GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //   sphere.transform.position = vert + transform.position;
        //}
>>>>>>> Stashed changes

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
    */

    #endregion

    #region Debugging
#if UNITY_EDITOR
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
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = transform.TransformPoint(vert);
            sphere.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            sphere.layer = 2;
            sphere.GetComponent<Renderer>().material = ballMat;
            yield return new WaitForSeconds(1f / arr.Length);
        }
    }
<<<<<<< Updated upstream
=======

    IEnumerator Boxes(Vector3[] arr)
    {
        yield return new WaitForSeconds(0f);
        foreach (Vector3 vert in arr)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sphere.transform.position = transform.TransformPoint(vert);
            sphere.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            sphere.layer = 2;
            sphere.GetComponent<Renderer>().material.color = Color.red;
            yield return new WaitForSeconds(1f / arr.Length);
        }
    }

    void ACylinder(Vector3 localPos, Color color)
    {
        if (DebugRaypointShapes)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sphere.transform.position = transform.TransformPoint(localPos);
            sphere.transform.localScale = new Vector3(0.5f, 0.25f, 0.5f);
            sphere.layer = 2;
            sphere.GetComponent<Renderer>().material.color = color;
        }
    }

    void SaveAsset()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + "viewConeMesh.asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }
#endif
    #endregion
>>>>>>> Stashed changes
}



