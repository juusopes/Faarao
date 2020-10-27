using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateOption
{
    PatrolState,
    AlertState,
    ChaseState,
    TrackingState,
    DistractedState,
    ControlledState,
    DetectionState
}

public class StateMachine
{
    private State currentState;

    private Dictionary<StateOption, State> _states;

    Character character;

    public StateMachine(Character owner)
    {
        character = owner;
        _states = new Dictionary<StateOption, State>()
        {
            {StateOption.PatrolState,       new PatrolState(owner, this)},
            { StateOption.AlertState,       new AlertState(owner, this)},
            { StateOption.ChaseState,       new ChaseState(owner, this)},
            { StateOption.TrackingState,    new TrackingState(owner, this)},
            { StateOption.DistractedState,  new DistractedState(owner, this)},
            { StateOption.ControlledState,  new ControlledState(owner, this)},
            { StateOption.DetectionState,   new DetectionState(owner, this)},
        };

        SetState(StateOption.PatrolState);
    }

    public void UpdateSM()
    {
        currentState.Tick();
    }

    public void PlayerDied()
    {
        currentState.PlayerDied();
    }
    public void PlayerTakesControl()
    {
        currentState.PlayerTakesControl();
    }

    public void SetState(StateOption stateOption)
    {
        if (!_states.ContainsKey(stateOption))
            return;

        State state = _states[stateOption];

        if (state == null || currentState == state)
            return;

        if (currentState != null)
            currentState.OnStateExit();

        currentState = state;
        character.gameObject.name = "Enemy State - " + GetStateName();

        currentState.OnStateEnter();

        character.UpdateIndicator(stateOption);
    }



    public string GetStateName()
    {
        return currentState.GetType().Name;
    }

}
