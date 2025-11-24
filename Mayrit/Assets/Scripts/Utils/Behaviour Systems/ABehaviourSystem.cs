using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract class for behaviour decision systems.
/// </summary>
public abstract class ABehaviourSystem : IBehaviourSystem
{
    #region PROPERTIES HELPERS
    public IBehaviourEntity BehaviourEntity => _behaviourEntity;
    #endregion

    #region PROPERTIES
    public IBehaviourEntity _behaviourEntity;
    #endregion

    #region CONSTRUCTOR
    public ABehaviourSystem(IBehaviourEntity behaviourEntity)
    {
        _behaviourEntity = behaviourEntity;

        if (_behaviourEntity == null)
            Debug.LogError(_behaviourEntity.GO.name + ": Behaviour Entity is null in " + GetType().Name);
    }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    /// <summary>
    /// Debugs the current decision of the system.
    /// </summary>
    public abstract void DebugDecision();

    /// <summary>
    /// Resets the decision system to its initial state.
    /// </summary>
    public abstract void Reset();
    #endregion

    #region MONOBEHAVIOUR EQUIVALENTS: OPTIONAL
    public virtual void Awake() { }
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }
    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerStay(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
    #endregion
}