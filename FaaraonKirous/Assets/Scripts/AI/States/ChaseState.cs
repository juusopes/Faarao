using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ChaseState : State
{
    public ChaseState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        Chase();
        Look();
    }

    private void Chase()
    {
        character.SetDestination(character.chaseTarget);
    }

    void Look()
    {
        if (CanSeePlayer)
        {
            character.LostTrackOfPlayer();
            ToTrackingState();
        }
    }
}

