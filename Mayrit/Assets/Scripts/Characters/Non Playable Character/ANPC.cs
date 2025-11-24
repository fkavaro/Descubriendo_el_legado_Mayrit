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
    public int AvoidancePriorityVariance => _avoidancePriorityVariance;
    public int BaseAvoidancePriority => _baseAvoidancePriority;
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
    public bool IsTalking
    {
        get => _isTalking;
        set => _isTalking = value;
    }
    public bool IsBeingTalkedTo
    {
        get => _isBeingTalkedTo;
        set => _isBeingTalkedTo = value;
    }
    public bool IsReadyToTalk
    {
        get => _isReadyToTalk;
        set => _isReadyToTalk = value;
    }
    public INPC CurrentInteractionTarget
    {
        get => _currentInteractionTarget;
        set => _currentInteractionTarget = value;
    }
    public INPC LastInteractionTarget
    {
        get => _lastInteractionTarget;
        set => _lastInteractionTarget = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    // Character information
    [SerializeField] protected string _givenName = "";
    [SerializeField] protected string _familyName = "";

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
    #endregion

    #region INTERNAL PROPERTIES
    NPCMovementController _movementController;
    NavMeshAgent _agent;
    bool _isInStreet = true;
    bool _isTalking = false;
    bool _isBeingTalkedTo = false;
    bool _isReadyToTalk = false;
    INPC _currentInteractionTarget, _lastInteractionTarget;
    #endregion

    #region MONOBEHAVIOUR
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

        _movementController.CheckNPCExecution();
    }
    #endregion

    #region INHERITED METHODS
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

    public virtual bool IsAvailableForConversation()
    {
        return _isInStreet && CharacterModel.activeInHierarchy;
    }

    public virtual bool CanAcceptConversation(INPC initiator)
    {
        if (IsAvailableForConversation() && !_isTalking && _lastInteractionTarget != initiator)
        {
            _currentInteractionTarget = initiator;
            _isBeingTalkedTo = true;
            return true;
        }
        else
            return false;
    }

    public virtual void StartConversation()
    {
        _isTalking = true;
        AnimationController.ChangeToTalk();
    }

    public virtual void EndConversation()
    {
        _isTalking = false;
        _isBeingTalkedTo = false;
        _isReadyToTalk = false;
        _lastInteractionTarget = CurrentInteractionTarget;
        _currentInteractionTarget = null;
    }
    #endregion
}