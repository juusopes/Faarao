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
        float xAngleSampled = xStartingAngle;
        float yAngleSampled = yGlobalAngleIn;
        Vector3 previousSample = Vector3.positiveInfinity;
        Vector3 secondPreviousSample = Vector3.positiveInfinity;
        bool hasResampled = false;
        RaycastHit lastTrueRayCastHit = new RaycastHit();

        for (int x = 0; x < xRayCount; x++)
        {
            if (xAngleSampled < maxXAngle)       //Negatives are up angle
                return;

#if UNITY_EDITOR
            if (x >= maxXIterations)
                return;
#endif

            Vector3 direction = Quaternion.Euler(xAngleSampled, yAngleSampled, 0) * Vector3.forward;
            //Debug.Log(xAngle + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);

            RaycastHit raycastHit;
            RaycastHit previousRayCastHit;

            //if(x > 0)
            previousRayCastHit = x > 0 ? lastColumnSampleRays[x - 1] : new RaycastHit();
            Vector3 sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);


            //if (!hasResampled)
            //    hasResampled = TryReTargetingSamplingAngle(x, y, xAngleSampled, yGlobalAngleIn, raycastHit, ref yAngleSampled, ref sample);

            InspectSample(false, x, xAngleSampled, yAngleSampled, previousSample, raycastHit, previousRayCastHit, lastTrueRayCastHit, sample);


            //Save sample info for next iteration

            //uv[vertexIndex] = GetVertexUV(y, sample);
#if UNITY_EDITOR
            if (DebugRaypointShapes)
                raySamplePoints[vertexIndex] = sample;
#endif
            if (raycastHit.collider != null)
                lastTrueRayCastHit = raycastHit;
            lastColumnSamplePoints[x] = sample;
            lastColumnSampleRays[x] = raycastHit;

            //CreateTriangle();

            secondPreviousSample = previousSample;
            previousSample = sample;
            vertexIndex++;
            xAngleSampled -= xAngleIncrease;


            if (ShouldQuitXIteration(xAngleSampled, raycastHit, previousRayCastHit))
                return;
        }
    }

    private bool ShouldQuitXIteration(float xAngleSampled, RaycastHit raycastHit, RaycastHit previousRayCastHit)
    {
        if (previousRayCastHit.collider && raycastHit.collider == null)
            if (previousRayCastHit.collider.CompareTag("HighestObject"))
                return true;

        //If the first two upsamples have no hit, just give up iteration
        // if (xAngleSampled < -1 && raycastHit.collider == null && previousRayCastHit.collider == null)
        //     return true;

        return false;
    }

    private void InspectSample(bool isResample, int x, float xAngleSampled, float yAngleSampled, Vector3 previousSample, RaycastHit raycastHit, RaycastHit previousRayCastHit, RaycastHit lastTrueRayCastHit, Vector3 sample)
    {
        switch (GetVerticalDirection(xAngleSampled))
        {
            case Looking.Down:
                InspectDownSample(isResample, x, xAngleSampled, yAngleSampled, previousSample, raycastHit, previousRayCastHit, sample);
                break;
            case Looking.ZeroAngle:
                InspectDownSample(isResample, x, xAngleSampled, yAngleSampled, previousSample, raycastHit, previousRayCastHit, sample);
                InspectFlatSample(yAngleSampled, sample, lastTrueRayCastHit);
                break;
            case Looking.Up:
                InspectUpSample(yAngleSampled, previousSample, sample, raycastHit, previousRayCastHit);
                break;
        }
    }


    private void InspectDownSample(bool isResample, int x, float xAngleSampled, float yAngleSampled, Vector3 previousSample, RaycastHit raycastHit, RaycastHit previousRayCastHit, Vector3 sample)
    {
        // if (!AreVerticallyAligned(previousSample, sample))        //Ignore any hits that are above previous
        //{
        //Debug.Log(IsClearlyLower(previousSample, sample));

        //Debug.Log("Last sampled vertex was of type: " + lastSampleType);

        bool reSampleX = false;

        //When dropping to lower level than last hit (only add vertex to first floor hit point)
        if (IsClearlyLower(previousSample, sample) && HitPointIsUpFacing(raycastHit))
        {
            //First try to create ledge before creating floor to keep last vertex correct
            if (x > 0)
            {
                // Debug.Log("<b><color=cyan>Floor to down floor calculation</color></b>");
                //TryCreateFloorToDownFloorVertex(previousRayCastHit, previousSample, sample, xAngleSampled, yAngleSampled);
                reSampleX = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);

            }
            TryCreateFloorVertex(raycastHit, sample);
            //Debug.Log("<b><color=blue>Floor calculation</color></b>");
        }
        //When going forward from wall to floor
        if (IsWallToFloor(previousRayCastHit, raycastHit))
        {
            //Debug.Log("<b><color=white>Wall to floor calculation</color></b>");
            TryCreateWallToFloorCornerVertex(previousSample, sample);
        }
        //When going up from floor to wall
        else if (IsFloorToWall(previousRayCastHit, raycastHit))
        {
            //Debug.Log("<b><color=orange>Floor to wall calculation</color></b>");
            //Todo: consider back tracking
            if (!TryCreateFloorToWallCornerVertex(yAngleSampled, previousSample, sample))
                reSampleX = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //Hit two walls that are clearly not same wall
        else if (IsWallToWall(previousRayCastHit, raycastHit) && IsClearlyLonger(previousSample, sample))
        {
            //Debug.Log("<b><color=lime>Ledge calculation hitting a wall further away</color></b>");
            reSampleX = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //When ray did not hit floor after climbing a wall
        else if (lastSampleType == SampleType.FloorToWallCorner && !AreSimilarHeight(previousSample, sample) && !AreVerticallyAligned(previousSample, sample))
        {
            //Debug.Log("<b><color=green>Ledge calculation climbing up and missing floor</color></b>");
            reSampleX = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //If previous hit was on a floor and new sample is reaching max sight while not hitting anything
        else if (HitPointIsUpFacing(previousRayCastHit) && AreSimilarLenght(sample, SightRange))
        {
            //Debug.Log("<b><color=olive>Ledge calculation end of sight </color></b>");
            reSampleX = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        // }

        if (!isResample && reSampleX)
        {
            Debug.Log("Sample was " + xAngleSampled + " " + yAngleSampled);
            ReSampleXAngle(x, LastAddedVertex, previousRayCastHit);
        }
    }


    private void InspectFlatSample(float yAngleSampled, Vector3 sample, RaycastHit previousRayCastHit)
    {
        if (AreSimilarLenght(sample, SightRange))
        {
            //Debug.Log("<b><color=red>End of sight calculation</color></b>");
            TryCreateVertexToEndOfSightRange(lastSampleType, yAngleSampled, sample, previousRayCastHit);
        }
    }

    private void InspectUpSample(float yAngleSampled, Vector3 previousSample, Vector3 sample, RaycastHit raycastHit, RaycastHit previousRayCastHit)
    {
        if (!AreVerticallyAligned(previousSample, sample))
        {
            if (IsClearlyLonger(previousSample, sample))
            {
                //Debug.Log("<b><color=magenta>Ledge calculation AT UP ANGLES </color></b>");
                TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
            }

        }
    }
    private void ReSampleXAngle(int x, Vector3 ledgeEnd, RaycastHit previousRayCastHit)
    {
        Debug.Log("Resampling");
        Vector3 direction = (ConvertGlobal(ledgeEnd) - origin + Vector3.up * 0.1f).normalized;
        float xAngleSampled = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x;
        float yAngleSampled = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y;
        Debug.Log("Resample " + xAngleSampled + " " + yAngleSampled);
        //Debug.Log(xAngle + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);
        RaycastHit raycastHit;
        Vector3 sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);

        InspectSample(true, x, xAngleSampled, yAngleSampled, ledgeEnd, raycastHit, previousRayCastHit, previousRayCastHit, sample);
    }

    private bool TryReTargetingSamplingAngle(int x, int y, float xAngle, float yAngleIn, RaycastHit raycastHit, ref float yAngleOut, ref Vector3 sampleOut)
    {
        if (y > 0)
        {
            //TODO: WEIRD BLUE RAYCASTS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
                    //ACylinder(closestPoint, Color.magenta);
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