using UnityEngine;

/// <summary>
/// Abstract class for a state machine that handles the states of a controller.
/// </summary>
public abstract class AStateMachine<TController, TStateMachineType> : ADecisionSystem<TController>
where TController : ABehaviourController<TController>
where TStateMachineType : AStateMachine<TController, TStateMachineType>
{
    protected AState<TController, TStateMachineType> currentState, initialState;

    protected AStateMachine(TController controller) : base(controller) { }

    #region TO BE IMPLEMENTED METHODS
    public abstract void SetInitialState(AState<TController, TStateMachineType> state);
    public abstract void SwitchState(AState<TController, TStateMachineType> state);
    #endregion

    public string GetCurrentStateName()
    {
        return currentState?.StateName;
    }

    public bool IsCurrentState(AState<TController, TStateMachineType> state)
    {
        return currentState == state;
    }

    #region INHERITED METHODS
    /// <summary>
    /// Switchs back to initial state
    /// </summary>
    public override void Reset()
    {
        SwitchState(initialState);
    }

    /// <summary>
    /// Debugs the current state of the state machine.
    /// </summary>
    protected override void DebugDecision()
    {
        //if (controller.debugMode)
        //Debug.LogWarning(controller.transform.name + " is " + currentState.ToString());
        controller.actionText.text = GetCurrentStateName();
    }

    public virtual void ForceState(AState<TController, TStateMachineType> newState)
    {
        if (newState == currentState) return;

        // Don't exit the current state, just set and start the new one
        currentState = newState;
        DebugDecision();
        currentState.StartState();
    }
    #endregion

    #region UNITY EXECUTION EVENTS
    public override void Start()
    {
        currentState = initialState;
        DebugDecision();
        currentState?.StartState();
    }

    public override void Update()
    {
        if (!controller.isExecutionPaused)
            currentState?.OnUpdateState();
    }
    #endregion

    # region COLLISION AND TRIGGER EVENTS
    public override void OnCollisionEnter(Collision collision)
    {
        currentState?.OnCollisionEnter(collision);
    }

    public override void OnCollisionStay(Collision collision)
    {
        currentState?.OnCollisionStay(collision);
    }

    public override void OnCollisionExit(Collision collision)
    {
        currentState?.OnCollisionExit(collision);
    }

    public override void OnTriggerEnter(Collider other)
    {
        currentState?.OnTriggerEnter(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        currentState?.OnTriggerStay(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        currentState?.OnTriggerExit(other);
    }
    #endregion
}
