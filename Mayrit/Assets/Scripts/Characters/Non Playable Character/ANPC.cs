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
    public bool IsInStreet
    {
        get => _isInStreet;
        set => _isInStreet = value;
    }

    public INPC.RoleInConversation ConversationRole
    {
        get => _conversationRole;
        set => _conversationRole = value;
    }

    public bool ShouldTalk
    {
        get => _shouldTalk;
        set => _shouldTalk = value;
    }

    public bool IsReadyToTalk
    {
        get => _isReadyToTalk;
        set => _isReadyToTalk = value;
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
    #endregion

    #region INTERNAL PROPERTIES
    public event Action ConversationFinishedEvent;

    NPCMovementController _movementController;
    NavMeshAgent _agent;
    bool _isInStreet = true;
    bool _shouldTalk = false;
    bool _isReadyToTalk = false;
    public INPC _currentConversationTarget, _lastConversationTarget;
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
        return _isInStreet && _shouldTalk && CharacterModel.activeInHierarchy;
    }

    public virtual bool IsStillInConversation(INPC otherNpc)
    {
        return IsAvailableForConversation()
            && _currentConversationTarget == otherNpc
            && Vector3.Distance(GO.transform.position, otherNpc.GO.transform.position) < 5f;
    }

    public virtual bool CanAcceptConversation(INPC initiator)
    {
        if (IsAvailableForConversation()
            && _conversationRole.Equals(INPC.RoleInConversation.None)
            && _lastConversationTarget != initiator)
        {
            _currentConversationTargetGO = initiator.GO;
            _currentConversationTarget = initiator;

            // This is the follower
            _conversationRole = INPC.RoleInConversation.Follower;

            return true;
        }
        else
            return false;
    }

    public virtual void Talk()
    {
        AnimationController.ChangeToTalk();
    }

    public virtual void EndConversation()
    {
        ConversationFinishedEvent?.Invoke();

        _isReadyToTalk = false;
        _lastConversationTarget = _currentConversationTarget;
        _lastConversationTargetGO = _currentConversationTargetGO;
        _currentConversationTarget = null;
        _currentConversationTargetGO = null;

        _conversationRole = INPC.RoleInConversation.None;
    }
    #endregion
}