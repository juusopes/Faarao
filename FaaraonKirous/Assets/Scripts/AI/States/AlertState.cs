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

 
    void Look()
    {
        if(CanSeePlayer)
            ToChaseState();
    }


    void Search()
    {
        character.StopDestination();
        character.SearchRotate();
        searchTimer += Time.deltaTime;
        if (searchTimer >= character.classSettings.searchingDuration)
        {
            stateMachine.SetState(stateMachine.patrolState);
            ToPatrolState();
        }

    }

}

