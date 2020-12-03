using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ObjController : MonoBehaviour
{
    public int objectivesNeeded;

    public GameObject[] objectives;
    public int playersInside;

    public ObjectiveCounter[] objectiveCounters;
    public ObjectiveCounter endPointer;

    public bool[] objectiveDone;
    public bool inEndPoint;

    public int objectivesInLevel;

    private IEnumerator coroutine;
    public GameObject fadeToBlack;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < objectives.Length - 1; x++)
        {
            objectives[x].SetActive(false);
        }
        for (int x = 0; x < objectivesInLevel; x++)
        {
            objectives[x].SetActive(true);
        }


    }

    public void Update()
    {
        if (playersInside < 0)
        {
            playersInside = 0;
        }
        if (playersInside > 2)
        {
            playersInside = 2;
        }
        if (playersInside == 2)
        {
            CheckObjectives();
        }
    }

    public void CheckObjectives()
    {
        //Counts the number of completed objectives
        int tempBoolCounter = 0;

        Scene scene = SceneManager.GetActiveScene();

        foreach (bool done in objectiveDone)
        {
            if (done)
            {
                tempBoolCounter++;
            }
        }
        //Moves to next Build Index if enough objectives are done 
        if (tempBoolCounter >= objectivesNeeded)
        {
            if (scene.name == "CreditScene" && !fadeToBlack.activeSelf)
            {
                StartCoroutine(WaitForSeconds2(60));
                StartCoroutine(WaitForSeconds3(55));
            }
            else
            {
                fadeToBlack.SetActive(true);
                StartCoroutine(WaitForSeconds2(5));
            }
        }
    }

    public IEnumerator WaitForSeconds2(int time)
    {
        Scene scene = SceneManager.GetActiveScene();

        while (true)
        {
            yield return new WaitForSeconds(time);

            if (NetworkManager._instance.IsHost)
            {
                GameManager._instance.LoadNextLevel();
            }
        }
    }

    public IEnumerator WaitForSeconds3(int time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            fadeToBlack.SetActive(true);
        }
    }
}