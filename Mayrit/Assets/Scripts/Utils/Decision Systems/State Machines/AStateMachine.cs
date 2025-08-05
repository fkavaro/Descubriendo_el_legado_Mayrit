using UnityEngine;

/// <summary>
/// Abstract class for a state machine that handles the states of a controller.
/// </summary>
public abstract class AStateMachine<TController, TStateMachineType> : ADecisionSystem<TController>
where TController : ABehaviourController<TController>
where TStateMachineType : AStateMachine<TController, TStateMachineType>
{
    public AState<TController, TStateMachineType> CurrentState => _currentState;

    protected AState<TController, TStateMachineType> _currentState, _initialState;

    // Constructor
    protected AStateMachine(TController controller) : base(controller) { }

    #region TO BE IMPLEMENTED METHODS
    public abstract void SwitchState(AState<TController, TStateMachineType> state);
    #endregion

    public virtual void SetInitialState(AState<TController, TStateMachineType> state)
    {
        if (state == _currentState) return;

        _initialState = state;
    }

    public bool IsCurrentState(AState<TController, TStateMachineType> state)
    {
        return _currentState == state;
    }

    public virtual void ForceState(AState<TController, TStateMachineType> newState)
    {
        if (newState == _currentState) return;

        // Don't exit the current state, just set and start the new one
        _currentState = newState;
        DebugDecision();
        _currentState.StartState();
    }
    #region INHERITED METHODS
    /// <summary>
    /// Switchs back to initial state
    /// </summary>
    public override void Reset()
    {
        SwitchState(_initialState);
    }

    /// <summary>
    /// Debugs the current state of the state machine.
    /// </summary>
    protected override void DebugDecision()
    {
        if (_controller._debugMode)
            Debug.Log("[" + _controller.name + "]" + " is " + _currentState.Name);
    }

    #endregion

    #region UNITY EXECUTION EVENTS
    public override void Awake()
    {
        _currentState = _initialState;
        DebugDecision();
        _currentState?.AwakeState();
    }

    public override void Start()
    {
        _currentState?.StartState();
    }

    public override void Update()
    {
        if (!_controller._isExecutionPaused)
            _currentState?.OnUpdateState();
    }
    #endregion

    # region COLLISION AND TRIGGER EVENTS
    public override void OnCollisionEnter(Collision collision)
    {
        _currentState?.OnCollisionEnter(collision);
    }

    public override void OnCollisionStay(Collision collision)
    {
        _currentState?.OnCollisionStay(collision);
    }

    public override void OnCollisionExit(Collision collision)
    {
        _currentState?.OnCollisionExit(collision);
    }

    public override void OnTriggerEnter(Collider other)
    {
        _currentState?.OnTriggerEnter(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        _currentState?.OnTriggerStay(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        _currentState?.OnTriggerExit(other);
    }
    #endregion
}
