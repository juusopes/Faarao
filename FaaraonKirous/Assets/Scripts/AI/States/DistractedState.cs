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
        switch (distraction.option)
        {
            //COMMENTED : Always get blinding light
            //case DistractionOption.BlindingLight:
            //    character.StartImpairSightRange(distraction.effectTime);
            //    break;
            case DistractionOption.InsectSwarm:
                character.StartImpairFOV(distraction.effectTime);
                break;
        }
    }

    public override void OnStateExit()
    {
        character.ResetDistraction();
    }


    void ActDistracted()
    {
        switch (character.currentDistraction.option)
        {
            case DistractionOption.InsectSwarm:
                character.PanicRunAround();
                break;
            case DistractionOption.NoiseToLookAt :
            case DistractionOption.SightToLookAt:
                character.LerpLookAt(character.currentDistractionPos);
                break;
            case DistractionOption.NoiseToGoto:
            case DistractionOption.SightToGoTo:
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
