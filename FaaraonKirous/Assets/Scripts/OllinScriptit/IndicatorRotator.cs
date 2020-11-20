using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorRotator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (transform.childCount > 1)
        {
            transform.GetChild(0).transform.Rotate(0, 0, 25f * Time.deltaTime);
            transform.GetChild(1).transform.Rotate(0, 0, -25f * Time.deltaTime);
            transform.GetChild(2).transform.Rotate(0, 0, 25 * Time.deltaTime);
        }
    }
}
