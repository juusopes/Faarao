using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject activeCharacter => GameManager._instance.CurrentCharacter;
    private float camHeight;
    private Quaternion camRot;
    public bool camFollow;

    //Movement Script
    public float moveAmount;
    public float borderThickness;
    public int panLimit;

    public Transform cameraController;
    public Transform cameraPos;
    public Transform cameraStabilizer;

    private float zoomSpeed;

    public GameObject objective1, objective2, objective3, objective4, objective5;
    public GameObject character1, character2;

    public bool rotating;
    public float rotateSpeed;

    public float rotation;

    public int zPanLimit, xPanLimit;

    private float clickTime, timeSinceLastClick;
    private float doubleClick = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            timeSinceLastClick = Time.time - clickTime;

            if(timeSinceLastClick <= doubleClick)
            {
                FindCharacter1();
            }

            clickTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            timeSinceLastClick = Time.time - clickTime;

            if (timeSinceLastClick <= doubleClick)
            {
                FindCharacter2();
            }

            clickTime = Time.time;
        }

        CamPos();

        transform.position = new Vector3(Mathf.Clamp(cameraController.position.x, -xPanLimit, xPanLimit),
            transform.position.y,
            Mathf.Clamp(cameraController.position.z, -zPanLimit, zPanLimit));

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

        if (Input.mousePosition.x >= Screen.width - borderThickness)
        {
            cameraController.transform.Translate(transform.right * moveAmount * Time.deltaTime, Space.World);
        }

        else if (Input.mousePosition.x <= borderThickness)
        {
            cameraController.transform.Translate(transform.right * -moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y >= Screen.height - borderThickness)
        {
            cameraController.transform.Translate(transform.forward * moveAmount * Time.deltaTime, Space.World);
        }

        if (Input.mousePosition.y <= borderThickness)
        {
            cameraController.transform.Translate(transform.forward * -moveAmount * Time.deltaTime, Space.World);
        }

    }

    public void CenterCamera()
    {
        cameraStabilizer.transform.position = new Vector3(cameraStabilizer.position.x, cameraStabilizer.transform.position.y, cameraStabilizer.position.z);
        cameraController.transform.position = new Vector3(cameraStabilizer.position.x, cameraController.transform.position.y, cameraStabilizer.position.z);

        //cameraController.transform.eulerAngles = new Vector3(cameraController.rotation.x, cameraController.rotation.y, cameraController.rotation.z);
        //cameraPos.transform.position = new Vector3(cameraController.position.x, cameraPos.position.y, cameraController.position.z - 39.76f);
    }

    public void FindObjective1()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective1.transform.position.x, cameraController.transform.position.y, objective1.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(objective1.transform.position.x, cameraStabilizer.transform.position.y, objective1.transform.position.z);
    }
    public void FindObjective2()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective2.transform.position.x, cameraController.transform.position.y, objective2.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(objective2.transform.position.x, cameraStabilizer.transform.position.y, objective2.transform.position.z);
    }
    public void FindObjective3()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective3.transform.position.x, cameraController.transform.position.y, objective3.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(objective3.transform.position.x, cameraStabilizer.transform.position.y, objective3.transform.position.z);
    }
    public void FindObjective4()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective4.transform.position.x, cameraController.transform.position.y, objective4.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(objective4.transform.position.x, cameraStabilizer.transform.position.y, objective4.transform.position.z);
    }
    public void FindObjective5()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(objective5.transform.position.x, cameraController.transform.position.y, objective5.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(objective5.transform.position.x, cameraStabilizer.transform.position.y, objective5.transform.position.z);
    }

    public void FindCharacter1()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(character1.transform.position.x, cameraController.transform.position.y, character1.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(character1.transform.position.x, cameraStabilizer.transform.position.y, character1.transform.position.z);
    }

    public void FindCharacter2()
    {
        CenterCamera();

        cameraController.transform.position = new Vector3(character2.transform.position.x, cameraController.transform.position.y, character2.transform.position.z);
        //cameraStabilizer.transform.position = new Vector3(character2.transform.position.x, cameraStabilizer.transform.position.y, character2.transform.position.z);
    }
}
