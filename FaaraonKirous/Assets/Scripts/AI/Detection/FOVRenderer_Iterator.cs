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


            //Save sample info for next iteration

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
            case Looking.ZeroAngle:
                InspectDownSample(x, y, xAngleSample, yAngleSample, previousSample, raycastHit, previousRayCastHit, sample);
                InspectFlatSample(yAngleSample, sample, previousRayCastHit);
                break;
            case Looking.Up:
                InspectUpSample(yAngleSample, previousSample, sample, previousRayCastHit);
                break;
        }
    }


    private void InspectDownSample(int x, int y, float xAngleSample, float yAngleSample, Vector3 previousSample, RaycastHit raycastHit, RaycastHit previousRayCastHit, Vector3 sample)
    {
        if (!AreVerticallyAligned(previousSample, sample))        //Ignore any hits that are above previous
        {
            //Debug.Log(IsClearlyLower(previousSample, sample));

            //Debug.Log("Last sampled vertex was of type: " + lastSampleType);

            //When dropping to lower level 
            if (IsClearlyLower(previousSample, sample) && HitPointIsUpFacing(raycastHit))
            {
                TryCreateFloorVertex(raycastHit, sample);
                Debug.Log("Floor calculation");
                if (x > 0)
                {
                    Debug.Log("Floor to down floor calculation");
                    TryCreateFloorToDownFloorVertex(previousRayCastHit, previousSample, sample, xAngleSample, yAngleSample);
                }
            }
            //When going forward from wall to floor 
            else if (IsWallToFloor(previousRayCastHit, raycastHit))
            {
                Debug.Log("Wall to floor calculation");
                TryCreateWallToFloorCornerVertex(previousSample, sample);
            }
            //When going up from floor to wall
            else if (IsFloorToWall(previousRayCastHit, raycastHit))
            {
                Debug.Log("Floor to wall calculation");
                //Todo: consider back tracking
                TryCreateFloorToWallCornerVertex(yAngleSample, previousSample, sample);
            }
            //When ray did not hit floor after climbing a wall
            else if (IsWallToWall(previousRayCastHit, raycastHit) && IsClearlyLonger(previousSample, sample)
                || lastSampleType == SampleType.FloorToWallCorner
                || lastSampleType == SampleType.WallToFloorCorner)
            {
                Debug.Log("Ledge calculation");
                TryCreateLedgeVertices(lastSampleType, false, yAngleSample, previousSample, sample, previousRayCastHit);
            }
        }
    }


    private void InspectFlatSample(float yAngleSample, Vector3 sample, RaycastHit previousRayCastHit)
    {
        if (AreSimilarLenght(sample, SightRange))
        {
            Debug.Log("End of sight calculation");
            TryCreateVertexToEndOfSightRange(lastSampleType, yAngleSample, sample, previousRayCastHit);
        }
    }

    private void InspectUpSample(float yAngleSample, Vector3 previousSample, Vector3 sample, RaycastHit previousRayCastHit)
    {
        if (!AreVerticallyAligned(previousSample, sample))
        {
            if (IsClearlyLonger(previousSample, sample))
                TryCreateLedgeVertices(lastSampleType, true, yAngleSample, previousSample, sample, previousRayCastHit);
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
                    //Vector3 closestPoint = testRayHit.collider.ClosestPoint(ConvertGlobal(reSampleTest));
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

                //Vector3 relativePos = (ConvertGlobal(compareSample) - ConvertGlobal(reSampleTest));
                //Debug.Log("Relative " + relativePos);
                //float range = relativePos.magnitude + 0.5f;
                //int yRetargetAng = retargeting == 1 ? 1 : -1;
                //Quaternion lookAt = Quaternion.LookRotation(relativePos, Vector3.up);// * Quaternion.Euler(0, yRetargetAng, 0);
                //float testYAngle = transform.rotation.eulerAngles.y + yRetargetAng;
                //Vector3 cornerDirectionCheck = relativePos;// lookAt * Vector3.forward;

                //Vector3 reSample = GetSamplePoint(ConvertGlobal(reSampleTest), relativePos, 1f);

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
}