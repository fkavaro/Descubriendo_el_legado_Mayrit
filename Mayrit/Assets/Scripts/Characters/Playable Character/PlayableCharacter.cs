using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SelectableObject))]
public class PlayableCharacter : ACharacter<FiniteStateMachine<APlayableCharacterState>>
{
    #region PROPERTY HELPERS
    public bool IsBeingControlled => _fsm.IsCurrentState(_controlledState);
    public PlayableCharacterMovementController MovementController => _movementController;
    #endregion

    #region INTERNAL PROPERTIES
    PlayableCharacterMovementController _movementController;

    // State machine and states
    FiniteStateMachine<APlayableCharacterState> _fsm;
    NotControlled_PlayableCharacterState _notControlledState;
    Controlled_PlayableCharacterState _controlledState;
    AtPOI_PlayableCharacterState _atPOIState;

    // Dependency Injection
    TourManager _tourManager;
    UIManager _uiManager;
    GameManager _gameManager;
    CameraManager _cameraManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<APlayableCharacterState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        _notControlledState = new(this);
        _controlledState = new(this);
        _atPOIState = new(this);

        _fsm.SetInitialState(_notControlledState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        AnimationController = new(this, this, CharacterAnimator);
        _movementController = new(this, GetComponent<CharacterController>());

        // Get dependencies from ServiceLocator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to events
        _uiManager.PlayCharacterClickedEvent += OnPlayCharacterClicked;
        _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
        _tourManager.TourPOIVisitedEvent += OnTourPOIVisited;
        _cameraManager.OnCameraStateChangedEvent += OnCameraStateChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _uiManager.PlayCharacterClickedEvent -= OnPlayCharacterClicked;
        _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _tourManager.TourPOIVisitedEvent -= OnTourPOIVisited;
        _cameraManager.OnCameraStateChangedEvent -= OnCameraStateChanged;
    }
    #endregion

    #region STATE HANDLING
    public void SwitchToNotControlledState()
    {
        _fsm.SwitchState(_notControlledState);
    }

    public void SwitchToControlledState()
    {
        _fsm.SwitchState(_controlledState);
    }

    public void SwitchToAtPOIState(PointOfInterest poi)
    {
        _atPOIState.CurrentPOI = poi;
        _fsm.SwitchState(_atPOIState);
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayCharacterClicked()
    {
        SwitchToControlledState();
    }

    void OnContextualPanelHidden()
    {
        if (!_cameraManager.IsInSpectatorState)
            SwitchToControlledState();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        SwitchToAtPOIState(poi);
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState)
            SwitchToControlledState();
        else
            SwitchToNotControlledState();
    }
    #endregion
}
