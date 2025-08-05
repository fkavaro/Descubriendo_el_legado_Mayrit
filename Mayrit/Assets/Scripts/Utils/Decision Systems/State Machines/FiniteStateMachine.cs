
/// <summary>
/// Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class FiniteStateMachine<TController> : AStateMachine<TController, FiniteStateMachine<TController>>
where TController : ABehaviourController<TController>
{
    public FiniteStateMachine(TController controller) : base(controller) { }

    #region INHERITED METHODS
    /// <summary>
    /// Switchs to another state after exiting the current.
    /// </summary>
    public override void SwitchState(AState<TController, FiniteStateMachine<TController>> state)
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
