public abstract class State
{
    protected Character character;
    protected StateMachine stateMachine;

    public abstract void Tick();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    public State(Character character, StateMachine stateMachine)
    {
        this.character = character;
        this.stateMachine = stateMachine;
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
}
