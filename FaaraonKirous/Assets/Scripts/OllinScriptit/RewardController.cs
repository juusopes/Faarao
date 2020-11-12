using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    public ObjController objectives;
    private LevelController levelController;

    //Rewards Player 1 if true
    public bool[] rewardPlayerOne;
    //Unlocks AbilityNumberTold
    public int[] rewardAbility;

    // Start is called before the first frame update
    void Start()
    {
        levelController = this.gameObject.GetComponent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateObjectives()
    {
        for (int x = 0; x < objectives.objectiveDone.Length - 1; x++) {
            if (objectives.objectiveDone[x] && rewardAbility[x] > 0)
            {
                if (rewardPlayerOne[x])
                {
                    levelController.pharaoh.GetComponent<PlayerController>().abilityAllowed[rewardAbility[x]] = true;
                }
                else if (!rewardPlayerOne[x])
                {
                    levelController.priest.GetComponent<PlayerController>().abilityAllowed[rewardAbility[x]] = true;
                }
            }
        }
    }
}
