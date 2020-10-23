using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AlertState : State
{
    private float searchTimer;

    public AlertState(Character character, StateMachine stateMachine) : base(character, stateMachine){}

    public override void Tick()
    {
        Search();
        Look();
    }

    public override void OnStateEnter()
    {
        searchTimer = 0;
    }

    void Search()
    {
        character.StopNavigation();
        character.SearchRotate();
        searchTimer += Time.deltaTime;
        if (searchTimer >= character.classSettings.searchingDuration)
        {
            stateMachine.SetState(stateMachine.patrolState);
            ToPatrolState();
        }
    }

    void Look()
    {
        DefaultLook();
    }
}

