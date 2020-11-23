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
        playersInside = 0;
    }

    public void Update()
    {
        if (playersInside < 0)
        {
            Debug.Log("TOO FEW!");
            playersInside = 0;
        }
        if (playersInside > 2)
        {
            playersInside = 2;
            Debug.Log("TOO MANY!");
        }
        if (playersInside == 2)
        {
            CheckObjectives();
        }
    }

    private void CheckObjectives()
    {
        //Counts the number of completed objectives
        int tempBoolCounter = 0;
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
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex + 1);
        }
    }

    public void LoadNextScene()
    {
        StartCoroutine(WaitForSeconds2());
    }

    public IEnumerator WaitForSeconds2()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex + 1);
        }
    }
}