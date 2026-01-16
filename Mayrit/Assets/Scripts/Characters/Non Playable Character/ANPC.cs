using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for NPC (Non-Playable Character).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class ANPC<BehaviourSystemType> : ACharacter<BehaviourSystemType>, INPC
where BehaviourSystemType : ABehaviourSystem
{
    #region PROPERTIES HELPERS
    public NavMeshAgent Agent => _agent;
    public NPCMovementController MovementController => _movementController;
    public float AvoidanceRadius => _avoidanceRadius;
    public float MaxSamplingDistance => _maxSamplingDistance;
    public int BaseAvoidancePriority => _baseAvoidancePriority;
    public int AvoidancePriorityVariance => _avoidancePriorityVariance;
    public float WalkSpeedVariance => _walkSpeedVariance;
    public bool IsStopped
    {
        get => _isStopped;
        set => _isStopped = value;
    }
    public string GivenName => _givenName;
    public string FamilyName => _familyName;

    public INPC.RoleInConversation ConversationRole
    {
        get => _conversationRole;
        set => _conversationRole = value;
    }

    public bool NotInAccessZone
    {
        get => _notInAccessZone;
        set => _notInAccessZone = value;
    }

    public bool HasArrivedToMiddlePoint
    {
        get => _hasArrivedToMiddlePoint;
        set => _hasArrivedToMiddlePoint = value;
    }
    public INPC CurrentConversationTarget
    {
        get => _currentConversationTarget;
        set => _currentConversationTarget = value;
    }
    public INPC LastConversationTarget
    {
        get => _lastConversationTarget;
        set => _lastConversationTarget = value;
    }

    public float ConversationDuration
    {
        get => _conversationDuration;
        set => _conversationDuration = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    // Character information
    [SerializeField] protected string _givenName = "";
    [SerializeField] protected string _familyName = "";

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
    [SerializeField] protected bool _isStopped = false;
    [Tooltip("Base avoidance priority (0 = most important, 99 = least)")]
    [SerializeField] protected int _baseAvoidancePriority = 50;
    [Tooltip("Random +/- variance applied to base avoidance priority")]
    [SerializeField] protected int _avoidancePriorityVariance = 10;
    [Tooltip("Random +/- variance applied to walk speed")]
    [SerializeField] protected float _walkSpeedVariance = 0.5f;
    [SerializeField] protected Spot _destinationSpot;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action ConversationEndedEvent;

    NPCMovementController _movementController;
    NavMeshAgent _agent;
    bool _notInAccessZone = true;
    bool _hasArrivedToMiddlePoint = false;
    int _originalAvoidancePriority;
    protected INPC _currentConversationTarget, _lastConversationTarget;
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        AnimationController = new(this, this, CharacterAnimator);
        _agent = GetComponent<NavMeshAgent>();
        _movementController = new(this);
    }

    protected override void Update()
    {
        base.Update();

        _destinationSpot = _movementController.DestinationSpot;
        _movementController.CheckBehaviourExecution();
    }
    #endregion

    #region CHARACTER INFORMATION METHODS
    public void SetFullName(string given, string family)
    {
        _givenName = given;
        _familyName = family;
        try
        {
            gameObject.name = string.IsNullOrEmpty(_familyName) ?
                _givenName :
                $"{_givenName} {_familyName}";
        }
        catch { }
    }
    #endregion

    #region CONVERSATION METHODS
    public virtual bool IsAvailableForConversation()
    {
        return _notInAccessZone && CharacterModel.activeInHierarchy;
    }

    public virtual bool IsTalking()
    {
        return IsAvailableForConversation()
            && (_currentConversationTarget != null || _conversationRole != INPC.RoleInConversation.None);
    }

    public virtual bool IsStillInConversationWith(INPC otherNpc)
    {
        if (!IsTalking())
        {
            if (DebugMode)
                Debug.Log($"[{Name}.IsStillInConversationWith()] is not talking: {_notInAccessZone}, {CharacterModel.activeInHierarchy}, {_currentConversationTarget != null}, {_conversationRole != INPC.RoleInConversation.None}", GO);
            return false;
        }

        if (_currentConversationTarget != otherNpc)
        {
            if (DebugMode)
                Debug.Log($"[{Name}.IsStillInConversationWith()] current conversation target is not {otherNpc.Name}", GO);
            return false;
        }

        // Use generous distance while moving to middle point (2x interaction range)
        // Tighten once both arrive (1x interaction range)
        float maxDistance = (_hasArrivedToMiddlePoint && otherNpc.HasArrivedToMiddlePoint)
            ? _interactionRange
            : _interactionRange * 2f;

        if (Vector3.Distance(GO.transform.position, otherNpc.GO.transform.position) < maxDistance)
            return true;
        else
        {
            if (DebugMode)
                Debug.Log($"[{Name}.IsStillInConversationWith()] too far from {otherNpc.Name} to continue conversation as {_conversationRole}", GO);

            return false;
        }
    }

    public virtual bool IsFollowingConversation()
    {
        return IsTalking() && _conversationRole == INPC.RoleInConversation.Follower;
    }

    public virtual bool TryInitiateConversationWith(INPC target)
    {
        // False if target is null or already talking, or if self is already talking
        if (target == null || target.IsTalking() || IsTalking())
            return false;

        // Set initiator role BEFORE attempting acceptance (handshake)
        _conversationRole = INPC.RoleInConversation.Initiator;

        // Verify the other NPC accepts the conversation with this as initiator
        if (!target.CanAcceptNewConversationFrom(this))
        {
            // Reset state on failure
            _conversationRole = INPC.RoleInConversation.None;
            _currentConversationTarget = null;
            _currentConversationTargetGO = null;
            return false;
        }

        // Assign current conversation target
        _currentConversationTarget = target;
        _currentConversationTargetGO = target.GO;

        if (DebugMode)
            Debug.Log($"[{Name}.TryInitiateConversation()] successfully engaged in conversation with {target.Name}", GO);

        return true;
    }

    public virtual bool CanAcceptNewConversationFrom(INPC initiator)
    {
        // Verify initiator has claimed the Initiator role (handshake verification)
        if (!initiator.ConversationRole.Equals(INPC.RoleInConversation.Initiator))
        {
            if (DebugMode)
                Debug.LogWarning($"[{Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because they are not an Initiator", GO);
            return false;
        }

        // Reject if not available for conversation (in access zone or model inactive)
        if (!IsAvailableForConversation())
        {
            // if (DebugMode)
            //     Debug.LogWarning($"[{Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because not available", GO);
            return false;
        }

        // Reject if already talking with someone else or the same as last time
        if (IsTalking() || _lastConversationTarget == initiator)
        {
            // if (DebugMode)
            //     Debug.LogWarning($"[{Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because already talked recently", GO);
            return false;
        }

        // Assign follower role and initiatort as current conversation target
        _conversationRole = INPC.RoleInConversation.Follower;
        _currentConversationTargetGO = initiator.GO;
        _currentConversationTarget = initiator;

        return true;
    }

    public virtual void Talk()
    {
        // Save original priority and set to minimum (0 = most important) so no other agent can push
        if (_agent != null)
        {
            _originalAvoidancePriority = _agent.avoidancePriority;
            _agent.avoidancePriority = 0;  // Highest priority - won't be pushed by others
        }

        MovementController.SetIfStopped(true);
        AnimationController.ChangeToTalk();
    }

    public virtual void EndConversationAsInitiator()
    {
        if (_conversationRole != INPC.RoleInConversation.Initiator)
        {
            if (DebugMode)
                Debug.LogWarning($"[{Name}.EndConversation()] trying to end conversation but not as Initiator", GO);
            return;
        }

        ConversationEndedEvent?.Invoke();
        ConversationSucceeded();
    }

    public virtual void ConversationSucceeded()
    {
        UpdateConversationState(_currentConversationTarget, _currentConversationTargetGO);
    }

    public virtual void ConversationInterrupted()
    {
        UpdateConversationState(null, null);
    }

    public virtual void UpdateConversationState(INPC otherNpc, GameObject otherNpcGO)
    {
        // Restore original avoidance priority
        if (_agent != null)
            _agent.avoidancePriority = _originalAvoidancePriority;

        _lastConversationTarget = otherNpc;
        _lastConversationTargetGO = otherNpcGO;
        _currentConversationTarget = null;
        _currentConversationTargetGO = null;
        _hasArrivedToMiddlePoint = false;
        _conversationDuration = 0f;
        _conversationRole = INPC.RoleInConversation.None;
    }
    #endregion
}