using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract class for a state machine that handles the states of a controller.
/// </summary>
public abstract class AStateMachine<StateType> : ABehaviourSystem
where StateType : AState
{
    #region PROPERTIES
    public StateType CurrentState => _currentState;
    protected StateType _currentState, _initialState;
    protected List<StateType> _statesSequence = new();

    public event Action SwitchedStateEvent;
    #endregion

    #region CONSTRUCTOR
    protected AStateMachine(IBehaviourEntity entity)
    : base(entity) { }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual void SwitchState(StateType newState)
    {
        SwitchedStateEvent?.Invoke();
    }
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
    public override void DebugDecision()
    {
        _behaviourEntity.CurrentActionInfo = _currentState.StateName;
    }
    #endregion

    #region PUBLIC METHODS
    public virtual void SetInitialState(StateType state)
    {
        if (state == _currentState) return;

        _initialState = state;
    }

    public bool IsCurrentState(StateType state)
    {
        return _currentState == state && _currentState != null;
    }

    public virtual void ForceState(StateType newState)
    {
        if (newState == _currentState) return;

        // Don't exit the current state, just set and start the new one
        _currentState = newState;
        DebugDecision();
        _currentState.StartState();
    }

    /// <summary>
    /// Coroutine to wait for a random amount of time before switching to the next state.
    /// </summary>
    public IEnumerator SwitchStateAfterRandomTime(StateType nextState, int min = 5, int max = 21)
    {
        int waitTime = UnityEngine.Random.Range(min, max);
        return SwitchStateAfterCertainTime(nextState, waitTime);
    }

    /// <summary>
    /// Coroutine to wait for a specified amount of time before switching to the next state.
    /// </summary>
    public virtual IEnumerator SwitchStateAfterCertainTime(StateType nextState, float waitTime)
    {
        _behaviourEntity.IsExecutionPaused = true;

        yield return new WaitForSeconds(waitTime);

        SwitchState(nextState);
        _behaviourEntity.IsExecutionPaused = false;
    }
    #endregion

    #region STATE SEQUENCE METHODS
    public void AddStateToSequence(StateType state)
    {
        if (_statesSequence.Contains(state)) return;
        _statesSequence.Add(state);
        _initialState ??= state; // Set as initial state if none set
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

        StateType previousState = _statesSequence[currentIndex - 1];

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

        StateType nextState = _statesSequence[currentIndex + 1];

        SwitchState(nextState);
        return true;
    }

    public bool AtFistStateInSequence()
    {
        return _currentState == _statesSequence[0];
    }

    public bool AtLastStateInSequence()
    {
        return _currentState == _statesSequence[^1];
    }
    #endregion

    #region LIFE CYCLE: DERIVED TO CURRENT STATE
    public override void Start()
    {
        if (_initialState == null)
        {
            Debug.LogWarning(_behaviourEntity.GO.name + ": AStateMachine has no initial state set.");
            return;
        }

        SwitchState(_initialState);
    }

    public override void Update()
    {
        if (!_behaviourEntity.IsExecutionPaused)
            _currentState?.UpdateState();
    }

    public override void LateUpdate()
    {
        if (!_behaviourEntity.IsExecutionPaused)
            _currentState?.LateUpdateState();
    }

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
