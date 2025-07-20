using System.Collections;
using UnityEngine;

/// <summary>
/// Base class with common functionalities for all states.
/// </summary>
public abstract class AState<TController, TStateMachine>
    where TController : ABehaviourController<TController>
    where TStateMachine : AStateMachine<TController, TStateMachine>
{
    public string StateName => _stateName;

    protected string _stateName;
    protected TController _controller;
    protected TStateMachine _stateMachine;
    protected float _stateTime = 0f;

    // Constructor given AStateMachine
    public AState(string name, TStateMachine stateMachine)
    {
        _stateName = name;
        _stateMachine = stateMachine;
        _controller = stateMachine.controller;
    }

    public void SwitchState(AState<TController, TStateMachine> nextState)
    {
        _stateMachine?.SwitchState(nextState);
    }

    public void ReturnToPreviousState()
    {
        // StateMachine is an stack state machine
        if (_stateMachine is StackFiniteStateMachine<TController> stateFSM)
        {
            stateFSM.Pop();
        }
        else
        {
            Debug.LogError("State machine is not a stack state machine. Cannot return to previous state.");
            return;
        }

    }

    public virtual void AwakeState() { } // Optionally implemented in subclasses
    public abstract void StartState(); // Implemented in subclasses

    public void OnUpdateState()
    {
        _stateTime += Time.deltaTime; // Update the state time
        UpdateState(); // Call the UpdateState method implemented in subclasses
    }

    public abstract void UpdateState(); // Implemented in subclasses

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

    // /// <summary>
    // /// Coroutine to wait for a random amount of time before switching to the next state.
    // /// </summary>
    // protected IEnumerator SwitchStateAfterRandomTime(AState<TController, TStateMachine> nextState)
    // {
    //     int waitTime = Random.Range(5, 21);
    //     return SwitchStateAfterCertainTime(waitTime, nextState);
    // }

    // /// <summary>
    // /// Coroutine to wait for a specified amount of time before switching to the next state.
    // /// </summary>
    // protected virtual IEnumerator SwitchStateAfterCertainTime(float waitTime, AState<TController, TStateMachine> nextState)
    // {
    //     _controller.isExecutionPaused = true;

    //     yield return new WaitForSeconds(waitTime);

    //     _stateMachine?.SwitchState(nextState);
    //     _controller.isExecutionPaused = false;
    // }
}