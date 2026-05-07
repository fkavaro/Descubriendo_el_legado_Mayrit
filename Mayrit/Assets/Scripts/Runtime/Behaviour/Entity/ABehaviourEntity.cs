using UnityEngine;
using System.Collections;

/// <summary>
/// Abstract behaviour entity class with a generic behaviour system.
/// </summary>
public abstract class ABehaviourEntity<BehaviourSystemType> : MonoBehaviour, IBehaviourEntity
where BehaviourSystemType : ABehaviourSystem
{
    #region PROPERTIES HELPERS
    public string Name => gameObject.name;
    public GameObject GO => gameObject;
    public BehaviourSystemType BehaviourSystem
    {
        get => _behaviourSystem;
        set => _behaviourSystem = value;
    }

    public bool IsExecutionPaused
    {
        get => _isExecutionPaused;
        set => _isExecutionPaused = value;
    }

    public bool DebugMode
    {
        get => _debugMode;
        set => _debugMode = value;
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
    [SerializeField] protected bool _debugMode = false;
    [Tooltip("Whether to pause the execution of the behaviour system or not")]
    [SerializeField] protected bool _isExecutionPaused;
    [SerializeField, ReadOnly] protected string _currentActionInfo = "";
    #endregion

    #region INTERNAL PROPERTIES
    BehaviourSystemType _behaviourSystem;
    #endregion

    #region TO BE IMPLEMENTED METHODS
    /// <summary>
    /// Returned value will be assigned to the BehaviourSystem property.
    /// Is executed automatically in Awake().
    /// </summary>
    public abstract BehaviourSystemType DefineBehaviourSystem();
    #endregion

    #region LIFE CYCLE: DERIVED TO BEHAVIOUR SYSTEM
    protected virtual void Awake()
    {
        _behaviourSystem = DefineBehaviourSystem();
    }

    protected virtual void Start()
    {
        _behaviourSystem?.Start();
    }

    protected virtual void Update()
    {
        _behaviourSystem?.Update();
    }

    protected virtual void LateUpdate()
    {
        _behaviourSystem?.LateUpdate();
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        _behaviourSystem?.OnCollisionEnter(collision);
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        _behaviourSystem?.OnCollisionStay(collision);
    }

    protected virtual void OnCollisionExit(Collision collision)
    {
        _behaviourSystem?.OnCollisionExit(collision);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        _behaviourSystem?.OnTriggerEnter(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        _behaviourSystem?.OnTriggerStay(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        _behaviourSystem?.OnTriggerExit(other);
    }

    void IBehaviourEntity.StartCoroutine(IEnumerator enumerator)
    {
        StartCoroutine(enumerator);
    }
    #endregion
}
