using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DistractedState : State
{
    public DistractedState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        ActDistracted();
        Look();
    }

    private Distraction distraction => character.currentDistraction;

    public override void OnStateEnter()
    {
        character.StopNavigation();

        switch (distraction.option)
        {

            case AbilityOption.DistractBlindingLight:
                //COMMENTED : Always get blinding light
                //character.StartImpairSightRange(distraction.effectTime);
                ToAlertState();
                break;
            case AbilityOption.DistractInsectSwarm:
                character.StartImpairFOV(distraction.effectTime);
                break;
        }
    }

    public override void OnStateExit()
    {
        character.ResetDistraction();
    }

    public override void PlayerTakesControl()
    {
        ToControlledState();
    }


    void ActDistracted()
    {
        switch (character.currentDistraction.option)
        {
            case AbilityOption.DistractInsectSwarm:
                character.PanicRunAround();
                break;
            case AbilityOption.DistractNoiseToLookAt:
            case AbilityOption.DistractSightToLookAt:
                character.LerpLookAt(character.currentDistractionPos);
                break;
            case AbilityOption.DistractNoiseToGoto:
            case AbilityOption.DistractSightToGoTo:
                character.SetDestination(character.currentDistractionPos);
                break;
        }
    }

    void Look()
    {
        if (CanSeePlayer)
        {
            ToChaseState();
        }
        else if (!IsDistracted)
        {
            ToAlertState();
        }
    }
}
