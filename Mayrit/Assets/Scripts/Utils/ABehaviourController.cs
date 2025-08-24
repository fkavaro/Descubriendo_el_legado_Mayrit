using System;
using UnityEngine;
using TMPro;

public class ABehaviourController
{
    public readonly IBehaviourControllable _controllable;

    public ADecisionSystem _decisionSystem;

    // Construtor
    public ABehaviourController(IBehaviourControllable controllable)
    {
        _controllable = controllable;
    }

    /// <summary>
    /// Create the main decision system. Executed after OnAwake().
    /// </summary>
    //protected abstract ADecisionSystem<TController> CreateDecisionSystem();

    /// <summary>
    /// Resets the decision system.
    /// </summary>
    public void ResetBehaviour()
    {
        _decisionSystem?.Reset();
    }

    #region UNITY EXECUTION EVENTS
    public void Awake()
    {
        OnAwake();
        _decisionSystem?.Awake();
    }
    protected virtual void OnAwake() { } // Optionally implemented in subclasses

    public void Start()
    {
        OnStart();
        _decisionSystem?.Start();
    }
    protected virtual void OnStart() { } // Optionally implemented in subclasses

    public void Update()
    {
        OnUpdate();
        _decisionSystem?.Update();
    }
    protected virtual void OnUpdate() { } // Optionally implemented in subclasses

    public void LateUpdate()
    {
        OnLateUpdate();
        _decisionSystem?.LateUpdate();
    }
    protected virtual void OnLateUpdate() { } // Optionally implemented in subclasses

    #endregion

    // TODO: remove
    // #region COLLISION AND TRIGGER EVENTS
    // private void OnCollisionEnter(Collision collision)
    // {
    //     _decisionSystem?.OnCollisionEnter(collision);
    //     AtOnCollisionEnter(collision);
    // }
    // protected virtual void AtOnCollisionEnter(Collision collision) { } // Optionally implemented in subclasses

    // private void OnCollisionStay(Collision collision)
    // {
    //     _decisionSystem?.OnCollisionStay(collision);
    //     AtOnCollisionStay(collision);
    // }
    // protected virtual void AtOnCollisionStay(Collision collision) { } // Optionally implemented in subclasses

    // private void OnCollisionExit(Collision collision)
    // {
    //     _decisionSystem?.OnCollisionExit(collision);
    //     AtOnCollisionExit(collision);
    // }
    // protected virtual void AtOnCollisionExit(Collision collision) { } // Optionally implemented in subclasses


    // private void OnTriggerEnter(Collider other)
    // {
    //     _decisionSystem?.OnTriggerEnter(other);
    //     AtOnTriggerEnter(other);
    // }
    // protected virtual void AtOnTriggerEnter(Collider other) { } // Optionally implemented in subclasses

    // private void OnTriggerStay(Collider other)
    // {
    //     _decisionSystem?.OnTriggerEnter(other);
    //     AtOnTriggerStay(other);
    // }
    // protected virtual void AtOnTriggerStay(Collider other) { } // Optionally implemented in subclasses

    // private void OnTriggerExit(Collider other)
    // {
    //     _decisionSystem?.OnTriggerEnter(other);
    //     AtOnTriggerExit(other);
    // }
    // protected virtual void AtOnTriggerExit(Collider other) { } // Optionally implemented in subclasses
    // #endregion
}
