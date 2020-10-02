using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActiveArea : MonoBehaviour
{
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
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
        if (other.tag == "TargetableOnject")
        {
            player.GetComponent<PlayerController>().interactList.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        foreach (GameObject gO in player.GetComponent<PlayerController>().interactList)
        {
            if (gO == other.gameObject)
            {
                player.GetComponent<PlayerController>().interactList.Remove(gO);
            }
        }
    }
}
