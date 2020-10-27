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
        stateMachine.SetState(stateMachine.alertState);
    }

    protected void ToPatrolState()
    {
        stateMachine.SetState(stateMachine.patrolState);
    }
    protected void ToChaseState()
    {
        stateMachine.SetState(stateMachine.chaseState);
    }
    protected void ToTrackingState()
    {
        stateMachine.SetState(stateMachine.trackingState);
    }

    protected void ToDistractedState()
    {
        stateMachine.SetState(stateMachine.distractedState);
    }
    protected void ToControlledState()
    {
        stateMachine.SetState(stateMachine.controlledState);
    }

    protected void ToDetectionState()
    {
        stateMachine.SetState(stateMachine.detectionState);
    }
}
