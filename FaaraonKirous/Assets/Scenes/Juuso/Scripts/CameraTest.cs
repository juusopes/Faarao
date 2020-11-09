using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public GameObject cameraPoint, cameraStabilizer;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cameraPoint.transform.position = cameraStabilizer.transform.position;
    }
}
