using UnityEngine;

/// <summary>
/// Abstract behaviour entity class with a generic behaviour system.
/// </summary>
/// <typeparam name="T"> The type of the behaviour system.</typeparam>
public abstract class ABehaviourEntity<T> : MonoBehaviour, IBehaviourEntity
where T : ABehaviourSystem
{
    #region PROPERTIES HELPERS
    public string Name => gameObject.name;
    public GameObject GO => gameObject;

    public bool IsExecutionPaused
    {
        get => _isExecutionPaused;
        set => _isExecutionPaused = value;
    }

    public string CurrentActionInfo
    {
        get => _currentActionInfo;
        set => _currentActionInfo = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Behaviour System")]
    [Tooltip("Whether to show debug messages in the console or not")]
    [SerializeField] protected bool _debugMode;
    [Tooltip("Whether to pause the execution of the behaviour system or not")]
    [SerializeField] protected bool _isExecutionPaused;
    [SerializeField, ReadOnly]
    protected string _currentActionInfo = "";
    #endregion

    #region TO BE IMPLEMENTED METHODS
    /// <summary>
    /// Returned value will be assigned to the BehaviourSystem property.
    /// Is executed in Awake().
    /// </summary>
    public abstract T InitializeBehaviourSystem();

    /// <summary>
    /// The behaviour system of the entity.
    /// </summary>
    public T BehaviourSystem;
    #endregion

    #region MONOBEHAVIOUR: DERIVED TO BEHAVIOUR SYSTEM
    protected virtual void Awake()
    {
        BehaviourSystem = InitializeBehaviourSystem();
        BehaviourSystem?.Awake();
    }

    protected virtual void Start()
    {
        BehaviourSystem?.Start();
    }

    protected virtual void Update()
    {
        BehaviourSystem?.Update();
    }

    protected virtual void LateUpdate()
    {
        BehaviourSystem?.LateUpdate();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        BehaviourSystem?.OnCollisionEnter(collision);
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        BehaviourSystem?.OnCollisionStay(collision);
    }

    protected virtual void OnCollisionExit(Collision collision)
    {
        BehaviourSystem?.OnCollisionExit(collision);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        BehaviourSystem?.OnTriggerEnter(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        BehaviourSystem?.OnTriggerStay(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        BehaviourSystem?.OnTriggerExit(other);
    }
    #endregion
}
