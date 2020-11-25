using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshForcePosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forcedPos = transform.parent.transform.position;
        forcedPos.y = transform.parent.transform.position.y - 1;
        transform.position = forcedPos;
    }
}
