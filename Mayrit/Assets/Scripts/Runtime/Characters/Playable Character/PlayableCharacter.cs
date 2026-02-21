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
    public event Action PositionResetEvent;

    Vector3 _originalPosition;
    Quaternion _originalRotation;
    PlayableCharacterMovementController _movementController;

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
        _uiManager.PlayTourClickedEvent += OnPlayTourClicked;
        _uiManager.ResetTourClickedEvent += OnResetTourClicked;
        _uiManager.ContextualPanelHiddenEvent += OnContextualPanelHidden;
        _tourManager.POIVisitedEvent += OnTourPOIVisited;
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        //_progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    }

    void UnsubscribeFromServicesEvents()
    {
        _uiManager.PlayTourClickedEvent -= OnPlayTourClicked;
        _uiManager.ResetTourClickedEvent -= OnResetTourClicked;
        _uiManager.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _tourManager.POIVisitedEvent -= OnTourPOIVisited;
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        //_progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    }
    #endregion

    #region PUBLIC METHODS
    public void LocateAt(Transform transform)
    {
        Vector3 fixedPosition = transform.position + Vector3.up * 2f;
        GO.transform.SetPositionAndRotation(fixedPosition, transform.rotation);
    }
    #endregion

    #region PRIVATE METHODS
    void ResetPositionAndRotation()
    {
        GO.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayTourClicked()
    {
        SwitchToControlledState();
    }

    void OnResetTourClicked()
    {
        SwitchToControlledState();
        ResetPositionAndRotation();
        PositionResetEvent?.Invoke();
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

    // void OnMilestoneChanged(Milestone_DataSO milestoneData)
    // {
    //     int milestoneIndex = milestoneData.MilestoneIndex;
    //     int highestCompleted = _progressManager.HighestCompletedMilestoneIndex;

    //     if (milestoneIndex < highestCompleted)
    //     {
    //         //if (DebugMode)
    //         Debug.Log($"[PlayableCharacter] Milestone changed to index {milestoneIndex}, which is below the highest completed milestone index {highestCompleted}. Resetting position and rotation to reflect progress.");

    //         ResetPositionAndRotation();
    //     }
    // }
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
