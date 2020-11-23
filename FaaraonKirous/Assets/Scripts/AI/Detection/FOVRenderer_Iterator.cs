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
        //Vector3 previousSample = vertexPoints[0];
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
            //Debug.Log(xAngleSampled + " " + Quaternion.LookRotation(direction, Vector3.up).eulerAngles.x);

            RaycastHit raycastHit;
            RaycastHit previousRayCastHit;

            //if(x > 0)
            previousRayCastHit = x > 0 ? lastColumnSampleRays[x - 1] : new RaycastHit();
            Vector3 sample = GetSamplePoint(origin, direction, SightRange, out raycastHit);


            //if (!hasResampled)
            //    hasResampled = TryReTargetingSamplingAngle(x, y, xAngleSampled, yGlobalAngleIn, raycastHit, ref yAngleSampled, ref sample);

            Vector3 reSampleXCorner = InspectSample(false, x, xAngleSampled, yAngleSampled, previousSample, sample, lastTrueRayCastHit, previousRayCastHit, raycastHit);

#if UNITY_EDITOR
            reSampleXCorner = disableReSampling ? Vector3.zero : reSampleXCorner;
#endif
            if (reSampleXCorner != Vector3.zero)
            {
                Vector3 reDirection = (ConvertGlobal(reSampleXCorner) - origin + Vector3.up * 0.1f).normalized;
                float xAngleReSampled = Quaternion.LookRotation(reDirection, Vector3.up).eulerAngles.x;
                float yAngleReSampled = Quaternion.LookRotation(reDirection, Vector3.up).eulerAngles.y;
                Debug.Log("Resample " + xAngleSampled + " " + yAngleSampled);
                Vector3 reSample = GetSamplePoint(origin, reDirection, SightRange, out raycastHit, Color.red);

                InspectSample(true, x, xAngleReSampled, yAngleReSampled, previousSample, reSample, lastTrueRayCastHit, previousRayCastHit, raycastHit);
            }

            //Save sample info for next iteration

            //uv[vertexIndex] = GetVertexUV(y, sample);
#if UNITY_EDITOR
            if (DebugRaypointShapes)
            {
                raySamplePoints[vertexIndex] = sample;
                raySamplePoints[vertexIndex].w = HasNotHit(raycastHit) ? 0 : 1;
            }
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


            if (ShouldQuitXIteration(xAngleSampled, previousRayCastHit, raycastHit))
                return;
        }
    }

    private bool ShouldQuitXIteration(float xAngleSampled, RaycastHit previousRayCastHit, RaycastHit raycastHit)
    {
        if (previousRayCastHit.collider && raycastHit.collider == null)
            if (previousRayCastHit.collider.CompareTag("HighestObject"))
                return true;

        //If the first two upsamples have no hit, just give up iteration
        // if (xAngleSampled < -1 && raycastHit.collider == null && previousRayCastHit.collider == null)
        //     return true;

        return false;
    }

    private Vector3 InspectSample(bool isResample, int x, float xAngleSampled, float yAngleSampled, Vector3 previousSample, Vector3 sample, RaycastHit lastTrueRayCastHit, RaycastHit previousRayCastHit, RaycastHit raycastHit)
    {
        Vector3 reSampleXCorner = Vector3.zero;
        switch (GetVerticalDirection(xAngleSampled))
        {
            case Looking.Down:
                reSampleXCorner = InspectDownSample(isResample, x, xAngleSampled, yAngleSampled, previousSample, sample, previousRayCastHit, raycastHit);
                break;
            case Looking.ZeroAngle:
                reSampleXCorner = InspectDownSample(isResample, x, xAngleSampled, yAngleSampled, previousSample, sample, previousRayCastHit, raycastHit);
                InspectFlatSample(yAngleSampled, sample, lastTrueRayCastHit, raycastHit);
                break;
            case Looking.Up:
                InspectUpSample(yAngleSampled, previousSample, sample, previousRayCastHit, raycastHit);
                break;
        }
        return reSampleXCorner;
    }


    private Vector3 InspectDownSample(bool isResampleX, int x, float xAngleSampled, float yAngleSampled, Vector3 previousSample, Vector3 sample, RaycastHit previousRayCastHit, RaycastHit raycastHit)
    {
        // if (!AreVerticallyAligned(previousSample, sample))        //Ignore any hits that are above previous
        //{
        //Debug.Log(IsClearlyLower(previousSample, sample));

        //Debug.Log("Last sampled vertex was of type: " + lastSampleType);

        Vector3 reSampleXCorner = Vector3.zero;
#if UNITY_EDITOR     
        bool foundSample = false;
#endif

        //Debug.Log(IsClearlyLower(previousSample, sample).ToString() + HitPointIsUpFacing(raycastHit).ToString() + (x > 0).ToString());
        //Debug.Log(raycastHit.collider + raycastHit.normal.ToString());

        //When dropping to lower level than last hit (only add vertex to first floor hit point)
        if (IsClearlyLower(previousSample, sample) && HitPointIsUpFacing(raycastHit))
        //if (IsClearlyLower(LastAddedVertex, sample) && HitPointIsUpFacing(raycastHit))
        {
            //First try to create ledge before creating floor to keep last vertex correct
            if (x > 0 && lastSampleType != SampleType.LedgeAtDownAngle)
            {

#if UNITY_EDITOR               
                if (debuggingLogging) Debug.Log("<b><color=cyan>Floor to down floor calculation</color></b>");
#endif

                //TryCreateFloorToDownFloorVertex(previousRayCastHit, previousSample, sample, xAngleSampled, yAngleSampled);
                reSampleXCorner = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
            }
            TryCreateFloorVertex(raycastHit, sample);

#if UNITY_EDITOR
            foundSample = true;
            if (debuggingLogging) Debug.Log("<b><color=blue>Floor calculation</color></b>");
#endif
        }

        //if (isResampleX)
        //    return;
        //If first ray hit wall
        else if (x == 0 && HitPointIsSideFacing(raycastHit))
        {
            TryCreateFloorToWallCornerVertex(yAngleSampled, FirstVertex, sample);
        }
        //When going forward from wall to floor
        else if (IsWallToFloor(previousRayCastHit, raycastHit))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=white>Wall to floor calculation</color></b>");
#endif
            TryCreateWallToFloorCornerVertex(previousSample, sample);
            if(!AreHittingSameCollider(previousRayCastHit, raycastHit))
                TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //When going up from floor to wall
        else if (IsFloorToWall(previousRayCastHit, raycastHit))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=orange>Floor to wall calculation</color></b>");
#endif
            //Todo: consider back tracking
            if (!TryCreateFloorToWallCornerVertex(yAngleSampled, previousSample, sample))
            {
                //Debug.Log("Nope");
                reSampleXCorner = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
            }

        }
        //Hit two walls that are clearly not same wall
        else if (IsWallToWall(previousRayCastHit, raycastHit) && IsClearlyLonger(previousSample, sample))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=lime>Ledge calculation hitting a wall further away</color></b>");
#endif
            reSampleXCorner = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //When ray did not hit floor after climbing a wall
        else if (lastSampleType == SampleType.FloorToWallCorner && !AreSimilarHeight(previousSample, sample) && !AreVerticallyAligned(previousSample, sample))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=green>Ledge calculation climbing up and missing floor</color></b>");
#endif
            reSampleXCorner = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
        //If previous hit was on a floor and new sample is reaching max sight while not hitting anything
        else if (!isResampleX && HasHit(previousRayCastHit) && HasNotHit(raycastHit))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=olive>Ledge calculation end of sight </color></b>");
#endif
            reSampleXCorner = TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
        }
#if UNITY_EDITOR
        else if (!foundSample)
        {
            if (drawShapesOnIgnoredSamples) ACylinder(sample);
            if (debuggingLogging) Debug.Log("<b><color=black>Nothing to do with the sample</color></b>");
        }
#endif
        // }

        return reSampleXCorner;
        /*
        if (!isResampleX && reSampleX)
        {
            Debug.Log("Sample was " + xAngleSampled + " " + yAngleSampled);
            ReSampleXAngle(x, LastAddedVertex, previousRayCastHit);
        }
        */
    }


    private void InspectFlatSample(float yAngleSampled, Vector3 sample, RaycastHit lastTrueRayCastHit, RaycastHit rayCastHit)
    {
        if (HasNotHit(rayCastHit))
        {
#if UNITY_EDITOR
            if (debuggingLogging) Debug.Log("<b><color=red>End of sight calculation</color></b>");
#endif
            TryCreateVertexToEndOfSightRange(lastSampleType, yAngleSampled, sample, lastTrueRayCastHit);
        }
    }

    private void InspectUpSample(float yAngleSampled, Vector3 previousSample, Vector3 sample, RaycastHit previousRayCastHit, RaycastHit raycastHit)
    {
        if (!AreVerticallyAligned(previousSample, sample))
        {
            if (IsClearlyLonger(previousSample, sample))
            {
#if UNITY_EDITOR
                if (debuggingLogging) Debug.Log("<b><color=magenta>Ledge calculation AT UP ANGLES </color></b>");
#endif
                TryCreateLedgeVertices(lastSampleType, false, yAngleSampled, previousSample, sample, previousRayCastHit);
            }

        }
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