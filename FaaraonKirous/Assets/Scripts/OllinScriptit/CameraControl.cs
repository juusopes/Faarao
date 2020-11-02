using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject activeCharacter;
    private float camHeight;
    private Quaternion camRot;
    public bool camFollow;

    //Movement Script
    public float moveAmount;
    public float borderThickness = 10;
    public Vector2 panLimit;
    public float camScrollSpeed;

    public GameObject cameraAnchor;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        CamPos();
    }

    private void Initialize()
    {
        camHeight = 40;
        camFollow = false;
        moveAmount = 40f;
    }

    private void CamPos()
    {
        if (cameraAnchor.transform.position.z < -30f)
        {
            // scrolling up (zoom in)
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y - camScrollSpeed, transform.position.z + camScrollSpeed);
            }
        }

        if (cameraAnchor.transform.position.z > -45f)
        {
            //scrolling down (zoom out)
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                GetComponent<Transform>().position = new Vector3(transform.position.x, transform.position.y + camScrollSpeed, transform.position.z - camScrollSpeed);
            }
        }


        if (camFollow)
        {
            if (transform.parent != null)
            {
                transform.position = new Vector3(transform.parent.transform.position.x, camHeight, transform.parent.transform.position.z);

                Vector3 v3 = cameraAnchor.transform.position;
                v3.x = transform.parent.transform.position.x;
                cameraAnchor.transform.position = v3;

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
            if (transform.parent != null)
            {
                transform.parent = null;
            }
#if UNITY_EDITOR
            if(CamUtility.IsMouseOverGameWindow())       //If unity editor only move the camera if it is insde the screen
#endif
                MoveCamera();
        }
    }

    private void MoveCamera()
    {
        Vector3 pos = transform.position;
        if (Input.mousePosition.x >= Screen.width - borderThickness)
        {
            cameraAnchor.transform.Translate(transform.right * 50 * Time.deltaTime, Space.World);
        }

        else if (Input.mousePosition.x <= borderThickness)
        {
            cameraAnchor.transform.Translate(transform.right * -50 * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y >= Screen.height - borderThickness)
        {
            cameraAnchor.transform.Translate(transform.forward * 50 * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y <= borderThickness)
        {
            cameraAnchor.transform.Translate(transform.forward * -50 * Time.deltaTime, Space.World);
        }

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);
        transform.position = pos;
    }
}
