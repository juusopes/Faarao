using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotation : MonoBehaviour
{
    public float rotateSpeed;
    public Transform anchorPointTransform;

    public float xspeed;

    // Start is called before the first frame update
    void Start()
    {
        xspeed = 0;
        rotateSpeed = 2f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            xspeed += rotateSpeed * Input.GetAxis("Mouse X");

            anchorPointTransform.transform.eulerAngles = new Vector3(0, xspeed, 0);
        }
    }
}
