using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
class OffMeshLinkMovement
{
    NavMeshAgent navMeshAgent;
    Transform charTransform;
    private bool MoveAcrossNavMeshesStarted = false;
    private float charRadius;
    private float charJumpHeight;
    private float CharHeight => navMeshAgent.baseOffset;



    public OffMeshLinkMovement(Transform trans, NavMeshAgent agent, float radius, float jumpHeight)
    {
        navMeshAgent = agent;
        charTransform = trans;
        charRadius = radius;
        charJumpHeight = jumpHeight;
    }

    public bool CanStartLink()
    {
        return !MoveAcrossNavMeshesStarted;
    }

    public OffMeshLinkRoute GetOffMeshLinkRoute()
    {
        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;
        if (!data.valid)
            return null;

        int area = 0;

        if (data.offMeshLink != null)
            area = data.offMeshLink.area;
        //var areaLink = (OffMeshLink)navMeshAgent.navMeshOwner;
        //if(areaLink != null)
        //area = areaLink.area;

        //Debug.Log("Area: " + area);

        if (area == 3)
            return GetOffMeshLinkRouteJumpGap(data);
        else if (area == 4)
            return GetOffMeshLinkRouteLadder(data);
        return GetOffMeshLinkRouteDefault(data);
    }

    public OffMeshLinkRoute GetOffMeshLinkRouteDefault(OffMeshLinkData data)
    {
        if (!data.valid)
            return null;

        Vector3[] route = new Vector3[] {
                charTransform.position,
                data.endPos + (Vector3.up * navMeshAgent.baseOffset)
                };

        OffMeshLinkRoute linkRoute = new OffMeshLinkRoute();
        linkRoute.route = route;
        linkRoute.RotationType = OffMeshRotationType.RotateMoving;
        Vector3 relativePos = route[1] - route[0];
        relativePos.y = 0;
        linkRoute.faceDirection = Quaternion.LookRotation(relativePos);


        return linkRoute;
    }

    public OffMeshLinkRoute GetOffMeshLinkRouteJumpGap(OffMeshLinkData data)
    {
        if (!data.valid)
            return null;
        Vector3[] route = new Vector3[] {
                charTransform.position,
                ((charTransform.position + data.endPos) / 2) + (Vector3.up * charJumpHeight),
                data.endPos + (Vector3.up * navMeshAgent.baseOffset)
                };

        OffMeshLinkRoute linkRoute = new OffMeshLinkRoute();
        linkRoute.route = route;
        linkRoute.RotationType = OffMeshRotationType.RotateMoving;
        Vector3 relativePos = route[1] - route[0];
        relativePos.y = 0;
        linkRoute.faceDirection = Quaternion.LookRotation(relativePos);

        return linkRoute;
    }

    public OffMeshLinkRoute GetOffMeshLinkRouteLadder(OffMeshLinkData data)
    {
        if (!data.valid)
            return null;

        bool goingUp = data.startPos.y < data.endPos.y;
        Vector3 lowerPos = goingUp ? data.startPos : data.endPos;
        Vector3 higherPos = !goingUp ? data.startPos : data.endPos;

        Vector3[] route;
        Vector3 faceDirection, xzPos, wallPos;

        faceDirection = higherPos - lowerPos;
        faceDirection.y = 0;

        //Thanks unity for changing start/endPos! :(
        RaycastHit hit;
        if (Physics.Raycast(lowerPos + Vector3.up, faceDirection, out hit, 1f, RayCaster.ladderLayerMask))
            wallPos = hit.point;
        else
            wallPos = lowerPos + (faceDirection / 2);

        xzPos = wallPos - (faceDirection.normalized * charRadius);


        Vector3 ladder_bottom = new Vector3(xzPos.x, lowerPos.y + navMeshAgent.baseOffset, xzPos.z);
        Vector3 ladder_top = new Vector3(xzPos.x, higherPos.y + navMeshAgent.baseOffset, xzPos.z);
        //Vector3 ledge_top = higherPos + Vector3.up * CharHeight;
        Vector3 ledge_top = ladder_top + (faceDirection * (1 + charRadius));

        if (goingUp)
            route = new Vector3[] {
                charTransform.position,
                ladder_bottom,
                ladder_top,
                ledge_top
                };
        else
            route = new Vector3[] {
                charTransform.position,
                ledge_top,
                ladder_top,
                ladder_bottom
                };

        OffMeshLinkRoute linkRoute = new OffMeshLinkRoute();
        linkRoute.route = route;
        linkRoute.faceDirection = Quaternion.LookRotation(faceDirection, Vector3.up);
        linkRoute.RotationType = OffMeshRotationType.StopRotate;
        if (data.offMeshLink != null)
            linkRoute.area = data.offMeshLink.area;
        else
            Debug.LogWarning("Could not get offmeshlink area. Using default");

        return linkRoute;
    }

    public IEnumerator MoveAcrossNavMeshLink(OffMeshLinkRoute linkRoute, Vector3 destination)
    {
        yield return MoveAcrossNavMeshLink(linkRoute);
        navMeshAgent.SetDestination(destination);
    }

    public IEnumerator MoveAcrossNavMeshLink(OffMeshLinkRoute linkRoute)
    {
        if (linkRoute.route == null || linkRoute.route.Length == 0)
            yield break;

        MoveAcrossNavMeshesStarted = true;

        //Traverse the off mesh route
        navMeshAgent.updateRotation = linkRoute.RotationType == OffMeshRotationType.None;
        Quaternion startRotation = charTransform.rotation;
        Quaternion newRotation = Quaternion.Euler(linkRoute.faceDirection.eulerAngles);

        for (int i = 1; i < linkRoute.route.Length; i++)
        {
            Vector3 startPos = linkRoute.route[i - 1];
            Vector3 endPos = linkRoute.route[i];
            float duration = (endPos - startPos).magnitude / navMeshAgent.speed;
            float t = 0.0f;
            float tStep = 1.0f / duration;
            while (t < 1.0f)
            {
                charTransform.position = Vector3.Lerp(startPos, endPos, t);
                //Match rotation
                if (i == 1 && linkRoute.RotationType == OffMeshRotationType.RotateMoving)
                {
                    charTransform.rotation = Quaternion.Lerp(startRotation, newRotation, t * 10);
                }
                //navMeshAgent.destination = charTransform.position;
                t += tStep * Time.deltaTime;
                yield return null;
            }

            //Match rotation
            if (i == 1 && linkRoute.RotationType == OffMeshRotationType.StopRotate)
            {
                var timePassed = 0f;
                while (timePassed < duration)
                {
                    var factor = timePassed / duration;
                    charTransform.rotation = Quaternion.Lerp(startRotation, newRotation, factor);
                    timePassed += Time.deltaTime;

                    yield return null;
                }
                charTransform.rotation = linkRoute.faceDirection;
            }
        }
        //TODO: WARP TO CLOSEST POINT IN NAVMESH COMPARING TO LAST POSITION
        //Snap pos after ladder
        if (linkRoute.RotationType == OffMeshRotationType.StopRotate)
            navMeshAgent.Warp(linkRoute.route.Last());
        navMeshAgent.updateRotation = true;
        navMeshAgent.CompleteOffMeshLink();
        MoveAcrossNavMeshesStarted = false;
    }


    IEnumerator DefaultMoveAcrossNavMeshLink()
    {
        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;
        navMeshAgent.updateRotation = false;

        Vector3 startPos = navMeshAgent.transform.position;
        Vector3 endPos = data.endPos + (Vector3.up * navMeshAgent.baseOffset);
        float duration = (endPos - startPos).magnitude / navMeshAgent.speed;
        float t = 0.0f;
        float tStep = 1.0f / duration;
        while (t < 1.0f)
        {
            charTransform.position = Vector3.Lerp(startPos, endPos, t);
            navMeshAgent.destination = charTransform.position;
            t += tStep * Time.deltaTime;
            yield return null;
        }
        charTransform.position = endPos;
        navMeshAgent.updateRotation = true;
        navMeshAgent.CompleteOffMeshLink();
        MoveAcrossNavMeshesStarted = false;
    }

    private IEnumerator RotateOverTime(Quaternion targetRotation, float duration)
    {
        var startRotation = charTransform.rotation;

        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles = new Vector3(0, eulerAngles.y, 0);
        Quaternion newRotation = Quaternion.Euler(eulerAngles);

        var timePassed = 0f;
        while (timePassed < duration)
        {
            var factor = timePassed / duration;
            // optional add ease-in and -out
            //factor = Mathf.SmoothStep(0, 1, factor);

            charTransform.rotation = Quaternion.Lerp(startRotation, newRotation, factor);
            // or
            //transformToRotate.rotation = Quaternion.Slerp(startRotation, targetRotation, factor);

            // increae by the time passed since last frame
            timePassed += Time.deltaTime;

            // important! This tells Unity to interrupt here, render this frame
            // and continue from here in the next frame
            yield return null;
        }

        // to be sure to end with exact values set the target rotation fix when done
        charTransform.rotation = targetRotation;
    }
}

