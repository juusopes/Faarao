using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundIndicatorController : MonoBehaviour
{
    private LevelController lC;
    // Start is called before the first frame update
    void Start()
    {
        lC = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();   
    }

    // Update is called once per frame
    void Update()
    {
        lC.currentCharacter.GetComponent<PlayerController>().groundInd = this.gameObject;
        if (lC.currentCharacter.GetComponent<PlayerController>().abilityActive || lC.currentCharacter.GetComponent<PlayerController>().useAttack || lC.currentCharacter.GetComponent<PlayerController>().useInteract || lC.currentCharacter.GetComponent<PlayerController>().useRespawn)
        {
            this.gameObject.SetActive(true);
        } else
        {
            this.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            this.gameObject.SetActive(false);
        } 
    }
}
