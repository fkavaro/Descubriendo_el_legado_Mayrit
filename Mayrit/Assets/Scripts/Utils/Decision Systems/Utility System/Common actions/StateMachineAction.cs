using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action that runs a finite state machine (FSM) by an Utility System.
/// </summary>
public class StateMachineAction<TController, TStateMachine> : ABinaryAction<TController>
where TController : ABehaviourController<TController>
where TStateMachine : AStateMachine<TController, TStateMachine>
{
    TStateMachine _stateMachine;
    bool _alreadyStarted = false;

    public StateMachineAction(UtilitySystem<TController> utilitySystem, TStateMachine stateMachine)
        : base("FSM", utilitySystem, 0.5f)
    {
        _stateMachine = stateMachine;
    }

    protected override bool SetDecisionFactor()
    {
        return true; // Will remain valid action
    }

    public override void StartAction()
    {
        // Start just once - to maintain current state after returning from other action
        if (!_alreadyStarted)
        {
            _alreadyStarted = true;
            _stateMachine.Start();
        }
    }

    public override void UpdateAction()
    {
        _stateMachine.Update();
    }

    public override bool IsFinished()
    {
        return true; // Allows evaluation of other actions
    }

    /// <returns>State name of FSM action</returns>
    public override string DebugDecision()
    {
        return _stateMachine.GetCurrentStateName();
    }

    public override void Reset()
    {
        _alreadyStarted = false;
        _stateMachine.Reset();
    }

    public void ForceState(AState<TController, TStateMachine> newState)
    {
        _stateMachine.ForceState(newState);
    }

    public bool IsCurrentState(AState<TController, TStateMachine> state)
    {
        return _stateMachine.IsCurrentState(state);
    }
}