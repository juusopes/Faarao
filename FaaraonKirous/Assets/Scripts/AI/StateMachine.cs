using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class StateMachine
{
    private State currentState;

    [HideInInspector]
    public PatrolState patrolState;
    [HideInInspector]
    public AlertState alertState;
    [HideInInspector]
    public ChaseState chaseState;
    [HideInInspector]
    public TrackingState trackingState;
    [HideInInspector]
    public DistractedState distractedState;
    Character character;
    public StateMachine(Character owner)
    {
        character = owner;
        patrolState = new PatrolState(owner, this);
        alertState = new AlertState(owner, this);
        chaseState = new ChaseState(owner, this);
        trackingState = new TrackingState(owner, this);
        distractedState = new DistractedState(owner, this);

        SetState(patrolState);
    }

    public void UpdateSM()
    {
        currentState.Tick();
    }

    public void SetState(State state)
    {
        if (state == null || currentState == state)
            return;

        if (currentState != null)
            currentState.OnStateExit();

        currentState = state;
        character.gameObject.name = "Enemy State - " + GetStateName();

        currentState.OnStateEnter();

#if UNITY_EDITOR
        SetIndicator();
#endif
    }

    public void SetIndicator()
    {
        if (currentState == patrolState)
            character.UpdateIndicator(Color.green);
        else if (currentState == chaseState)
            character.UpdateIndicator(Color.red);
        else if (currentState == alertState)
            character.UpdateIndicator(Color.yellow);
        else if (currentState == trackingState)
            character.UpdateIndicator(Color.blue);
        else if (currentState == distractedState)
            character.UpdateIndicator(Color.black);
    }

    public string GetStateName()
    {
        return currentState.GetType().Name;
    }

}
