using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    public bool activated;
    private Color col;

    // Start is called before the first frame update
    void Start()
    {
        col = this.gameObject.GetComponent<MeshRenderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        ActiveEffect();
    }

    private void ActiveEffect()
    {
        if(activated)
        {
            this.gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
        } else
        {
            this.gameObject.GetComponent<MeshRenderer>().material.color = col;
        }
    }
}
