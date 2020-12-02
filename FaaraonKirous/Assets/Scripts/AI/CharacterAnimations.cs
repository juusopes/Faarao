using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations
{
    Character character;
    Animator animator;
    public AnimationState currentState;

    private Dictionary<AnimationState, string> states = new Dictionary<AnimationState, string>()
        {
            {AnimationState.Idle, "Goon_Idle"},
            {AnimationState.Walk, "Goon_walking"},
            {AnimationState.Run, "Goon_Run"},
            {AnimationState.WalkTurn, "Goon_Walkturn"},
            {AnimationState.Death, "Goon_dying"},
        };

    public CharacterAnimations(Character character)
    {
        this.character = character;
        animator = character.transform.GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogWarning("No animator found");
    }

    public void SetAnimationState(AnimationState state)
    {
        if (animator == null)
            return;
        if (currentState == state)
            return;
        if (!states.ContainsKey(state))
            return;

        string stateName = states[state];
        if (!String.IsNullOrEmpty(stateName))
        {
            currentState = state;
            animator.Play(stateName);
        }

    }
}
