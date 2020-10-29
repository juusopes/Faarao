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

    public override void OnStateEnter()
    {
        character.SetTrackingMovementSpeed(true);
    }

    public override void OnStateExit()
    {
        character.SetTrackingMovementSpeed(false);
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
        character.SearchLastSeenPosition();

        if (character.HasReachedDestination())
        {
            ToAlertState();
        }
    }
    void Look()
    {
        if (CanSeePlayer)
        {
            ToDetectionState();
        }
        else if (IsDistracted)
        {
            ToDistractedState();
        }
    }
}
