using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SelectableObject))]
public class PlayableCharacter : ACharacter<FiniteStateMachine<APlayableCharacterState>>
{
    #region PROPERTY HELPERS
    public bool IsBeingControlled => _fsm.IsCurrentState(_controlledState);
    #endregion

    #region INTERNAL PROPERTIES
    public PlayableCharacterMovementController _playerController;

    FiniteStateMachine<APlayableCharacterState> _fsm;
    NotControlled_PlayableCharacterState _notControlledState;
    Controlled_PlayableCharacterState _controlledState;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<APlayableCharacterState> InitializeBehaviourSystem()
    {
        _fsm = new(this);

        _notControlledState = new(this);
        _controlledState = new(this);

        _fsm.SetInitialState(_notControlledState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        AnimationController = new(this, this, CharacterAnimator);
        _playerController = new(this, GetComponent<CharacterController>());

        // Subscribe to events
        UIManager.Instance.PlayCharacterClickedEvent += SwitchToControlledState;
        UIManager.Instance.OnContextualPanelHiddenEvent += SwitchToControlledState;
        TourManager.Instance.TourPOIVisitedEvent += OnTourPOIVisited;
        CameraManager.Instance.ThirdPersonCameraExitedEvent += OnExitThirdPersonCamera;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        UIManager.ExistingInstance.PlayCharacterClickedEvent -= SwitchToControlledState;
        UIManager.ExistingInstance.OnContextualPanelHiddenEvent -= SwitchToControlledState;
        TourManager.ExistingInstance.TourPOIVisitedEvent -= OnTourPOIVisited;
        CameraManager.ExistingInstance.ThirdPersonCameraExitedEvent -= OnExitThirdPersonCamera;
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
    #endregion

    #region CALLBACK METHODS
    void OnTourPOIVisited(PointOfInterest interest)
    {
        SwitchToNotControlledState();
    }

    void OnExitThirdPersonCamera()
    {
        SwitchToNotControlledState();
    }
    #endregion
}
