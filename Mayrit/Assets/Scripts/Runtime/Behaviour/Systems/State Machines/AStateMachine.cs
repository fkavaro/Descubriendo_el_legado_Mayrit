using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract class for a state machine that handles the states of a controller.
/// </summary>
public abstract class AStateMachine<GenericState> : ABehaviourSystem
where GenericState : AState
{
    #region PROPERTIES
    readonly List<GenericState> _statesSequence = new();
    public GenericState CurrentState => _currentState;
    protected GenericState _currentState, _initialState;
    public int CurrentStateIndex => _statesSequence.IndexOf(_currentState);

    public event Action SwitchedStateEvent;
    #endregion

    #region CONSTRUCTOR
    protected AStateMachine(IBehaviourEntity entity)
    : base(entity) { }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual void SwitchState(GenericState newState)
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
        if (_behaviourEntity.DebugMode)
            Debug.Log($"{_behaviourEntity.GO.name} switched to state: {_currentState.StateName}");

        _behaviourEntity.CurrentAction = _currentState.StateName;
    }
    #endregion

    #region PUBLIC METHODS
    public virtual void SetInitialState(GenericState state)
    {
        if (state == _currentState) return;

        _initialState = state;
    }

    public bool IsCurrentState(GenericState state)
    {
        return _currentState == state && _currentState != null;
    }

    public virtual void ForceState(GenericState newState)
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
    public IEnumerator SwitchStateAfterRandomTime(GenericState nextState, int min = 5, int max = 21)
    {
        int waitTime = UnityEngine.Random.Range(min, max);
        return SwitchStateAfterCertainTime(nextState, waitTime);
    }

    /// <summary>
    /// Coroutine to wait for a specified amount of time before switching to the next state.
    /// </summary>
    public virtual IEnumerator SwitchStateAfterCertainTime(GenericState nextState, float waitTime)
    {
        _behaviourEntity.IsExecutionPaused = true;

        yield return new WaitForSeconds(waitTime);

        SwitchState(nextState);
        _behaviourEntity.IsExecutionPaused = false;
    }
    #endregion

    #region STATE SEQUENCE METHODS
    public void AddStateToSequence(GenericState state)
    {
        if (_statesSequence.Contains(state)) return;
        _statesSequence.Add(state);
        _initialState ??= state; // Set as initial state if none set
    }

    /// <summary>
    /// Switches to the previous state in the list.
    /// </summary>
    public int SwitchToPreviousStateInSequence()
    {
        int previousStateIndex = -1;

        if (_statesSequence.Count == 0 || _currentState == null)
            return previousStateIndex;


        if (CurrentStateIndex <= 0) // First state
        {
            _currentState.ExitState();
            return previousStateIndex;
        }

        previousStateIndex = CurrentStateIndex - 1;
        GenericState previousState = _statesSequence[previousStateIndex];
        SwitchState(previousState);
        return previousStateIndex;
    }

    /// <summary>
    /// Switches to the next state in the list.
    /// </summary>
    public virtual int SwitchToNextStateInSequence()
    {
        int nextStateIndex = -1;

        if (_statesSequence.Count == 0 || _currentState == null)
            return nextStateIndex;

        if (CurrentStateIndex >= _statesSequence.Count - 1) // Last state
        {
            _currentState.ExitState();
            return nextStateIndex;
        }

        nextStateIndex = CurrentStateIndex + 1;
        GenericState nextState = _statesSequence[nextStateIndex];
        SwitchState(nextState);
        return nextStateIndex;
    }

    public bool AtFistStateInSequence()
    {
        return _currentState == _statesSequence[0];
    }

    public bool AtLastStateInSequence()
    {
        return _currentState == _statesSequence[^1];
    }

    public void SetInitialStateFromSequence(int index)
    {
        if (index < 0 || index >= _statesSequence.Count)
        {
            Debug.LogError("Index out of range when setting initial state from sequence.");
            return;
        }

        SetInitialState(_statesSequence[index]);
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
