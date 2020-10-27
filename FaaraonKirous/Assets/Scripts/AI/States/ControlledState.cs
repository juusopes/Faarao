using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControlledState : State
{
    private float controlledTimer;

    public ControlledState(Character character, StateMachine stateMachine) : base(character, stateMachine){}

    public override void Tick()
    {
        ActPosessed();
    }

    public override void OnStateEnter()
    {
        character.StopNavigation();
        character.additionalTarget = UtilsClass.GetMinVector();
        controlledTimer = 0;
        character.clickSelector.SetActive(false);
        character.SetSightVisuals(false);
        character.isPosessed = true;
    }

    public override void OnStateExit()
    {
        character.clickSelector.SetActive(true);
        character.SetSightVisuals(true);
        character.isPosessed = false;
    }

    void ActPosessed()
    {
        if (!UtilsClass.IsMinimumVector(character.additionalTarget))
            character.SetDestination(character.additionalTarget);

        controlledTimer += Time.deltaTime;
        if (controlledTimer >= character.classSettings.controlledDuration)
        {
            character.StopNavigation();
            ToAlertState();
        }
    }
}

