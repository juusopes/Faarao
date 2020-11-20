using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTest : MonoBehaviour
{
    public Transform cameraPoint, cameraStabilizer;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //    cameraPoint.transform.position = cameraStabilizer.transform.forward * 20/*, transform.rotation*/;

        //    print(cameraPoint.transform.position);

        //cameraPoint.transform.position = cameraStabilizer.TransformPoint(new Vector3(0, -35, 40));

        //copy = Instantiate(newObject, transform.position + transform.forward * 2.0, transform.rotation);
    }
}
