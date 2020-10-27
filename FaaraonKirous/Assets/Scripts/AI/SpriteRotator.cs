using UnityEngine;
public class SpriteRotator : MonoBehaviour
{
    private Camera cam;
    private void LateUpdate()
    {
        if(cam)
            transform.forward = -cam.transform.forward;
        else
            cam = Camera.main;
    }
}
