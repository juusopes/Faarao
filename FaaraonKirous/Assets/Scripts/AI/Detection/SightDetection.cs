using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightDetection
{
    GameObject parentObject;
    public LineMaterials lm;
    private LineRenderer lineRenderer;
    public GameObject targetObject;
    public bool hasCaughtObject = false;
    private float lineSpeed;
    private float lineLenght = 0;
    private const float lineSpeedRangeMultiplier = 2f;
    private const float lineShrinkSpeedMultiplier = 0.5f;
    private float linePercentage;
    private int scalingDirection = 1;      //Going towards 1 or away -1
    private GameObject newGameObject;
    private float playerDiedResetTime = 2f;
    public Vector3 endPoint;
    public LineType lineType;

    public SightDetection(GameObject parent, LineMaterials lm, float lineWidth, float lineSpeed)
    {
        this.lm = lm;
        parentObject = parent;
        newGameObject = new GameObject("SightLineRenderer");
        newGameObject.transform.SetParent(parent.transform);
        newGameObject.layer = 2;
        lineRenderer = newGameObject.AddComponent<LineRenderer>() as LineRenderer;
        lineRenderer.SetPosition(0, OwnPosition);
        lineRenderer.SetPosition(1, OwnPosition);
        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        this.lineSpeed = lineSpeed;
    }

    private Vector3 OwnPosition => parentObject.transform.position;
    private Vector3 TargetPosition => targetObject.transform.position;
    private float TargetDistance => Vector3.Distance(OwnPosition, TargetPosition);
    private Vector3 TargetDirection => (TargetPosition - OwnPosition).normalized;
    private float CurrentLineLenght => Vector3.Distance(OwnPosition, lineRenderer.GetPosition(1));

    public void DestroyLine()
    {
        Object.Destroy(newGameObject);
    }

    public void DisplaySightTester(bool enable, Vector3 position, LineType lt)
    {
        lineRenderer.enabled = enable;
        if (enable)
        {
            SetLineColor(lt);
            DrawLine(OwnPosition, position);
        }
    }

    public IEnumerator DisableSightTesterTimed()
    {
        yield return new WaitForSeconds(1);
        lineRenderer.enabled = false;
    }

    public IEnumerator ResetLineRenderer(GameObject targetObject)
    {
        yield return new WaitForSeconds(playerDiedResetTime);
        this.targetObject = targetObject;
        lineLenght = 0;
        scalingDirection = 1;
        hasCaughtObject = false;
        yield return null;
    }

    /// <summary>
    /// Simulates sight and draws a sight line. Returns true once the enemy detection line end has reached player. If enemy can see player the line expands, otherwise it shrinks.
    /// </summary>
    /// <param name="CanSeeObject">If enemy can see player</param>
    /// <returns></returns>
    public bool SimulateSightDetection(bool CanSeeObject)
    {
        if (targetObject == null)
            return false;

        if(hasCaughtObject)
        {
            UpdateLineColor();
            DrawLine(OwnPosition, TargetPosition);
        }
        else if (CanSeeObject || linePercentage > 0)        //Only run if we see player or line is out
        {
            scalingDirection = CanSeeObject ? 1 : -1;
            float lineSpeedScale = scalingDirection == 1 ? lineSpeed : lineSpeed * lineShrinkSpeedMultiplier;
            lineSpeedScale = lineSpeedScale + (lineSpeedRangeMultiplier * linePercentage);
            //TODO: Fix speed when AI is moving -> solution STOP AI MOVING WOOOOWW
            lineLenght = CurrentLineLenght + (scalingDirection * lineSpeedScale * Time.deltaTime);
            
            endPoint = OwnPosition + (TargetDirection * lineLenght);
            //Debug.Log(Vector3.Distance(end, OwnPosition), parentObject);

            UpdateLineColor();
            DrawLine(OwnPosition, endPoint);

            linePercentage = lineLenght / TargetDistance;

            if (CanSeeObject && linePercentage >= 0.99f)
                hasCaughtObject = true;
        }

        return hasCaughtObject;
    }

    private void DrawLine(Vector3 start, Vector3 target)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, target);
    }


    private void UpdateLineColor()
    {
        if (hasCaughtObject)
            SetLineColor(LineType.Red);
        else if (scalingDirection == 1)
            SetLineColor(LineType.Yellow);
        else
            SetLineColor(LineType.Green);
    }

    private void SetLineColor(LineType lc)
    {
        lineType = lc;
        //TODO: Fix material instancing
        lineRenderer.material = lm.GetMaterial(lc);
    }
}
