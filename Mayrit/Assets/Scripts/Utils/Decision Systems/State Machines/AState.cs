using System.Collections;
using UnityEngine;

/// <summary>
/// Base class with common functionalities for all states.
/// </summary>
public abstract class AState<TStateMachine>
    where TStateMachine : AStateMachine<TStateMachine>
{
    public string Name => _stateName;
    protected string _stateName;
    protected readonly IBehaviourControllable _controllable;
    protected TStateMachine _stateMachine;
    protected float _stateTime = 0f;
    protected readonly AState<TStateMachine> _nextState;

    // Constructor
    public AState(string name,
    TStateMachine stateMachine)
    {
        _stateName = name;
        _stateMachine = stateMachine;
        _stateMachine.AddStateToSequence(this);
        _controllable = stateMachine._controllable;
    }

    /// <summary>
    /// Checks if this state is the current state in the state machine.
    /// </summary>
    public bool IsCurrentState()
    {
        return _stateMachine.IsCurrentState(this);
    }

    public virtual void SwitchState(AState<TStateMachine> nextState)
    {
        _stateMachine?.SwitchState(nextState);
    }

    public virtual void AwakeState() { } // Optionally implemented in subclasses
    public abstract void StartState(); // Implemented in subclasses

    public void OnUpdateState()
    {
        _stateTime += Time.deltaTime; // Update the state time
        UpdateState(); // Call the UpdateState method implemented in subclasses
    }
    public virtual void UpdateState() { } // Optionally implemented in subclasses

    public void OnLateUpdateState()
    {
        LateUpdateState(); // Call the LateUpdateState method implemented in subclasses
    }
    public virtual void LateUpdateState() { } // Optionally implemented in subclasses

    public void OnExitState()
    {
        _stateTime = 0f; // Reset the state time
        ExitState(); // Call the ExitState method implemented in subclasses
    }

    public virtual void ExitState() { } // Optionally in subclasses

    public virtual void OnTriggerEnter(Collider other) { } // Optionally implemented in subclasses
    public virtual void OnTriggerStay(Collider other) { } // Optionally implemented in subclasses
    public virtual void OnTriggerExit(Collider other) { } // Optionally implemented in subclasses

    public virtual void OnCollisionEnter(Collision collision) { } // Optionally implemented in subclasses
    public virtual void OnCollisionStay(Collision collision) { } // Optionally implemented in subclasses
    public virtual void OnCollisionExit(Collision collision) { } // Optionally implemented in subclasses

    /// <summary>
    /// Coroutine to wait for a random amount of time before switching to the next state.
    /// </summary>
    protected IEnumerator SwitchStateAfterRandomTime(AState<TStateMachine> nextState, int min = 5, int max = 21)
    {
        int waitTime = Random.Range(min, max);
        return SwitchStateAfterCertainTime(nextState, waitTime);
    }

    /// <summary>
    /// Coroutine to wait for a specified amount of time before switching to the next state.
    /// </summary>
    public virtual IEnumerator SwitchStateAfterCertainTime(AState<TStateMachine> nextState, float waitTime)
    {
        _controllable.IsExecutionPaused = true;

        yield return new WaitForSeconds(waitTime);

        _stateMachine?.SwitchState(nextState);
        _controllable.IsExecutionPaused = false;
    }
}