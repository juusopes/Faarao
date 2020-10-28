using UnityEngine;

public abstract class State
{
    protected Character character;
    protected StateMachine stateMachine;

    public abstract void Tick();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
    public virtual void PlayerDied() { }
    public virtual void PlayerTakesControl() { }

    protected bool CanSeePlayer => character.CanDetectAnyPlayer;
    protected bool IsDistracted => character.isDistracted;
    protected Vector3 ChaseTarget => character.chaseTarget;

    public State(Character character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
    }

    protected void DefaultLook()
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

    protected void LostTrackOfPlayer()
    {
        character.lastSeenPosition = ChaseTarget;
    }

    protected void ToAlertState()
    {
        stateMachine.SetState(StateOption.AlertState);
    }

    protected void ToPatrolState()
    {
        stateMachine.SetState(StateOption.PatrolState);
    }
    protected void ToChaseState()
    {
        stateMachine.SetState(StateOption.ChaseState);
    }
    protected void ToTrackingState()
    {
        stateMachine.SetState(StateOption.TrackingState);
    }

    protected void ToDistractedState()
    {
        stateMachine.SetState(StateOption.DistractedState);
    }
    protected void ToControlledState()
    {
        stateMachine.SetState(StateOption.ControlledState);
    }

    protected void ToDetectionState()
    {
        stateMachine.SetState(StateOption.DetectionState);
    }
}
