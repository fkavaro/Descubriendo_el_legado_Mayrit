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
    #endregion

    #region CONSTRUCTOR
    public AState(string statename)
    {
        _stateName = statename;
    }
    #endregion

    #region VIRTUAL METHODS
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
}