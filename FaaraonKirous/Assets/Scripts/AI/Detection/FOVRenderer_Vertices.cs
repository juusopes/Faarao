using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.VirtualTexturing;


public partial class FOVRenderer
{
    #region Vertex Calculation cases ====================================================================================================

    private void TryCreateFloorVertex(RaycastHit raycastHit, Vector3 sample)
    {
        //if (FOVUtil.HitPointIsUpFacing(raycastHit))          //Hit ground-like flatish position
        //{
        AddVertexPoint(sample, SampleType.Floor);
        //}
    }

    private void TryCreateFloorToDownFloorVertex(RaycastHit previousRaycastHit, Vector3 previousSample, Vector3 sample, float xAngle, float yAngle)
    {
        Vector3 extraPolatedVec = ExtraPolateVector(SampleType.FloorToDownFloor, previousSample, sample, xAngle);
        Vector3 cornerDirection = ((ConvertGlobal(previousSample) + Vector3.up * -0.2f) - ConvertGlobal(extraPolatedVec)).normalized;
        Vector3 cornerSample = ConvertLocal(GetSpecificColliderHitPoint(extraPolatedVec, cornerDirection, SightRange, previousRaycastHit.collider));
        if (cornerSample != Vector3.zero)
        {
            Vector3 corner = new Vector3(cornerSample.x, previousSample.y, cornerSample.z);
            AddVertexPoint(corner, SampleType.FloorToDownFloor);
        }


        /*
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
        */
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

    private void TryCreateWallToFloorCornerVertex(Vector3 previousSample, Vector3 sample)
    {
        Vector3 wallCorner = new Vector3(previousSample.x, sample.y, previousSample.z);     //Simulate position of corner
        AddVertexPoint(wallCorner, SampleType.WallToFloorCorner);
    }

    private void TryCreateVertexToEndOfSightRange(Vector3 sample)
    {
        Vector3 downSample = GetSamplePoint(ConvertGlobal(sample), Vector3.down, playerHeight * 2f);
        if (sample.y - downSample.y < playerHeight + yTolerance)
        {
            AddVertexPoint(downSample, SampleType.EndOfSightRange);
        }
    }

    private void TryCreateLedgeVertices(bool isAboveZeroAngle, float yAngleIn, Vector3 previousSample, Vector3 sample)
    {
        //Debug.Log("try app vert");
        Vector3 corner = GetLedgeCorner(yAngleIn, previousSample, sample);
        float cornerRotX = Quaternion.LookRotation(corner, Vector3.up).eulerAngles.x;
        float xRadCornerAngle = cornerRotX * Mathf.Deg2Rad;     //Calculate x Angle of character looking at corner in radians
        float maxSightRangeOverLedge;

        if (isAboveZeroAngle)
        {
            maxSightRangeOverLedge = GetMaxSightRangeOverLedge(xRadCornerAngle, yAngleIn, sample, corner);
        }
        else
        {
            maxSightRangeOverLedge = sample.magnitude;
        }

        Vector3 ledgeEnd = TryGetEndOfLedge(corner, xRadCornerAngle, maxSightRangeOverLedge);

        if (!ledgeEnd.Equals(Vector3.zero))
        {
            AddVertexPoint(corner, SampleType.LedgeAtUpAngle);
            AddVertexPoint(ledgeEnd, SampleType.LedgeAtUpAngle);

        }
        else
            Debug.Log("nope");
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
        float cornerY = GetSamplePoint(ConvertGlobal(approximateCorner), direction, SightRange).y;       //Raycast almost straight down towards ledge to determine y height
        if (cornerY < previousSample.y)                                                                          //If ray failed, use approximate corner
            return approximateCorner;

        return new Vector3(approximateCorner.x, cornerY, approximateCorner.z);
    }

    private Vector3 TryGetEndOfLedge(Vector3 corner, float xRadCornerAngle, float maxSightRangeOverLedge)
    {
        Vector3 ledgeEnd = Vector3.zero;
        int emptyRaysInRow = 0;
        Debug.Log("Try to get ledge end with range : " + maxSightRangeOverLedge);
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

    #endregion

    #region Vertex Calculation helpers =======================================================================================================

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
        float ledgeHorizontalEnd = GetSamplePoint(ConvertGlobal(corner) + Vector3.up * ledgeSightBlockingHeight, yDirection, maxNearCathetusLenght).magnitude;       //Get sample point looking at xDirection to see if the is something obstructing sight on ledge floor within the distance of Near Cathetus

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

    #endregion
}