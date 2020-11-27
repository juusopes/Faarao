using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellcastLimiter : MonoBehaviour
{
    public GameObject[] limitIcons;
    public int abilityCastNum;
    // Start is called before the first frame update
    void Start()
    {
        SetIcons();
    }

    // Update is called once per frame
    void Update()
    {
        if (abilityCastNum > 0)
        {

        }
    }

    private void SetIcons()
    {
        limitIcons = new GameObject[transform.childCount];
        for(int x = 0; x < transform.childCount; x++)
        {
            limitIcons[x] = transform.GetChild(x).gameObject;
        }
    }

    private void EmptyLimiter(int limiterNum)
    {
        limitIcons[limiterNum].transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount -= Time.deltaTime;
    }

}
