using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjController : MonoBehaviour
{

    public UnityEngine.GameObject objective1Done;
    public UnityEngine.GameObject objective2Done;
    public UnityEngine.GameObject objective3Done;
    public UnityEngine.GameObject objective4Done;
    public UnityEngine.GameObject objective5Done;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            objective1Done.SetActive(true);
        }
    }
}