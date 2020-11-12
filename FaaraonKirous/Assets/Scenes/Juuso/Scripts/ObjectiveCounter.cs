using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectiveCounter: MonoBehaviour
{
    public bool objectiveDone;
    public bool inEndPoint;

    public int objNum;

    public GameObject objectiveDoneMark;
    public GameObject endPoint;

    public int playersInside;

    public RewardController rewards;

    private void Start()
    {
        playersInside = 0;

        //Set All Objectives To False
        objectiveDone = false;
        objectiveDoneMark.SetActive(false);

        rewards = GameObject.FindGameObjectWithTag("LevelController").GetComponent<RewardController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerIn = other.gameObject.tag == "Player";

        if (playerIn && objNum > 0)
        {
            int tempObjectiveNum = other.GetComponent<ObjectiveCounter>().objNum;
            objectiveDone = true;
            objectiveDoneMark.SetActive(true);

            rewards.objectiveCompleted = true;
            rewards.objectivesCompleted[0] = true;

            this.gameObject.SetActive(false);
        }

        if (playerIn && gameObject.name == "EndPoint")
        {
            playersInside++;

            if (playersInside > 2)
            {
                playersInside = 2;
            }

            inEndPoint = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playersInside--;

            if (playersInside < 0)
            {
                playersInside = 0;
            }
            inEndPoint = false;
        }
    }

    //public void addObjectiveDone()
    //{
    //    if(objective1Done)
    //    {
    //        objective1Done = true;
    //    }
    //}
}
