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
    #endregion

    #region INHERITED
    public override FiniteStateMachine<APlayableCharacterState> InitializeBehaviourSystem()
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

        // Subscribe to events
        UIManager.Instance.PlayCharacterClickedEvent += SwitchToControlledState;
        UIManager.Instance.OnContextualPanelHiddenEvent += SwitchToControlledState;
        TourManager.Instance.TourPOIVisitedEvent += OnTourPOIVisited;
        CameraManager.Instance.ThirdPersonCameraExitedEvent += OnExitThirdPersonCamera;
        GameManager.Instance.GamePausedEvent += OnGamePaused;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        UIManager.ExistingInstance.PlayCharacterClickedEvent -= OnPlayCharacterClicked;
        UIManager.ExistingInstance.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
        TourManager.ExistingInstance.TourPOIVisitedEvent -= OnTourPOIVisited;
        CameraManager.ExistingInstance.ThirdPersonCameraExitedEvent -= OnExitThirdPersonCamera;
        GameManager.ExistingInstance.GamePausedEvent -= OnGamePaused;
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
        SwitchToControlledState();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        SwitchToAtPOIState(poi);
    }

    void OnExitThirdPersonCamera()
    {
        SwitchToNotControlledState();
    }

    void OnGamePaused(bool isGamePaused)
    {
        // Disable character controls when game is paused
        if (isGamePaused)
            GameManager.Instance.InputActions.Player.Disable();
        else if (IsBeingControlled)
            GameManager.Instance.InputActions.Player.Enable();
    }
    #endregion
}
