using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementScript : MonoBehaviour
{
    public float moveAmount = 20f;
    public float borderThickness = 10;
    public Vector2 panLimit;

    public UnityEngine.GameObject cameraAnchor;

    private void Start()
    {
    }

    private void Update()
    {
        cameraAnchor.transform.eulerAngles = new Vector3(60, 0, 0);

        Vector3 pos = transform.position;

        if(Input.mousePosition.x >= Screen.width - borderThickness)
        {
            pos.x += moveAmount * Time.deltaTime;
        }

        else if (Input.mousePosition.x <= borderThickness)
        {
            pos.x -= moveAmount * Time.deltaTime;
        }

        if (Input.mousePosition.y >= Screen.height - borderThickness)
        {
            pos.z += moveAmount * Time.deltaTime;
        }

        if (Input.mousePosition.y <= borderThickness)
        {
            pos.z -= moveAmount * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);


        transform.position = pos;
    }

}
