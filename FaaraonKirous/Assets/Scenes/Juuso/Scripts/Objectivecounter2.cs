using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objectivecounter2 : MonoBehaviour
{
    public bool objective1Done, objective2Done, objective3Done;
    public bool inEndPoint;

    public GameObject objective1DoneMark, objective2DoneMark, objective3DoneMark;
    public GameObject endPoint;

    public int playersInside;

    private void Start()
    {
        playersInside = 0;
        objective1Done = false;
        objective2Done = false;
        objective3Done = false;

        objective1DoneMark.SetActive(false);
        objective2DoneMark.SetActive(false);
        objective3DoneMark.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerIn = other.gameObject.tag == "Player";

        if (playerIn && gameObject.name == "Objective1" && !objective1Done)
        {
            objective1Done = true;
            objective1DoneMark.SetActive(true);

            this.gameObject.SetActive(false);
        }

        if (playerIn && gameObject.name == "Objective2" && !objective2Done)
        {
            objective2Done = true;
            objective2DoneMark.SetActive(true);

            this.gameObject.SetActive(false);
        }

        if (playerIn && gameObject.name == "Objective3" && !objective3Done)
        {
            objective3Done = true;
            objective3DoneMark.SetActive(true);

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
        playersInside--;

        if (playersInside < 0)
        {
            playersInside = 0;
        }

        inEndPoint = false;
    }

    public void addObjectiveDone()
    {
        if(objective1Done)
        {
            objective1Done = true;
        }
    }
}
