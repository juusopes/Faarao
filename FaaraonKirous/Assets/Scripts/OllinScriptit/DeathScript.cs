using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DeathScript : MonoBehaviour
{
    //
    [SerializeField]
    private float hp;
    public float damage;
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
        if (damage > 0)
        {
            hp -= damage;
            if (hp < 0)
            {
                hp = 0;
            }
            damage = 0;
        }
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
