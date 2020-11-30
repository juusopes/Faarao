using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DetectionState : State
{
    private float searchTimer;

    public DetectionState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        Detect();
    }

    public override void OnStateEnter()
    {
        searchTimer = 0;
        character.StopNavigation();
    }

    public override void PlayerDied()
    {
        character.LostTrackOfPlayer();
        ToTrackingState();
    }

    void Detect()
    {
        character.LerpLookAt(ChaseTarget);

        if (CanSeePlayer)
        {
                searchTimer = 0;
        }
        else if (IsCautious())
        {
            StartTracking();
        }
        else
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= character.DetectionLostReactionDelay)
            {
                StartTracking();
            }
        }
    }

    void StartTracking()
    {
        character.LostTrackOfPlayer();
        //ToAlertState();
        ToTrackingState();
    }
}


