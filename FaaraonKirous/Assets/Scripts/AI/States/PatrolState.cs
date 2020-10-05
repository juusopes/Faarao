﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PatrolState : State
{
    private int nextWaypoint;

    public PatrolState(Character character, StateMachine stateMachine) : base(character, stateMachine) { }

    public override void Tick()
    {
        Patrol();
        Look();
    }

    void Patrol()
    {
        character.OnWaypoint();

        if (character.HasFinishedWaypointTask())
        {
            character.GetNextWaypoint();
        }
    }

    void Look()
    {
        if (character.CanSeePlayer())
            ToChaseState();
    }
}

