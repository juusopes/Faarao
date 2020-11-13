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
    //public bool cameraRotating;

    //Movement Script
    public float moveAmount;
    public float borderThickness = 10;
    public Vector2 panLimit;

    public Transform cameraController;
    public Transform cameraPos;
    public Transform cameraStabilizer;

    public float zoomSpeed;

    public GameObject objective1, objective2, objective3, objective4, objective5;

    public bool rotating;
    public float rotateSpeed;

    public float rotation;

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

        if (Input.GetMouseButtonDown(2))
        {
            CenterCamera();
        }
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(2))
        {
            rotating = true;

            rotation += rotateSpeed * Input.GetAxis("Mouse X");
            cameraController.transform.eulerAngles = new Vector3(0, rotation, 0);
        }
        else
        {
            rotating = false;
        }
    }

    private void Initialize()
    {
        camHeight = 40;
        camFollow = false;
        moveAmount = 30f;
        zoomSpeed = 40f;
    }

    public void CamPos()
    {

        if (cameraPos.transform.position.y > 25f)
        {
            // scrolling up (zoom in)
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                cameraPos.transform.Translate(transform.forward * zoomSpeed * Time.deltaTime, Space.World);
                cameraPos.transform.Translate(transform.up * -zoomSpeed * Time.deltaTime, Space.World);
            }
        }

        if (cameraPos.transform.position.y < 35f)
        {
            //scrolling down (zoom out)
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                cameraPos.transform.Translate(transform.forward * -zoomSpeed * Time.deltaTime, Space.World);
                cameraPos.transform.Translate(transform.up * zoomSpeed * Time.deltaTime, Space.World);
            }
        }

        if (camFollow)
        {
            if (transform.parent != null)
            {
                transform.position = new Vector3(transform.parent.transform.position.x, camHeight, transform.parent.transform.position.z);

                Vector3 v3 = cameraController.transform.position;
                v3.x = transform.parent.transform.position.x;
                cameraController.transform.position = v3;
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
            if (CamUtility.IsMouseOverGameWindow())        //If unity editor only move the camera if it is insde the screen
#endif
                MoveCamera();


        }
    }

    public void MoveCamera()
    {
        if (rotating)
        {
            return;
        }

        if(cameraController.transform.position.x > 130)
        {
            cameraController.transform.position = new Vector3(130, cameraController.transform.position.y, cameraController.transform.position.z);
            return;
        }

        //Vector3 pos = transform.position;
        if (Input.mousePosition.x >= Screen.width - borderThickness)
        {
            cameraController.transform.Translate(transform.right * moveAmount * Time.deltaTime, Space.World);
            //cameraPos.transform.Translate(transform.right * moveAmount * Time.deltaTime, Space.World);
        }

        else if (Input.mousePosition.x <= borderThickness)
        {
            cameraController.transform.Translate(transform.right * -moveAmount * Time.deltaTime, Space.World);

            //cameraPos.transform.Translate(transform.right * -moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y >= Screen.height - borderThickness)
        {
            cameraController.transform.Translate(transform.forward * moveAmount * Time.deltaTime, Space.World);

            //cameraPos.transform.Translate(transform.forward * moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y <= borderThickness)
        {
            cameraController.transform.Translate(transform.forward * -moveAmount * Time.deltaTime, Space.World);

            //cameraPos.transform.Translate(transform.forward * -moveAmount * Time.deltaTime, Space.World);
        }

        //pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        //pos.z = Mathf.Clamp(pos.z, -panLimit.y, panLimit.y);
        //transform.position = pos;
    }

    public void CenterCamera()
    {
        cameraStabilizer.transform.position = new Vector3(cameraStabilizer.position.x, 0, cameraStabilizer.position.z);
        cameraController.transform.position = new Vector3(cameraStabilizer.position.x, 0, cameraStabilizer.position.z);

        cameraController.transform.eulerAngles = new Vector3(cameraPos.rotation.x, cameraPos.rotation.y, cameraPos.rotation.z);
        cameraPos.transform.position = new Vector3(cameraController.position.x, cameraPos.position.y, cameraController.position.z - 39.76f);
    }

    public void FindObjective1()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective1.transform.position.x, 0, objective1.transform.position.z);
        cameraStabilizer.transform.position = new Vector3(objective1.transform.position.x, 0, objective1.transform.position.z);
    }
    public void FindObjective2()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective2.transform.position.x, 0, objective2.transform.position.z);
        cameraStabilizer.transform.position = new Vector3(objective2.transform.position.x, 0, objective2.transform.position.z);
    }
    public void FindObjective3()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective3.transform.position.x, 0, objective3.transform.position.z);
        cameraStabilizer.transform.position = new Vector3(objective3.transform.position.x, 0, objective3.transform.position.z);
    }
    public void FindObjective4()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective4.transform.position.x, 0, objective4.transform.position.z);
        cameraStabilizer.transform.position = new Vector3(objective4.transform.position.x, 0, objective4.transform.position.z);
    }
    public void FindObjective5()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective5.transform.position.x, 0, objective5.transform.position.z);
        cameraStabilizer.transform.position = new Vector3(objective5.transform.position.x, 0, objective5.transform.position.z);
    }
}
