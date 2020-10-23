using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchButtonSwitch : MonoBehaviour
{
    LevelController lC;
    // Start is called before the first frame update
    void Start()
    {
        lC = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lC.activeCharacter.GetComponent<PlayerController>().isCrouching)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
        } else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
