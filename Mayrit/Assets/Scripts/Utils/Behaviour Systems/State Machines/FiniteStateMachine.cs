using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class FiniteStateMachine<StateType> : AStateMachine<StateType>
where StateType : AState
{
    #region CONSTRUCTOR
    public FiniteStateMachine(IBehaviourEntity entity)
    : base(entity) { }
    #endregion

    #region INHERITED METHODS
    /// <summary>
    /// Switchs to another state after exiting the current.
    /// </summary>
    public override void SwitchState(StateType newState)
    {
        if (newState == _currentState) return;

        _currentState?.OnExitState();
        _currentState = newState;
        DebugDecision();
        _currentState?.StartState();

        // Invoke switch event
        base.SwitchState(newState);
    }
    #endregion
}
