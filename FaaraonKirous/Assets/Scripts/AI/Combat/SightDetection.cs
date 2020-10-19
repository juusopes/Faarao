using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SightDetection
{
    GameObject parentObject;
    public LineMaterials lm;
    private LineRenderer lineRenderer;
    public GameObject player;
    public bool hasCaughtPlayer = false;
    private float lineSpeed;
    private float lineScalar = 0;
    private float linePercentage;
    private int scalingDirection = 1;      //Going towards 1 or away -1
    public SightDetection(GameObject parent, LineMaterials lm, float lineWidth)
    {
        this.lm = lm;
        parentObject = parent;
        GameObject newGameObject = new GameObject("SightLineRenderer");
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
    private Vector3 PlayerPosition => player.transform.position;
    private float PlayerDistance => Vector3.Distance(OwnPosition, PlayerPosition);
    private Vector3 PlayerDirection => (PlayerPosition - OwnPosition).normalized;
    private float CurrentLineLenght => Vector3.Distance(OwnPosition, lineRenderer.GetPosition(1));

    public void ResetLineRenderer(GameObject player, float lineSpeed)
    {
        this.player = player;
        this.lineSpeed = lineSpeed;
        lineScalar = 0;
        scalingDirection = 1;
        hasCaughtPlayer = false;
    }

    /// <summary>
    /// Simulates sight and draws a sight line. Returns true once the enemy detection line end has reached player. If enemy can see player the line expands, otherwise it shrinks.
    /// </summary>
    /// <param name="CanSeePlayer">If enemy can see player</param>
    /// <returns></returns>
    public bool SimulateSightDetection(bool CanSeePlayer)
    {
        if (player == null)
            return false;

        if(hasCaughtPlayer)
        {
            DrawLine(OwnPosition, PlayerPosition);
        }
        else if (CanSeePlayer || linePercentage > 0)        //Only run if we see player or line is out
        {
            scalingDirection = CanSeePlayer ? 1 : -1;
            //TODO: Fix speed when AI is moving
            lineScalar = Mathf.Min(CurrentLineLenght + scalingDirection * lineSpeed * Time.deltaTime, PlayerDistance);
            Vector3 end = OwnPosition + PlayerDirection * lineScalar;
            Debug.Log(Vector3.Distance(end, OwnPosition), parentObject);

            DrawLine(OwnPosition, end);

            linePercentage = lineScalar / PlayerDistance;

            if (CanSeePlayer && linePercentage >= 0.99f)
                hasCaughtPlayer = true;
        }

        return hasCaughtPlayer;
    }

    private void DrawLine(Vector3 start, Vector3 target)
    {
        UpdateLineColor();
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, target);
    }


    private void UpdateLineColor()
    {
        if (hasCaughtPlayer)
            SetLineColor(LineType.Red);
        else if (scalingDirection == 1)
            SetLineColor(LineType.Yellow);
        else
            SetLineColor(LineType.Green);
    }

    private void SetLineColor(LineType lc)
    {
        lineRenderer.material = lm.GetMaterial(lc);
    }
}
