using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Player Movement
        float xMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float zMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(xMovement, 0, zMovement);

        //Player rotate
        if (CamUtility.IsMouseOverGameWindow())
        {
            float xMouse = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            Vector3 lookAt = new Vector3(0, xMouse, 0);
            transform.Rotate(lookAt);
        }
    }

   
}
