using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacter : ACharacter<FiniteStateMachine<APlayableCharacterState>>
{
    #region PROPERTY HELPERS
    public bool IsBeingControlled => _fsm.IsCurrentState(_controlledState);
    public PlayableCharacterMovementController MovementController => _movementController;
    public DataSO CharacterData => _characterData;
    public LayerMask ObstacleLayers => _obstacleLayers;
    #endregion

    #region EDITOR FIELDS
    [Header("Playable Character Settings")]
    [SerializeField] DataSO _characterData;
    [SerializeField] LayerMask _obstacleLayers;
    #endregion

    #region INTERNAL PROPERTIES
    Vector3 _originalPosition;
    Quaternion _originalRotation;
    PlayableCharacterMovementController _movementController;

    // State machine and states
    FiniteStateMachine<APlayableCharacterState> _fsm;
    NotControlled_PlayableCharacterState _notControlledState;
    Controlled_PlayableCharacterState _controlledState;
    AtTourStop_PlayableCharacterState _atTourStopState;

    // Dependency Injection
    GameManager _gameManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<APlayableCharacterState> DefineBehaviourSystem()
    {
        _fsm = new(this);

        _notControlledState = new(this);
        _controlledState = new(this);
        _atTourStopState = new(this);

        _fsm.SetInitialState(_notControlledState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        if (_characterData == null)
            Debug.LogWarning($"[PlayableCharacter] No CharacterData assigned to {gameObject.name}. Please assign a DataSO with the character's information.", this);

        _animationController = new(this, this, CharacterAnimator);
        _movementController = new(this, GetComponent<CharacterController>());

        // Get dependencies from ServiceLocator
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        ServiceLocator.Instance.Register(this);
    }

    protected override void Start()
    {
        SubscribeToServicesEvents();

        base.Start();
    }

    void OnDisable()
    {
        UnsubscribeFromServicesEvents();
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE HANDLING
    public void SwitchToNotControlledState() => _fsm.SwitchState(_notControlledState);
    public void SwitchToControlledState() => _fsm.SwitchState(_controlledState);
    public void SwitchToAtTourStopState(TourStop tourStop)
    {
        _atTourStopState.CurrentTourStop = tourStop;
        _fsm.SwitchState(_atTourStopState);
    }
    #endregion

    #region EVENTS SUBSCRIPTION
    void SubscribeToServicesEvents()
    {
        _gameManager.TourAndPlayerResetEvent += ResetPositionAndRotation;
    }

    void UnsubscribeFromServicesEvents()
    {
        _gameManager.TourAndPlayerResetEvent -= ResetPositionAndRotation;
    }
    #endregion

    #region PUBLIC METHODS
    public void LocateAt(Transform transform)
    {
        Vector3 fixedPosition = transform.position + Vector3.up * 2f;
        GO.transform.SetPositionAndRotation(fixedPosition, transform.rotation);
    }
    #endregion

    #region CALLBACK METHODS
    void ResetPositionAndRotation()
    {
        GO.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
    }
    #endregion
}
