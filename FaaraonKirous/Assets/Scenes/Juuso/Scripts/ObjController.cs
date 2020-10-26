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

    public int objectivesDone;


    // Start is called before the first frame update
    void Start()
    {
        objectivesDone = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerIn = other.gameObject.tag == "Player";

        if (playerIn && gameObject.name == "Objective1")
        {
            objective1Done.SetActive(true);

            objectivesDone++;
        }

        if (playerIn && gameObject.name == "Objective2")
        {
            objective2Done.SetActive(true);

            objectivesDone++;
        }

        if (playerIn && gameObject.name == "Objective3")
        {
            objective3Done.SetActive(true);

            objectivesDone++;
        }

        if (playerIn && gameObject.name == "Objective4")
        {
            objective4Done.SetActive(true);

            objectivesDone++;
        }

        if (playerIn && gameObject.name == "Objective5")
        {
            objective5Done.SetActive(true);

            objectivesDone++;
        }
    }
}