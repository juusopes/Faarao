using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotation : MonoBehaviour
{
    public float rotateSpeed;
    public Transform anchorPointTransform;

    public float rotation;

    public bool rotating;

    // Start is called before the first frame update
    void Start()
    {
        rotation = 0;
        rotateSpeed = 2f;
        rotating = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            rotating = true;

            rotation += rotateSpeed * Input.GetAxis("Mouse X");
            anchorPointTransform.transform.eulerAngles = new Vector3(0, rotation, 0);
        }

        else
        {
            rotating = false;
        }
    }
}
