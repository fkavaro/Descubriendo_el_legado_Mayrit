using UnityEngine;

/// <summary>
/// Defines a class to handle a decision system for a controllable object.
/// </summary>
public class BehaviourController : MonoBehaviour
{
    [Header("Behaviour Controller Properties")]
    [Tooltip("Whether to show debug messages in the console or not")]
    public bool _debugMode = false;
    [Tooltip("Whether to update next frame or not")]
    public bool _isExecutionPaused = false;

    public ADecisionSystem _decisionSystem;

    /// <summary>
    /// Resets the decision system.
    /// </summary>
    public void ResetBehaviour()
    {
        _decisionSystem.Reset();
    }

    // #region UNITY EXECUTION METHODS
    // void Awake()
    // {
    //     OnAwake();
    //     _decisionSystem.Awake();
    // }
    // protected virtual void OnAwake() { } // Optionally implemented in subclasses

    // void Start()
    // {
    //     OnStart();
    //     _decisionSystem.Start();
    // }
    // protected virtual void OnStart() { } // Optionally implemented in subclasses

    // void Update()
    // {
    //     OnUpdate();
    //     _decisionSystem.Update();
    // }
    // protected virtual void OnUpdate() { } // Optionally implemented in subclasses

    // void LateUpdate()
    // {
    //     OnLateUpdate();
    //     _decisionSystem.LateUpdate();
    // }
    // protected virtual void OnLateUpdate() { } // Optionally implemented in subclasses

    // #endregion

    //     // #region COLLISION AND TRIGGER EVENTS
    //     void OnCollisionEnter(Collision collision)
    //     {
    //         _decisionSystem.OnCollisionEnter(collision);
    //         AtOnCollisionEnter(collision);
    //     }
    //     protected virtual void AtOnCollisionEnter(Collision collision) { } // Optionally implemented in subclasses

    //     void OnCollisionStay(Collision collision)
    //     {
    //         _decisionSystem.OnCollisionStay(collision);
    //         AtOnCollisionStay(collision);
    //     }
    //     protected virtual void AtOnCollisionStay(Collision collision) { } // Optionally implemented in subclasses

    //     void OnCollisionExit(Collision collision)
    //     {
    //         _decisionSystem.OnCollisionExit(collision);
    //         AtOnCollisionExit(collision);
    //     }
    //     protected virtual void AtOnCollisionExit(Collision collision) { } // Optionally implemented in subclasses


    //     void OnTriggerEnter(Collider other)
    //     {
    //         _decisionSystem.OnTriggerEnter(other);
    //         AtOnTriggerEnter(other);
    //     }
    //     protected virtual void AtOnTriggerEnter(Collider other) { } // Optionally implemented in subclasses

    //     void OnTriggerStay(Collider other)
    //     {
    //         _decisionSystem.OnTriggerEnter(other);
    //         AtOnTriggerStay(other);
    //     }
    //     protected virtual void AtOnTriggerStay(Collider other) { } // Optionally implemented in subclasses

    //     void OnTriggerExit(Collider other)
    //     {
    //         _decisionSystem.OnTriggerEnter(other);
    //         AtOnTriggerExit(other);
    //     }
    //     protected virtual void AtOnTriggerExit(Collider other) { } // Optionally implemented in subclasses
    // #endregion
}
