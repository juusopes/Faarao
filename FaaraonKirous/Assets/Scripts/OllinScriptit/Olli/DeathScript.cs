using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DeathScript : MonoBehaviour
{
    public int hp;
    public bool isDead;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        DeathCheck();
    }

    private void Initialize() {
        if (hp == 0)
        {
            hp = 1;
        }
        isDead = false;
    }

    private void DeathCheck()
    {
        if (hp == 0)
        {
            isDead = true;
        }
        if (isDead)
        {
            Debug.Log(this.gameObject + "Is Dead!");
        }
    }
}
