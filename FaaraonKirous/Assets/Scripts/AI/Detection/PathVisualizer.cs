using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

class PathVisualizer : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private TrailRenderer trail;
    private bool isVisualizing;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        Assert.IsNotNull(trail);
    }
    public IEnumerator Visualize(Vector3[] path)
    {
        if (path == null || path.Length == 0)
            yield break;
        if(isVisualizing)
            yield break;

        isVisualizing = true;
        trail.enabled = true;

        for (int i = 1; i < path.Length; i++)
        {
            Vector3 startPos = path[i - 1];
            Vector3 endPos = path[i];
            float duration = (endPos - startPos).magnitude / speed;
            float t = 0.0f;
            float tStep = 1.0f / duration;
            while (t < 1.0f)
            {
                transform.position = Vector3.Lerp(startPos, endPos, t);
                t += tStep * Time.deltaTime;
                yield return null;
            }
        }
        isVisualizing = false;
        trail.enabled = false;
        yield return null;
    }
}
