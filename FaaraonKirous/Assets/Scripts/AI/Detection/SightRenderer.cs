using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightRenderer
{
    GameObject parentObject;
    public LineMaterials lm;
    private LineRenderer lineRenderer;
    public GameObject targetObject;
    public bool hasCaughtObject = false;
    private float lineLenght = 0;
    private float maxLenght = 0;
    private const float lineSpeedRangeMultiplier = 2f;
    private const float lineShrinkSpeedMultiplier = 0.5f;
    private float linePercentage;
    private int scalingDirection = 1;      //Going towards 1 or away -1
    private GameObject newGameObject;
    private float playerDiedResetTime = 2f;
    public Vector3 endPoint;
    public LineType lineType;

    public SightRenderer(GameObject parent, LineMaterials lm, float lineWidth, float maxLenght)
    {
        this.lm = lm;
        this.maxLenght = maxLenght;
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
            SetLineMaterial(lt);
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
    public void DrawSightDetection(float sightPercentage, LineType lineType)
    {
        SetLineMaterial(lineType);

        endPoint = OwnPosition + parentObject.transform.forward * maxLenght * sightPercentage;

        DrawLine(OwnPosition, endPoint);
    }

    private void DrawLine(Vector3 start, Vector3 target)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, target);
    }

    private void SetLineMaterial(LineType lc)
    {
        lineType = lc;
        //TODO: Fix material instancing
        lineRenderer.material = lm.GetMaterial(lc);
    }
}
