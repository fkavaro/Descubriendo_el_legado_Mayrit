using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stack-based Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class StackFiniteStateMachine<StateType> : AStateMachine<StateType>
where StateType : AState
{
    readonly Stack<StateType> _stateStack = new();

    #region CONSTRUCTOR
    public StackFiniteStateMachine(IBehaviourEntity entity)
    : base(entity) { }
    #endregion

    #region INHERITED METHODS
    /// <summary>
    /// Switchs to another state after exiting the current,
    /// storing it in the stack.
    /// </summary>
    public override void SwitchState(StateType newState)
    {
        if (newState == _currentState)
        {
            if (_behaviourEntity.DebugMode)
                Debug.LogWarning($"{_behaviourEntity.GO.name} tried to switch to the same state: {newState?.StateName}");
            return;
        }

        // if (_behaviourEntity.DebugMode)
        //     Debug.Log($"{_behaviourEntity.GO.name} switching state from {_currentState?.StateName} to {newState?.StateName}");

        PushCurrentState();
        _currentState?.ExitState();
        _currentState = newState;
        DebugDecision();
        _currentState?.StartState();

        // Invoke switch event
        base.SwitchState(newState);
    }

    public override void ForceState(StateType newState)
    {
        if (newState == _currentState) return;

        PushCurrentState();
        // Don't exit the current state, just set and start the new one
        _currentState = newState;
        DebugDecision();
        _currentState?.StartState();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Returns previous state (top of the stack).
    /// </summary>
    public StateType GetPreviousState()
    {
        // Empty stack
        if (_stateStack.Count == 0)
            return null;
        else // Not empty
            // Get the last state from the stack without removing it
            return _stateStack.Peek();
    }

    public StateType PopPreviousState()
    {
        // Empty stack
        if (_stateStack.Count == 0)
            return null;
        else // Not empty
            // Get the last state from the stack and remove it
            return _stateStack.Pop();
    }

    /// <summary>
    /// Switches to the previous state in the stack,
    /// removing it from the stack.
    /// </summary>
    public StateType SwitchToPreviousStateInStack()
    {
        StateType previousState = PopPreviousState();

        if (previousState != null)
            SwitchState(previousState);

        return previousState;
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Pushes the current state onto the stack,
    /// allowing to return to it later.
    /// </summary>
    void PushCurrentState()
    {
        if (_currentState != null)
            _stateStack.Push(_currentState);
    }
    #endregion
}