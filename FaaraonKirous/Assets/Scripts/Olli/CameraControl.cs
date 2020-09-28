using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject activeCharacter;
    private float camHeight;
    private Quaternion camRot;
    public bool camFollow;
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
        camFollow = true;
    }

    private void CamPos()
    {
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
}
