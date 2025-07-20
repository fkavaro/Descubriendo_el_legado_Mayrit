
/// <summary>
/// Finite State Machine implementation for controlling a behaviour.
/// </summary>
public class FiniteStateMachine<TController> : AStateMachine<TController, FiniteStateMachine<TController>> where TController : ABehaviourController<TController>
{
    public FiniteStateMachine(TController controller) : base(controller) { }

    #region INHERITED METHODS
    /// <summary>
    /// Sets the initial state of the state machine.
    /// </summary>
    public override void SetInitialState(AState<TController, FiniteStateMachine<TController>> state)
    {
        if (state == currentState) return;

        initialState = state;
    }

    /// <summary>
    /// Switchs to another state after exiting the current..
    /// </summary>
    public override void SwitchState(AState<TController, FiniteStateMachine<TController>> state)
    {
        if (state == currentState) return;

        currentState?.OnExitState();
        currentState = state;
        DebugDecision();
        currentState?.StartState();
    }
    #endregion
}
