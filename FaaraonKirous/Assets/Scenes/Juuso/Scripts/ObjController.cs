using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjController : MonoBehaviour
{

    public GameObject objective1Done;
    public GameObject objective2Done;
    public GameObject objective3Done;
    public GameObject objective4Done;
    public GameObject objective5Done;

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