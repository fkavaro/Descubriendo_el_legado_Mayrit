using UnityEngine;
using System.Collections;

/// <summary>
/// Base class with common functionalities for all states.
/// </summary>
public abstract class AState
{
    #region PROPERTIES
    public string StateName => _stateName;
    protected string _stateName;
    protected float _stateTime = 0f;
    #endregion

    #region CONSTRUCTOR
    public AState(string statename)
    {
        _stateName = statename;
    }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual void AwakeState() { }
    public virtual void StartState() { }
    public virtual void UpdateState() { }
    public virtual void LateUpdateState() { }
    public virtual void ExitState() { }

    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerStay(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }
    #endregion

    #region PUBLIC METHODS
    public void OnUpdateState()
    {
        _stateTime += Time.deltaTime; // Update the state time
        UpdateState(); // Call the UpdateState method implemented in subclasses
    }

    public void OnLateUpdateState()
    {
        LateUpdateState();
    }

    public void OnExitState()
    {
        _stateTime = 0f; // Reset the state time
        ExitState(); // Call the ExitState method implemented in subclasses
    }
    #endregion
}