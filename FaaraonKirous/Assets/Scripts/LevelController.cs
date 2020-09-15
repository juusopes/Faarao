using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    private GameObject[] characters;
    private GameObject activeCharacter;
    private int current;
    // Start is called before the first frame update
    void Start()
    {
        characters = GameObject.FindGameObjectsWithTag("Player");
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        SwitchCharacter();
    }

    private void Initialize()
    {
        current = 0;
        foreach (GameObject character in characters)
        {
            character.GetComponent<PlayerController>().isActiveCharacter = false;
        }
        characters[current].GetComponent<PlayerController>().isActiveCharacter = true;
        activeCharacter = characters[current];
    }

    private void SwitchCharacter()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            characters[current].GetComponent<PlayerController>().isActiveCharacter = false;
            current++;
            if (current > characters.Length - 1)
            {
                Debug.Log("C");
                current = 0;
            }
            characters[current].GetComponent<PlayerController>().isActiveCharacter = true;
        }
    }
}
