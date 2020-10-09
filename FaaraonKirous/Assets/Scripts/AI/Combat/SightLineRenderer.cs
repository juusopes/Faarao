using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SightLineRenderer : MonoBehaviour
{
    public LineMaterials lm;
    LineRenderer lineRenderer;
    Material material;
    GameObject[] players = new GameObject[2];
    float speed = 2f;
    [Range(0, 1)]
    float linePercentage = 0;
    float step = 0.01f;
    float lineSpeed = 15f;
    int direction = 1;
    public float timeRemaining = 10;


    private Vector3 OwnPosition => transform.position;
    private Vector3 Player1Position => players[0] == null ? Vector3.zero : players[0].transform.position;
    private Vector3 Player2Position => players[1] == null ? Vector3.zero : players[1].transform.position;

    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        lineRenderer = GetComponent<LineRenderer>();
        //material = lineRenderer.material;
        //material.mainTextureScale = new Vector2(0.5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        //lineRenderer.SetPosition(0, OwnPosition);

        //float step = speed * Time.deltaTime;
        //Vector3 endPoint = Vector3.MoveTowards(OwnPosition, Player1Position, step);
        //lineRenderer.SetPosition(1, OwnPosition + endPoint);
        if (linePercentage <= 0)
            direction = 1;
        else if (linePercentage >= 1)
            direction = -1;


        linePercentage += direction * step * lineSpeed * Time.deltaTime;

        if (linePercentage > 0.9f)
            lineRenderer.material = lm.RedLine;
        else if (direction == 1)
            lineRenderer.material = lm.YellowLine;
        else
            lineRenderer.material = lm.GreenLine;

        DrawLineByPercentage(OwnPosition, Player1Position, linePercentage);    
}

    /* IEnumerator LerpUp()
     {
         float progress = 0;

         while (progress <= 1)
         {
             transform.localScale = Vector3.Lerp(InitialScale, FinalScale, progress);
             progress += Time.deltaTime * TimeScale;
             yield return null;
         }
         transform.localScale = FinalScale;


     }*/

    /// <summary>
    /// Draw line from enemy towards target with lenght of percentage. 1 = full line from enemy to target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="percentage"></param>
    private void DrawLineByPercentage(Vector3 start, Vector3 target, float percentage)
    {
        Vector3 lineVector = target - start;
        Vector3 endPoint = lineVector * percentage;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start + endPoint);
        float distance = Vector3.Distance(start, target);
        lineRenderer.material.mainTextureScale = new Vector2(distance * 2f, 1f);
    }


}
