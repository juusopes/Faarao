﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject activeCharacter;
    private float camHeight;
    private Quaternion camRot;
    public bool camFollow;

    //Move Camera
    public float moveAmount = 21f;
    public float borderThickness = 10;
    public Vector2 panLimit;

    public GameObject cameraAnchor;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        CamPos();
    }

    private void Initialize()
    {
        camRot = transform.rotation;
        camHeight = 40;
        camFollow = false;
    }

    private void CamPos()
    {
        cameraAnchor.transform.eulerAngles = new Vector3(60, 0, 0);
        if (camFollow)
        {
            Debug.Log("CamFollow");
            if (transform.parent != null)
            {
                transform.rotation = camRot;
                transform.position = new Vector3(transform.parent.transform.position.x, camHeight, transform.parent.transform.position.z);
            }
            if (transform.parent == null)
            {
                float xAxisValue = Input.GetAxis("Horizontal");
                float zAxisValue = Input.GetAxis("Vertical");
                this.gameObject.transform.Translate(new Vector3(xAxisValue, zAxisValue, 0.0f));
            }
        }
        else
        {
            MoveCam();
            if (transform.parent != null)
            {
                transform.parent = null;
            }
        }
    }
    private void MoveCam()
    {
        Debug.Log("Cam Move");
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - borderThickness)
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


