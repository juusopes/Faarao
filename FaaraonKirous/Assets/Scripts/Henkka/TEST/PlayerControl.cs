using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    private DistractionOption dsOption;
    public DistractionSpawner ds;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        //Player Movement
        float xMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float zMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(xMovement, 0, zMovement);

        //Player rotate

        float xMouse = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        Vector3 lookAt = new Vector3(0, xMouse, 0);
        transform.Rotate(lookAt);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            dsOption = DistractionOption.BlindingLight;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            dsOption = DistractionOption.InsectSwarm;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            dsOption = DistractionOption.InspectableNoise;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            dsOption = DistractionOption.SomethingToGoTo;
        if (Input.GetKeyDown(KeyCode.Alpha5))
            dsOption = DistractionOption.SomethingToLookAt;

        if (Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                SpawnDistraction(hit.point, dsOption);
        }
    }

    private void SpawnDistraction(Vector3 pos, DistractionOption option)
    {
        if (ds == null)
            return;
        ds.SpawnAtPosition(pos, option);
    }
}
