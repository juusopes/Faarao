using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectiveCounter: MonoBehaviour
{
    public bool objectiveDone;
    public bool inEndPoint;

    public bool pharaohOnly, priestOnly;

    private GameObject pharaoh, priest;

    public int objNum;

    public GameObject objectiveDoneMark;

    private ObjController objectiveContoller;

    public RewardController rewards;

    public ActivatorScript activated, activated2, activated3, activated4, activated5;

    private void Start()
    {
        //Set All Objectives To False
        objectiveDone = false;
        if (objNum > 0)
        {
            objectiveDoneMark.SetActive(false);
        }

        priest = GameObject.Find("Priest");
        pharaoh = GameObject.Find("Pharaoh");
        rewards = GameObject.FindGameObjectWithTag("LevelController").GetComponent<RewardController>();
        objectiveContoller = transform.parent.gameObject.GetComponent<ObjController>();
    }

    public void Update()
    {
        if (activated.activated == true)
        {
            objectiveDone = true;
            objectiveDoneMark.SetActive(true);

            objectiveContoller.objectiveDone[objNum - 1] = true;

            rewards.UpdateObjectives();

            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && objNum == 0)
        {
            objectiveContoller.playersInside++;
            inEndPoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && objNum == 0)
        {
            objectiveContoller.playersInside--;
            inEndPoint = false;
        }
    }
}
