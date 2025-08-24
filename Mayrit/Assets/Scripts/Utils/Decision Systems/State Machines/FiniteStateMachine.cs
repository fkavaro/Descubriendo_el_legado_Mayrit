
using UnityEngine;

/// <summary>
/// Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class FiniteStateMachine : AStateMachine<FiniteStateMachine>
{
    // Constructor
    public FiniteStateMachine(ABehaviourController controller)
    : base(controller) { }

    #region INHERITED METHODS
    /// <summary>
    /// Switchs to another state after exiting the current.
    /// </summary>
    public override void SwitchState(AState<FiniteStateMachine> state)
    {
        if (state == _currentState) return;

        _currentState?.OnExitState();
        _currentState = state;
        DebugDecision();
        _currentState?.AwakeState();
        _currentState?.StartState();
    }
    #endregion
}
