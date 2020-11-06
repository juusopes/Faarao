using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
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

    public Transform cameraAnchor;
    public Transform cameraPos;
    public Transform cameraStabilizer;

    public Transform objective1, objective2, objective3, objective4, objective5;

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
        if (Input.GetMouseButtonDown(2))
        {
            cameraAnchor.transform.position = new Vector3(cameraStabilizer.position.x, 0, cameraStabilizer.position.z);
            cameraAnchor.transform.eulerAngles = new Vector3(cameraPos.rotation.x, cameraPos.rotation.y, cameraPos.rotation.z);
            cameraPos.transform.position = new Vector3(cameraAnchor.position.x, cameraPos.position.y, cameraAnchor.position.z - 39.76f);
        }

        if (cameraPos.transform.position.y > 25f)
        {
            // scrolling up (zoom in)
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                cameraPos.transform.Translate(transform.forward * moveAmount * Time.deltaTime, Space.World);
                cameraPos.transform.Translate(transform.up * -moveAmount * Time.deltaTime, Space.World);
            }
        }

        if (cameraPos.transform.position.y < 35f)
        {
            //scrolling down (zoom out)
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                cameraPos.transform.Translate(transform.forward * -moveAmount * Time.deltaTime, Space.World);
                cameraPos.transform.Translate(transform.up * moveAmount * Time.deltaTime, Space.World);
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

            cameraPos.transform.Translate(transform.right * moveAmount * Time.deltaTime, Space.World);
        }

        else if (Input.mousePosition.x <= borderThickness)
        {
            cameraPos.transform.Translate(transform.right * -moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y >= Screen.height - borderThickness)
        {
            cameraPos.transform.Translate(transform.forward * moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y <= borderThickness)
        {
            cameraPos.transform.Translate(transform.forward * -moveAmount * Time.deltaTime, Space.World);
        }

        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);
        transform.position = pos;
    }

    public void FindObjective1()
    {
        cameraAnchor.transform.position = objective1.transform.position;
        cameraStabilizer.transform.position = objective1.transform.position;

        //cameraAnchor.transform.position = new Vector3(cameraStabilizer.position.x, 0, cameraStabilizer.position.z);
        cameraAnchor.transform.eulerAngles = new Vector3(cameraPos.rotation.x, cameraPos.rotation.y, cameraPos.rotation.z);
        cameraPos.transform.position = new Vector3(cameraAnchor.position.x, cameraPos.position.y, cameraAnchor.position.z - 39.76f);
    }
    public void FindObjective2()
    {
        cameraAnchor.transform.position = objective2.transform.position;
        cameraStabilizer.transform.position = objective1.transform.position;
    }
    public void FindObjective3()
    {
        cameraAnchor.transform.position = objective3.transform.position;
        cameraStabilizer.transform.position = objective1.transform.position;
    }
    public void FindObjective4()
    {
        cameraAnchor.transform.position = objective4.transform.position;
        cameraStabilizer.transform.position = objective1.transform.position;
    }
    public void FindObjective5()
    {
        cameraAnchor.transform.position = objective5.transform.position;
        cameraStabilizer.transform.position = objective1.transform.position;
    }
}
