using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ChaseState : State
{
    public ChaseState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        Chase();
    }

    public override void PlayerTakesControl()
    {
        ToControlledState();
    }

    public override void PlayerDied()
    {
        ToAlertState();
    }


    private void Chase()
    {
        if (CanSeePlayer)
        {
            character.LerpLookAt(ChaseTarget, 150f);
            character.SetDestination(ChaseTarget);
        }
        else
        {
            if (!CanSeePlayer)
            {
                LostTrackOfPlayer();
                ToTrackingState();
            }
        }
    }
}

