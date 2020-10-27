using System.Collections;
using System.Collections.Generic;
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
    private GameObject newGameObject;
    private float playerDiedResetTime = 2f;

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
    private Vector3 PlayerPosition => player.transform.position;
    private float PlayerDistance => Vector3.Distance(OwnPosition, PlayerPosition);
    private Vector3 PlayerDirection => (PlayerPosition - OwnPosition).normalized;
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

    public IEnumerator ResetLineRenderer(GameObject player)
    {
        yield return new WaitForSeconds(playerDiedResetTime);
        this.player = player;
        lineScalar = 0;
        scalingDirection = 1;
        hasCaughtPlayer = false;
        yield return null;
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
            UpdateLineColor();
            DrawLine(OwnPosition, PlayerPosition);
        }
        else if (CanSeePlayer || linePercentage > 0)        //Only run if we see player or line is out
        {
            scalingDirection = CanSeePlayer ? 1 : -1;
            //TODO: Fix speed when AI is moving
            lineScalar = Mathf.Min(CurrentLineLenght + scalingDirection * lineSpeed * Time.deltaTime, PlayerDistance);
            Vector3 end = OwnPosition + PlayerDirection * lineScalar;
            //Debug.Log(Vector3.Distance(end, OwnPosition), parentObject);

            UpdateLineColor();
            DrawLine(OwnPosition, end);

            linePercentage = lineScalar / PlayerDistance;

            if (CanSeePlayer && linePercentage >= 0.99f)
                hasCaughtPlayer = true;
        }

        return hasCaughtPlayer;
    }

    private void DrawLine(Vector3 start, Vector3 target)
    {
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
        //TODO: Fix material instancing
        lineRenderer.material = lm.GetMaterial(lc);
    }
}
