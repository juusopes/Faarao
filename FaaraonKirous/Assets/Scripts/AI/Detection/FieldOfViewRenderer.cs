using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;

public class FieldOfViewRenderer : MonoBehaviour
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

    #region Fields and expressions
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
    public List<Vector3> vertexPoints;
    public Vector2[] uv;
    public int[] triangles;
    int vertexIndex = 1;
    int triangleIndex = 0;
    SampleType lastSampleType = 0;

    //Tweakable values
    private const int yRayCount = (0) + 1;    // ODD NUMBER 
    private const int xRayCount = (10) + 1;    // ODD NUMBER 
    private const float maxXAngle = -10.0000001f;
    private const float ledgeStep = 0.2f;
    private const float ledgeSightBlockingHeight = 0.3f;
    private const float enemySightHeight = 2.785f;
    private const float playerHeight = 2.785f;
    private const float yTolerance = 0.2f;                  //Difference between sampled corner and raycasted corner with added tolerance in case floor is not evenly shaped
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
        FloorToDownFloor,
        FloorToUpFloor,
        FloorToWallCorner,
        WallToWall,
        WallToFloorCorner,
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
        material = GetComponent<Renderer>().material;
        origin = Vector3.zero;
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
        AddVertexPoint(MeshOrigin, SampleType.None);
    }

    private void IterateY()
    {
        float yGlobalAngle = yStartingAngle;
        for (int y = 0; y < yRayCount; y++)
        {
            IterateX(y, yGlobalAngle);
            //Debug.Log(y + " global y " + yGlobalAngle + " " + yStartingAngle + " " + yFOV);
            yGlobalAngle += yAngleIncrease;
        }
    }

    private void IterateX(int y, float yGlobalAngleIn)
    {
        float xAngleSample = xStartingAngle;
        float yAngleSample = yGlobalAngleIn;
        Vector3 previousSample = Vector3.positiveInfinity;
        Vector3 secondPreviousSample = Vector3.positiveInfinity;
        bool hasResampled = false;

        for (int x = 0; x < xRayCount; x++)
        {
            if (xAngleSample < maxXAngle)       //Negatives are up angle
                return;

            Vector3 direction = Quaternion.Euler(xAngleSample, yAngleSample, 0) * Vector3.forward;
            //Debug.Log(xAngle + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);

            RaycastHit raycastHit;
            RaycastHit previousRayCastHit;
            //if(x > 0)
            previousRayCastHit = x > 0 ? lastColumnSampleRays[x - 1] : new RaycastHit();
            Vector3 sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);

            if (!hasResampled)
                hasResampled = TryReTargetingSamplingAngle(x, y, xAngleSample, yGlobalAngleIn, raycastHit, ref yAngleSample, ref sample);

            InspectSample(x, y, xAngleSample, yAngleSample, previousSample, raycastHit, previousRayCastHit, sample);

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

    private void InspectSample(int x, int y, float xAngleSample, float yAngleSample, Vector3 previousSample, RaycastHit raycastHit, RaycastHit previousRayCastHit, Vector3 sample)
    {
        switch (GetVerticalDirection(xAngleSample))
        {
            case Looking.Down:
                InspectDownSample(x, y, xAngleSample, yAngleSample, previousSample, raycastHit, previousRayCastHit, sample);
                break;
            case Looking.Flat:
                InspectFlatSample(sample);
                break;
            case Looking.Up:
                InspectUpSample(yAngleSample, previousSample, sample);
                break;
        }
    }


    private void InspectDownSample(int x, int y, float xAngleSample, float yAngleSample, Vector3 previousSample, RaycastHit raycastHit, RaycastHit previousRayCastHit, Vector3 sample)
    {
        if (!FOVUtil.AreVerticallyAligned(previousSample, sample))        //Ignore any hits that are above previous
        {
            //Debug.Log(IsClearlyLower(previousSample, sample));

            if (FOVUtil.IsClearlyLower(previousSample, sample))             //When dropping to lower level 
            {
                TryCreateFloorVertex(raycastHit, sample);
                if (x > 0)
                    TryCreateFloorDropVertex(previousRayCastHit, previousSample, sample, xAngleSample, yAngleSample);
            }
            else if (FOVUtil.IsFloorToWall(previousRayCastHit, raycastHit))
            {
                //TryCreateFloorToWallCornerVertex(yAngleSample, previousSample, sample);
            }
            else if (FOVUtil.IsWallToFloor(previousRayCastHit, raycastHit))
            {
                //TryCreateWallToFloorCornerVertex(previousSample, sample);
            }



            //else if (lastSampleType != SampleType.FloorToWallCorner)
            //   TryCreateDownLedgeVertices(yAngle, previousSample, sample);

        }
    }



    private void InspectFlatSample(Vector3 sample)
    {
        if (FOVUtil.AreSimilarLenght(sample, SightRange))
        {
            TryCreateVertexToEndOfSightRange(sample);
        }
    }

    private void InspectUpSample(float yAngleSample, Vector3 previousSample, Vector3 sample)
    {
        if (!FOVUtil.AreVerticallyAligned(previousSample, sample))
        {
            if (FOVUtil.IsClearlyLonger(previousSample, sample))
                TryCreateUpLedgeVertices(yAngleSample, previousSample, sample);
        }
    }

    private bool TryReTargetingSamplingAngle(int x, int y, float xAngle, float yAngleIn, RaycastHit raycastHit, ref float yAngleOut, ref Vector3 sampleOut)
    {
        if (y > 0)
        {


            //TODO sample whole new sample in "missing spot", then add closest point as bonus to the second one

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
                    //Vector3 closestPoint = testRayHit.collider.ClosestPoint(transform.TransformPoint(reSampleTest));
                    Vector3 closestPoint = GetClosestPointOnCollider(testRayHit, reSampleTest);
                    ACylinder(closestPoint, Color.magenta);
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

    private Vector3 GetClosestPointOnCollider(RaycastHit testRayHit, Vector3 reSampleTest)
    {
        if (testRayHit.collider == null || reSampleTest == Vector3.zero)
        {
            Debug.LogWarning("No can do closest!");
            return Vector3.zero;
        }

        return transform.InverseTransformPoint(testRayHit.collider.ClosestPoint(transform.TransformPoint(reSampleTest)));
    }

    private int ShouldRetargetY(Vector3 preColumnSample, Vector3 sample)
    {
        bool shouldRetarget = FOVUtil.AreSimilarLenght(preColumnSample, SightRange) && !FOVUtil.AreSimilarLenght(sample, SightRange)       //If one of them is full and other not
                            || !FOVUtil.AreSimilarLenght(preColumnSample, SightRange) && FOVUtil.AreSimilarLenght(sample, SightRange);       //If one of them is full and other not
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
            AddVertexPoint(downSample, SampleType.EndOfSightRange);
        }
    }


    private void AddVertexPoint(Vector3 sample, SampleType sampleType)
    {
        lastSampleType = sampleType;
        vertexPoints.Add(sample);
    }

    private void TryCreateFloorVertex(RaycastHit raycastHit, Vector3 sample)
    {
        //if (FOVUtil.HitPointIsUpFacing(raycastHit))          //Hit ground-like flatish position
        //{
        AddVertexPoint(sample, SampleType.Floor);
        //}
    }


    private Vector3 ExtraPolateVector(SampleType type, Vector3 previousSample, Vector3 sample, float xAngle)
    {
        if (sample == Vector3.zero || previousSample == Vector3.zero)
            return Vector3.zero;

        switch (type)
        {
            case SampleType.FloorToDownFloor:
                float heightFromSight = 0 - previousSample.y;
                float xAngleRad = (90f - xAngle) * Mathf.Deg2Rad;
                float hypotenuse = heightFromSight / Mathf.Cos(xAngleRad);
                Debug.Log("Le" + heightFromSight + " " + xAngleRad);
                return sample.normalized * hypotenuse;
        }

        return Vector3.zero;
    }

    private void TryCreateFloorDropVertex(RaycastHit previousRaycastHit, Vector3 previousSample, Vector3 sample, float xAngle, float yAngle)
    {
        Vector3 extraPolatedVec = ExtraPolateVector(SampleType.FloorToDownFloor, previousSample, sample, xAngle);
        AddVertexPoint(extraPolatedVec, SampleType.None);
        Debug.Log(extraPolatedVec.magnitude + " " + previousSample.magnitude);
        //ACylinder(extraPolatedVec, Color.white);
        Vector3 closestPoint = GetClosestPointOnCollider(previousRaycastHit, extraPolatedVec);
        Vector3 extraToClosest = extraPolatedVec - closestPoint;
        //ACylinder(extraToClosest, Color.white);
        bool adjacent = true;
        //float yAngleRad = Vector3.Angle(extraToClosest, extraPolatedVec * -1) * Mathf.Deg2Rad;       //xAngle works directly, because the two lines are parallel when comparing angle
        float yAngleRad = yAngle * Mathf.Deg2Rad;       //xAngle works directly, because the two lines are parallel when comparing angle
        float offSetLenght;
        float otherLenght;
        bool isRightSide = Quaternion.LookRotation(closestPoint, Vector3.up).eulerAngles.y > Quaternion.LookRotation(extraPolatedVec, Vector3.up).eulerAngles.y;

        if (!adjacent)
        {
            float oppCathetus = extraToClosest.magnitude;
            float adjCathetus = oppCathetus / Mathf.Tan(yAngleRad);    //This is the offset lenght that closest point gets from collider (90 angle).
            otherLenght = oppCathetus;
            offSetLenght = adjCathetus;
        }
        else
        {
            float adjCathetus = extraToClosest.magnitude;
            float oppCathetus = Mathf.Tan(yAngleRad) * adjCathetus;    //This is the offset lenght that closest point gets from collider (90 angle).
            otherLenght = adjCathetus;
            offSetLenght = oppCathetus;
        }



        Debug.Log("Adjacent: " + otherLenght + " OffSetLenght : " + offSetLenght + " Angle: "  + " " + yAngleRad * Mathf.Rad2Deg  + " Angle Rad: " + yAngleRad + " Tan Angle: "+ Mathf.Tan(yAngleRad));
        this.name = (Quaternion.LookRotation(closestPoint, Vector3.up).eulerAngles.y - transform.rotation.eulerAngles.y).ToString() + " " + (Quaternion.LookRotation(extraPolatedVec, Vector3.up).eulerAngles.y - transform.rotation.eulerAngles.y).ToString();// + " " + yAngle.ToString();
        this.name = isRightSide.ToString();
        float rotAngle1 = isRightSide ? 90f : -90f;
        float rotAngle2 = isRightSide ? -90f : 90f;
        Vector3 offSet1 = Quaternion.Euler(0, rotAngle1, 0)  * extraToClosest.normalized * offSetLenght;
        Vector3 offSet2 = Quaternion.Euler(0, rotAngle2, 0)  * extraToClosest.normalized * offSetLenght;
        Vector3 absoluteClosestPoint1 = closestPoint - offSet1;
        Vector3 absoluteClosestPoint2 = closestPoint - offSet2;




        ///Vector3 closestPointReAligned = sample.normalized * closestPoint.magnitude;
        //closestPointReAligned = Quaternion.Euler(extraPolatedVec) * closestPointReAligned;
        //Vector3 closestPointReTargeted = GetClosestPointOnCollider(previousRaycastHit, closestPointReAligned);

        AddVertexPoint(closestPoint, SampleType.FloorToDownFloor);
       AddVertexPoint(absoluteClosestPoint1, SampleType.FloorToDownFloor);
       AddVertexPoint(absoluteClosestPoint2, SampleType.FloorToDownFloor);
    }

    private void TryCreateWallToFloorCornerVertex(Vector3 previousSample, Vector3 sample)
    {
        Vector3 wallCorner = new Vector3(previousSample.x, sample.y, previousSample.z);     //Simulate position of corner
        AddVertexPoint(wallCorner, SampleType.WallToFloorCorner);
    }

    private void TryCreateFloorToWallCornerVertex(float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        Vector3 wallCorner = new Vector3(sample.x, previousSample.y, sample.z);     //Simulate position of corner
        Vector3 cornerDirectionCheck = Quaternion.Euler(100, yAngleIn, 0) * Vector3.forward;
        if (CheckColliderExists(wallCorner + Vector3.up * 0.2f, cornerDirectionCheck, 0.5f))
        {
            AddVertexPoint(wallCorner, SampleType.FloorToWallCorner);
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
            AddVertexPoint(corner, SampleType.LedgeAtUpAngle);
            AddVertexPoint(ledgeEnd, SampleType.LedgeAtUpAngle);
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
            AddVertexPoint(corner, SampleType.LedgeAtUpAngle);
            AddVertexPoint(ledgeEnd, SampleType.LedgeAtUpAngle);

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
        {
            Color color = direction.y == 0 ? Color.yellow : Color.green;
            Debug.DrawRay(globalStart, direction * range, color, 1000f);
        }

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
                vertex = transform.InverseTransformPoint(origin + direction * raycastHit.distance);
                uvLenght = raycastHit.distance / SightRange;
            }
            else
            {
                vertex = transform.InverseTransformPoint(origin + direction * SightRange);
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
        primitive.transform.position = transform.TransformPoint(vert);
        primitive.transform.localScale = scale;
        primitive.layer = 2;
        return primitive;
    }


#endif
    #endregion
}