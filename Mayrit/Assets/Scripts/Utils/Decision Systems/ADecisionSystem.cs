using UnityEngine;

[RequireComponent(typeof(IBehaviourControllable))]
/// <summary>
/// Abstract class that defines a decision system for a controllable object.
/// </summary>
public abstract class ADecisionSystem : MonoBehaviour
{
    public IBehaviourControllable _controllable;

    [Tooltip("Whether to show debug messages in the console or not")]
    public bool _debugMode = false;
    [Tooltip("Whether to update next frame of the system or not")]
    public bool _isExecutionPaused = false;

    protected abstract void DebugDecision();
    public abstract void Reset();
    /// <summary>
    /// To be implemented as Awake in derived classes. 
    /// Will be executed late in Awake.
    /// </summary>
    protected virtual void OnAwake() { }

    void Awake()
    {
        if (_debugMode)
            Debug.Log(gameObject.name + " ADecisionSystem: Setting Controllable");

        _controllable = GetComponent<IBehaviourControllable>();
        _controllable.SetDecisionSystem();

        OnAwake();
    }
}