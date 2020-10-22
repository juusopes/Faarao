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

    private void Chase()
    {
        if (CanSeePlayer)
        {
            character.LerpLookAt(character.chaseTarget, 150f);
            character.SetDestination(character.chaseTarget);
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

