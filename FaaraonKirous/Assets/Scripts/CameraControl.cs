﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject activeCharacter;
    private float camHeight;
    private Quaternion camRot;
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
    }

    private void CamPos()
    {
        transform.rotation = camRot;
        transform.position = new Vector3(transform.parent.transform.position.x, camHeight, transform.parent.transform.position.z);
    }
}
