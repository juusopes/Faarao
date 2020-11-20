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

        //Scene scene = SceneManager.GetActiveScene();

        //if(scene.name == "Level 1")
        //{
        //    AddObjectives(3);
        //}

        //else if (scene.name == "Level 2")
        //{
        //    AddObjectives(4);
        //}
    }

    public void Update()
    {
        //objective1Done = objectivecounter1.objective1Done;
        //objective2Done = objectivecounter2.objective2Done;
        //objective3Done = objectivecounter3.objective3Done;
        //objective4Done = objectivecounter4.objective4Done;
        //objective5Done = objectivecounter5.objective5Done;
        //inEndPoint = endPointer.inEndPoint;
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
    //public int AddObjectives(int objCount)
    //{
    //    //Adds objectives in scene
    //    List<int> objectives = new List<int>();

    //    for (int i = 0; i < objCount; i++)
    //    {
    //        objectives.Add(i);

    //        if (objectives.Contains(0))
    //        {
    //            objective1.SetActive(true);
    //        }
    //        if (objectives.Contains(1))
    //        {
    //            objective2.SetActive(true);
    //        }
    //        if (objectives.Contains(2))
    //        {
    //            objective3.SetActive(true);
    //        }
    //        if (objectives.Contains(3))
    //        {
    //            objective4.SetActive(true);
    //        }
    //        if (objectives.Contains(4))
    //        {
    //            objective5.SetActive(true);
    //        }
    //    }

    //    return objCount;
    //}
}