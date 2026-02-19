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

    #region INTERNAL PROPERTIES
    Vector3 _originalPosition;
    Quaternion _originalRotation;
    PlayableCharacterMovementController _movementController;

    [Header("Playable Character Settings")]
    [SerializeField] DataSO _characterData;
    [SerializeField] LayerMask _obstacleLayers;

    // State machine and states
    FiniteStateMachine<APlayableCharacterState> _fsm;
    NotControlled_PlayableCharacterState _notControlledState;
    Controlled_PlayableCharacterState _controlledState;
    AtPOI_PlayableCharacterState _atPOIState;

    // Dependency Injection
    TourManager _tourManager;
    UIManager _uiManager;
    CameraManager _cameraManager;
    ProgressManager _progressManager;
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
    // TODO: remove eventually
    // void OnEnable()
    // {
    //     SubscribeToRuntimeEvents();
    // }
    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (!Application.isPlaying)
    //             SubscribeToRuntimeEvents();
    // #endif
    //     }

    protected override void Awake()
    {
        base.Awake();

        if (_characterData == null)
            Debug.LogWarning($"[PlayableCharacter] No CharacterData assigned to {gameObject.name}. Please assign a DataSO with the character's information.", this);

        _animationController = new(this, this, CharacterAnimator);
        _movementController = new(this, GetComponent<CharacterController>());

        // Get dependencies from ServiceLocator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();

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
        //UnsubscribeFromRuntimeEvents();
        UnsubscribeFromServicesEvents();
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE HANDLING
    public void SwitchToNotControlledState() => _fsm.SwitchState(_notControlledState);
    public void SwitchToControlledState() => _fsm.SwitchState(_controlledState);
    public void SwitchToAtPOIState(PointOfInterest poi)
    {
        _atPOIState.CurrentPOI = poi;
        _fsm.SwitchState(_atPOIState);
    }
    #endregion

    #region EVENTS SUBSCRIPTION
    void SubscribeToServicesEvents()
    {
        _uiManager.PlayCharacterClickedEvent += OnPlayCharacterClicked;
        _uiManager.ContextualPanelHiddenEvent += OnContextualPanelHidden;
        _tourManager.POIVisitedEvent += OnTourPOIVisited;
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    }

    void UnsubscribeFromServicesEvents()
    {
        _uiManager.PlayCharacterClickedEvent -= OnPlayCharacterClicked;
        _uiManager.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _tourManager.POIVisitedEvent -= OnTourPOIVisited;
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
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

    void OnMilestoneChanged(Milestone_DataSO milestoneMapping)
    {
        Tour tour = ServiceLocator.Instance.Get<Tour>();
        // Reset position and rotation if tour is completed
        if (tour != null && tour.IsCompleted)
            GO.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
    }
    #endregion

    #region EDITOR UPDATES
    // TODO: remove eventually
    // void SubscribeToRuntimeEvents()
    // {
    //     _progressManager = FindAnyObjectByType<ProgressManager>();
    //     if (_progressManager != null)
    //     {
    //         _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    //         //_progressManager.OnEditorUpdateChangedEvent += OnEditorUpdateChanged;
    //     }
    // }
    // void UnsubscribeFromRuntimeEvents()
    // {
    //     _progressManager = FindAnyObjectByType<ProgressManager>();
    //     if (_progressManager != null)
    //     {
    //         _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    //         //_progressManager.OnEditorUpdateChangedEvent -= OnEditorUpdateChanged;
    //     }
    // }
    //     void OnEditorUpdateChanged(bool updateInEditor)
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;
    //         if (this == null) return;
    //         // Not updated through editor
    //         if (!updateInEditor)
    //             // All playable characters active
    //             GO.SetActive(true);
    //         else
    //         {
    //             // Only active if corresponding to current milestone
    //             if (_progressManager.CurrentMilestoneMapping.PlayableCharacter == this)
    //                 GO.SetActive(true);
    //             else
    //                 GO.SetActive(false);
    //         }
    // #endif
    //     }
    #endregion
}
