using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class for a state machine that handles the states of a controller.
/// </summary>
public abstract class AStateMachine<TStateMachineType> : ADecisionSystem
where TStateMachineType : AStateMachine<TStateMachineType>
{
    public AState<TStateMachineType> CurrentState => _currentState;

    protected AState<TStateMachineType> _currentState, _initialState;
    protected List<AState<TStateMachineType>> _statesSequence = new();

    // Constructor
    public AStateMachine(ABehaviourController controller) : base(controller) { }

    #region TO BE IMPLEMENTED METHODS
    public abstract void SwitchState(AState<TStateMachineType> state);
    #endregion

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
        if (DebugMode)
            Debug.Log("[" + _controllable.Name + "]" + " is " + _currentState.Name);
    }
    #endregion

    #region PUBLIC METHODS
    public virtual void SetInitialState(AState<TStateMachineType> state)
    {
        if (state == _currentState) return;

        _initialState = state;
    }

    public bool IsCurrentState(AState<TStateMachineType> state)
    {
        return _currentState == state;
    }

    public virtual void ForceState(AState<TStateMachineType> newState)
    {
        if (newState == _currentState) return;

        // Don't exit the current state, just set and start the new one
        _currentState = newState;
        DebugDecision();
        _currentState.StartState();
    }

    public void AddStateToSequence(AState<TStateMachineType> state)
    {
        if (_statesSequence.Contains(state)) return;
        _statesSequence.Add(state);
        _initialState ??= state;
    }

    /// <summary>
    /// Switches to the previous state in the list.
    /// </summary>
    public bool SwitchToPreviousStateInSequence()
    {
        if (_statesSequence.Count == 0 || _currentState == null) return false;

        int currentIndex = _statesSequence.IndexOf(_currentState);

        if (currentIndex <= 0) // First state
            return false;

        AState<TStateMachineType> previousState = _statesSequence[currentIndex - 1];

        SwitchState(previousState);
        return true;
    }

    /// <summary>
    /// Switches to the next state in the list.
    /// </summary>
    public virtual bool SwitchToNextStateInSequence()
    {
        if (_statesSequence.Count == 0 || _currentState == null) return false;

        int currentIndex = _statesSequence.IndexOf(_currentState);

        if (currentIndex >= _statesSequence.Count - 1) // Last state
            return false;

        AState<TStateMachineType> nextState = _statesSequence[currentIndex + 1];

        SwitchState(nextState);
        return true;
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
        if (!_controllable.IsExecutionPaused)
            _currentState?.OnUpdateState();
    }

    public override void LateUpdate()
    {
        if (!_controllable.IsExecutionPaused)
            _currentState?.OnLateUpdateState();
    }

    #endregion

    #region COLLISION AND TRIGGER EVENTS
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
