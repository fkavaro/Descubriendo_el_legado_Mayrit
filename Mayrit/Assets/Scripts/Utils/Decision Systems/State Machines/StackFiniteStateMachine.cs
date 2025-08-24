using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stack-based Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class StackFiniteStateMachine<TController> : AStateMachine<TController, StackFiniteStateMachine<TController>>
where TController : MonoBehaviour
{
    readonly Stack<AState<TController, StackFiniteStateMachine<TController>>> _stateStack = new();

    public StackFiniteStateMachine(ABehaviourController<TController> controller)
    : base(controller) { }

    #region INHERITED METHODS
    /// <summary>
    /// Switchs to another state after exiting the current,
    /// storing it in the stack.
    /// </summary>
    public override void SwitchState(AState<TController, StackFiniteStateMachine<TController>> newState)
    {
        if (newState == _currentState) return;

        PushCurrentState();
        _currentState?.OnExitState();
        _currentState = newState;
        DebugDecision();
        _currentState?.AwakeState();
        _currentState?.StartState();
    }

    public override void ForceState(AState<TController, StackFiniteStateMachine<TController>> newState)
    {
        if (newState == _currentState) return;

        PushCurrentState();
        // Don't exit the current state, just set and start the new one
        _currentState = newState;
        DebugDecision();
        _currentState?.AwakeState();
        _currentState?.StartState();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Pushes the current state onto the stack,
    /// allowing to return to it later.
    /// </summary>
    public void PushCurrentState()
    {
        if (_currentState != null)
            _stateStack.Push(_currentState);
    }

    /// <summary>
    /// Returns previous state (top of the stack).
    /// </summary>
    public AState<TController, StackFiniteStateMachine<TController>> GetPreviousState()
    {
        // Empty stack
        if (_stateStack.Count == 0)
        {
            if (_controller._debugMode)
                Debug.Log("[" + _controller._name + "] state stack is empty");

            return null;
        }
        else // Not empty
        {
            // Get the last state from the stack without removing it
            var previousState = _stateStack.Peek();

            if (_controller._debugMode)
                Debug.Log("[" + _controller._name + "] Previous state: " + previousState.Name);

            return previousState;
        }
    }

    /// <summary>
    /// Switches to the previous state in the stack,
    /// removing it from the stack.
    /// </summary>
    public bool SwitchToPreviousStateInStack()
    {
        // Empty stack
        if (_stateStack.Count == 0)
        {
            if (_controller._debugMode)
                Debug.Log("[" + _controller._name + "] state stack is empty");

            return false;
        }

        // Not empty stack
        SwitchState(_stateStack.Pop());
        return true;
    }
    #endregion
}