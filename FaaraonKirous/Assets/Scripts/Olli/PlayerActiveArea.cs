﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActiveArea : MonoBehaviour
{
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Initialize()
    {
        player = transform.parent.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENter");
        if (other.tag == "TargetableObject")
        {
            player.GetComponent<PlayerController>().interactObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != null && other.gameObject == player.GetComponent<PlayerController>().interactObject)
        {
            player.GetComponent<PlayerController>().interactObject = null;
        }
    }
}
