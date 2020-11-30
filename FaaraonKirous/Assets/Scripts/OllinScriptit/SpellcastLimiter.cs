using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellcastLimiter : MonoBehaviour
{
    public GameObject[] limitIcons;
    public Image coolDownFillerImage;
    public int abilityNum;
    public bool targetPlayerOne;
    private GameObject targetCharacter;
    public LevelController levelController;
    [SerializeField]
    private int lastCount;
    [SerializeField]
    private float[] fullFillers;
    // Start is called before the first frame update
    void Start()
    {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        if (targetPlayerOne)
        {
            targetCharacter = levelController.pharaoh;
        }
        else
        {
            targetCharacter = levelController.priest;
        }
        SetIcons();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager._instance.IsFullyLoaded)
        {
            CalculateLimits();
        }
    }

    private void SetIcons()
    {
        int limitIconNumber;
        if (targetPlayerOne)
        {
            limitIconNumber = targetCharacter.GetComponent<PharaohAbilities>().abilityLimitList[abilityNum];
            for (int x = limitIcons.Length - 1; x > limitIconNumber - 1; x--)
            {
                limitIcons[x].SetActive(false);
            }
        }
        else
        {
            limitIconNumber = targetCharacter.GetComponent<PriestAbilities>().abilityLimitList[abilityNum];
            for (int x = limitIcons.Length - 1; x > limitIconNumber - 1; x--)
            {
                limitIcons[x].SetActive(false);
            }
        }
        fullFillers = new float[limitIconNumber];
        for (int x = 0; x < fullFillers.Length; x++)
        {
            fullFillers[x] = 1;
        }
        lastCount = limitIconNumber;
        coolDownFillerImage.fillAmount = 0;
    }

    private void CalculateLimits()
    {
        if (targetCharacter.GetComponent<PlayerController>().abilityLimits[abilityNum] < lastCount)
        {
            lastCount--;
            if (lastCount < fullFillers.Length)
            {
                fullFillers[lastCount] = 0;
            }
        }
        if (targetCharacter.GetComponent<PlayerController>().abilityLimits[abilityNum] > lastCount)
        {
            if (lastCount < fullFillers.Length)
            {
                fullFillers[lastCount] = 1;
            }
            lastCount++;
        }
        for (int x = fullFillers.Length - 1; x >= 0; x--)
        {
            if (limitIcons[x].transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount > fullFillers[x])
            {
                limitIcons[x].transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount -= Time.deltaTime;
            }
            else if (limitIcons[x].transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount < fullFillers[x])
            {
                limitIcons[x].transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount += Time.deltaTime;
            }
        }
        if (targetPlayerOne)
        {
            if (lastCount == 0)
            {
                coolDownFillerImage.fillAmount = 1;
            }
            else
            {
                coolDownFillerImage.fillAmount = targetCharacter.GetComponent<PlayerController>().abilityCooldowns[abilityNum] / targetCharacter.GetComponent<PharaohAbilities>().abilityCDList[abilityNum];
            }
        }
        else
        {
            if (lastCount == 0)
            {
                coolDownFillerImage.fillAmount = 1;
            }
            else
            {
                coolDownFillerImage.fillAmount = targetCharacter.GetComponent<PlayerController>().abilityCooldowns[abilityNum] / targetCharacter.GetComponent<PriestAbilities>().abilityCDList[abilityNum];
            }
        }
    }
}
