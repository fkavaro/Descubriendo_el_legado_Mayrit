using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for NPC (Non-Playable Character).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public abstract class ANPC<BehaviourSystemType> : ACharacter<BehaviourSystemType>, INPC
where BehaviourSystemType : ABehaviourSystem
{
    #region PROPERTIES HELPERS
    public NavMeshAgent Agent => _agent;
    public NPCMovementController MovementController => _movementController;
    public NPCInteractionController InteractionController => _interactionController;
    public float AvoidanceRadius => _avoidanceRadius;
    public float MaxSamplingDistance => _maxSamplingDistance;
    public int BaseAvoidancePriority => _baseAvoidancePriority;
    public int AvoidancePriorityVariance => _avoidancePriorityVariance;
    public float WalkSpeedVariance => _walkSpeedVariance;
    public House Home => _home;
    public Workplace Workplace => _workplace;
    public Sanctuary Sanctuary => _sanctuary;
    public Market Market => _market;
    public Stall MarketStall
    {
        get => _marketStall;
        set => _marketStall = value;
    }

    public INPC.RoleInConversation ConversationRole
    {
        get => _conversationRole;
        set => _conversationRole = value;
    }

    public bool InAccessZone
    {
        get => _interactionController._inAccessZone;
        set => _interactionController._inAccessZone = value;
    }
    public bool HasArrivedToMiddlePoint
    {
        get => _interactionController._hasArrivedToMiddlePoint;
        set => _interactionController._hasArrivedToMiddlePoint = value;
    }
    public INPC CurrentConversationTarget
    {
        get => _interactionController._currentConversationTarget;
        set => _interactionController._currentConversationTarget = value;
    }
    public INPC LastConversationTarget
    {
        get => _interactionController._lastConversationTarget;
        set => _interactionController._lastConversationTarget = value;
    }

    public GameObject CurrentConversationTargetGO
    {
        get => _currentConversationTargetGO;
        set => _currentConversationTargetGO = value;
    }
    public GameObject LastConversationTargetGO
    {
        get => _lastConversationTargetGO;
        set => _lastConversationTargetGO = value;
    }
    public float ConversationDuration
    {
        get => _conversationDuration;
        set => _conversationDuration = value;
    }

    public bool IsWaitingForAccess
    {
        get => _isWaitingForAccess;
        set => _isWaitingForAccess = value;
    }

    public float PlayerProximityRadius => _playerProximityRadius;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Conversation")]
    [Tooltip("Cooldown time between interactions with other NPCs")]
    [SerializeField] protected float _conversationCooldown = 0f;
    [SerializeField] protected INPC.RoleInConversation _conversationRole = INPC.RoleInConversation.None;
    [SerializeField] protected GameObject _currentConversationTargetGO;
    [SerializeField] protected GameObject _lastConversationTargetGO;
    [SerializeField, ReadOnly] protected float _conversationDuration = 0f;

    [Header("NavMeshAgent")]
    [Tooltip("Distance to which the agent will avoid other agents"), Range(0.5f, 2f)]
    [SerializeField] protected float _avoidanceRadius = 0.7f;
    [Tooltip("Max distance from the random point to a point on the navmesh, for target position sampling")]
    [SerializeField] protected float _maxSamplingDistance = 1f;
    [Tooltip("Base avoidance priority (0 = most important, 99 = least)")]
    [SerializeField] protected int _baseAvoidancePriority = 50;
    [Tooltip("Random +/- variance applied to base avoidance priority")]
    [SerializeField] protected int _avoidancePriorityVariance = 10;
    [Tooltip("Random +/- variance applied to walk speed")]
    [SerializeField] protected float _walkSpeedVariance = 0.5f;
    [SerializeField] protected Spot _destinationSpot;
    [SerializeField] protected float _playerProximityRadius = 2.5f;
    [Header("NPC References")]
    [SerializeField] protected House _home;
    [SerializeField] protected Workplace _workplace;
    [SerializeField] protected Sanctuary _sanctuary;
    [SerializeField] protected Market _market;
    [SerializeField] protected Stall _marketStall;
    [SerializeField] protected bool _isWaitingForAccess;
    #endregion

    #region INTERNAL PROPERTIES
    protected NPCMovementController _movementController;
    protected NPCInteractionController _interactionController;
    protected NavMeshAgent _agent;
    protected Collider _collider;
    protected CooldownDecorator _conversationCooldownNode;
    protected NPCPoolManager _poolManager;
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        _animationController = new(this, this, CharacterAnimator);
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _movementController = new(this);
        _interactionController = new(this, _agent, _interactionRange, _conversationCooldownNode);
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        _poolManager = ServiceLocator.Instance.Get<NPCPoolManager>();
    }

    protected override void Update()
    {
        base.Update();

        if (_destinationSpot != _movementController.DestinationSpot)
            _destinationSpot = _movementController.DestinationSpot;

        // Override agent stopped state if execution is paused
        if (_isExecutionPaused && _movementController.IsAgentValid && !_agent.isStopped)
            _agent.isStopped = true;
    }
    #endregion

    #region PUBLIC METHODS
    public void SetCharacterAndAgentActive(bool isActive)
    {
        if (_characterModel == null)
        {
            Debug.LogWarning($"[{gameObject.name}.SetCharacterActive()] Character Model reference is missing", gameObject);
            return;
        }

        if (_agent == null)
        {
            Debug.LogWarning($"[{gameObject.name}.SetCharacterActive()] NavMeshAgent component is missing", gameObject);
            return;
        }

        if (isActive)
        {
            Vector3 currentPosition = transform.position;

            if (!NavMesh.SamplePosition(currentPosition, out NavMeshHit hit, 0.15f, NavMesh.AllAreas))
            {
                if (NavMesh.SamplePosition(currentPosition, out hit, 2.0f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                }
                else
                {
                    OnFailedToActivate();
                    return;
                }
            }

            _agent.enabled = true;
            _collider.enabled = true;
        }
        else
        {
            _agent.enabled = false;
            _collider.enabled = false;
        }

        _characterModel.SetActive(isActive && _shouldRenderCharacterModel);
        _isOutdoors = isActive;
    }

    protected virtual void OnFailedToActivate()
    {
        _characterModel.SetActive(false);
        _isOutdoors = false;
    }
    #endregion
}