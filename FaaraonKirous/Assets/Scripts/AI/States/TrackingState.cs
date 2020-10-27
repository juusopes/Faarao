using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingState : State
{
    public TrackingState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        Track();
        Look();
    }

    public override void PlayerTakesControl()
    {
        ToControlledState();
    }

    public override void PlayerDied()
    {
        ToAlertState();
    }

    void Track()
    {
        character.SetDestination(character.lastSeenPosition);

        if (character.HasReachedDestination())
        {
            ToAlertState();
        }
    }
    void Look()
    {
        DefaultLook();
    }
}
