using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CameraManager : ABehaviourEntity<FiniteStateMachine<ACameraState>>
{
    #region GETTERS
    public bool IsInSpectatorState => _fsm.IsCurrentState(_spectatorState);

    public bool IsInOrbitalState => _fsm.IsCurrentState(_orbitalState);

    public bool IsInThirdPersonState => _fsm.IsCurrentState(_thirdPersonState);

    public bool IsInPOIState => _fsm.IsCurrentState(_poiState);

    public PlayableCharacter PlayableCharacter => _playableCharacter;

    public CinemachineCamera SpectatorCamera => _spectatorCamera;
    public CinemachineCamera OrbitalCamera => _orbitalCamera;
    public CinemachineCamera ThirdPersonCamera => _thirdPersonCamera;
    #endregion

    #region EDITOR PROPERTIES
    [Space]
    [SerializeField] SpectatorCameraDataSO _spectatorCameraData;
    [SerializeField] CinemachineCamera _spectatorCamera;

    [Space]
    [SerializeField] OrbitalCameraDataSO _orbitalCameraData;
    [SerializeField] CinemachineCamera _orbitalCamera;

    [Space]
    [SerializeField] ThirdPersonCameraDataSO _thirdPersonCameraData;
    [SerializeField] CinemachineCamera _thirdPersonCamera;
    #endregion

    #region EVENTS
    /// <summary>Invoked whenever the camera state changes (spectator, orbital, third-person, POI).</summary>
    public event Action CameraStateChangedEvent;
    #endregion

    #region INTERNAL PROPERTIES
    // Finiste State Machine and states
    FiniteStateMachine<ACameraState> _fsm;
    Spectator_CameraState _spectatorState;
    ThirdPerson_CameraState _thirdPersonState;
    Orbital_CameraState _orbitalState;
    POI_CameraState _poiState;

    // Dependency Injection
    UIManager _uiManager;
    TourManager _tourManager;
    SoundManager _soundManager;
    PlayableCharacter _playableCharacter;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<ACameraState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        // States initialization
        _spectatorState = new(_spectatorCameraData, _spectatorCamera);
        _orbitalState = new(_orbitalCameraData, _orbitalCamera);
        _thirdPersonState = new(_thirdPersonCameraData, _thirdPersonCamera);
        _poiState = new(_thirdPersonCameraData.SimulationSpeed);

        _fsm.SetInitialState(_spectatorState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Instance.Register(this);
    }

    protected override void Start()
    {
        // Get dependencies from ServiceLocator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        // Subscribe to events
        _uiManager.EdgeScrollingToggledEvent += _spectatorCameraData.OnIsEdgeScrollingToggled;
        //_spectatorState.ObjectSelectedEvent += SwitchToOrbitalCamera;
        _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent += OnPlayCharacterClicked;
        _tourManager.POIVisitedEvent += OnTourPOIVisited;

        // Set camera target at min height
        CinemachineOrbitalFollow _orbitalFollow = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.Radius = _spectatorCameraData.movementLimitsY.y;

        if (_spectatorCamera.LookAt.position.y != _spectatorCameraData.movementLimitsY.x)
        {
            // Fix spectator target height
            _spectatorCamera.LookAt.position = new(
                _spectatorCamera.LookAt.position.x,
                _spectatorCameraData.movementLimitsY.x,
                _spectatorCamera.LookAt.position.z);
        }

        // Check edge scrolling initial state
        _spectatorCameraData.isEdgeScrolling = _uiManager.EdgeScrollingValueSet;

        base.Start();
    }

    void OnDisable()
    {
        // Unsubscribe from events
        //_spectatorState.ObjectSelectedEvent -= SwitchToOrbitalCamera;
        _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent -= OnPlayCharacterClicked;
        _tourManager.POIVisitedEvent -= OnTourPOIVisited;

        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE TRANSITIONS
    /// <summary>
    /// Switches to spectator camera mode. Handles transitions from third-person and orbital modes.
    /// </summary>
    public void SwitchToSpectatorCamera()
    {
        if (IsInThirdPersonState)
            TransitionFromThirdPersonToSpectator();
        else if (IsInOrbitalState)
            TransitionFromOrbitalToSpectator();
    }

    public void SwitchToOrbitalCamera(OrbitalStateSetting orbitalStateSetting)
    {
        _orbitalState.Setting = orbitalStateSetting;
        _soundManager.PlayCameraTransitionSFX();

        SyncOrbitalWithSpectator();
        _fsm.SwitchState(_orbitalState);

        if (DebugMode)
            Debug.Log($"Switched to orbital camera around '{orbitalStateSetting.Target.name}'.");

        CameraStateChangedEvent?.Invoke();
    }

    // TODO: remove later
    // /// <summary>
    // /// Switches to orbital camera mode around the specified object.
    // /// </summary>
    // /// <param name="selectedElement">The object to orbit around.</param>
    // public void SwitchToOrbitalCamera(SelectableObject selectedElement)
    // {
    //     //_orbitalState.SelectedObject = selectedElement;
    //     _soundManager.PlayCameraTransitionSFX();

    //     SyncOrbitalCameraWithSpectator();
    //     _fsm.SwitchState(_orbitalState);

    //     if (DebugMode)
    //         Debug.Log($"Switched to orbital camera around '{selectedElement.name}'.");

    //     CameraStateChangedEvent?.Invoke();
    // }

    /// <summary>
    /// Switches to third-person camera mode, following the playable character.
    /// </summary>
    public void SwitchToThirdPersonCamera()
    {
        _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

        if (_playableCharacter == null)
        {
            Debug.LogError("Cannot switch to third person camera: PlayableCharacter is null.");
            return;
        }

        _thirdPersonCamera.LookAt.position = _playableCharacter.transform.position;
        _fsm.SwitchState(_thirdPersonState);

        if (DebugMode)
            Debug.Log("Switched to third person camera.");

        CameraStateChangedEvent?.Invoke();
    }

    /// <summary>
    /// Switches to POI (Point of Interest) camera mode.
    /// </summary>
    /// <param name="camera">The POI camera to switch to.</param>
    public void SwitchToPoiCamera(CinemachineCamera camera)
    {
        _soundManager.PlayCameraTransitionSFX();
        _poiState.Camera = camera;
        _fsm.SwitchState(_poiState);
        CameraStateChangedEvent?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS - STATE TRANSITIONS
    void TransitionFromThirdPersonToSpectator()
    {
        _soundManager.PlayCameraTransitionSFX();
        _spectatorCamera.LookAt.position = _thirdPersonCamera.LookAt.position;
        Vector3 spectatorLookAt = GetFixedSpectatorLookAtPosition(_spectatorCamera.LookAt.position);
        SyncSpectatorWithThirdPerson();
        _fsm.SwitchState(_spectatorState);
        SmoothVerticalMovementCoroutine(_spectatorCamera.LookAt, spectatorLookAt, _spectatorCameraData.targetPositionFixSpeed);

        if (DebugMode)
            Debug.Log("Switched to spectator camera from third person.");

        CameraStateChangedEvent?.Invoke();
    }

    void TransitionFromOrbitalToSpectator()
    {
        _soundManager.PlayCameraTransitionSFX();
        _spectatorCamera.LookAt.position = _orbitalCamera.LookAt.position;
        SyncSpectatorWithOrbital();
        Vector3 spectatorLookAt = GetFixedSpectatorLookAtPosition(_spectatorCamera.LookAt.position);
        _spectatorCamera.LookAt.position = spectatorLookAt;
        _fsm.SwitchState(_spectatorState);

        if (DebugMode)
            Debug.Log("Switched to spectator camera from orbital.");

        CameraStateChangedEvent?.Invoke();
    }

    Vector3 GetFixedSpectatorLookAtPosition(Vector3 currentPosition)
    {
        return new(
            currentPosition.x,
            _spectatorCameraData.movementLimitsY.x,
            currentPosition.z
        );
    }

    void SyncSpectatorWithOrbital()
    {
        CinemachineOrbitalFollow spectatorOrbit = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.GetComponent<CinemachineOrbitalFollow>();

        spectatorOrbit.HorizontalAxis.Value = orbitalOrbit.HorizontalAxis.Value;
        spectatorOrbit.VerticalAxis.Value = orbitalOrbit.VerticalAxis.Value;
    }

    void SyncOrbitalWithSpectator()
    {
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow spectatorOrbit = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();

        orbitalOrbit.HorizontalAxis.Value = spectatorOrbit.HorizontalAxis.Value;
        orbitalOrbit.VerticalAxis.Value = spectatorOrbit.VerticalAxis.Value;
    }

    void SyncSpectatorWithThirdPerson()
    {
        CinemachineOrbitalFollow spectatorOrbit = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();

        if (spectatorOrbit == null || _thirdPersonCamera.LookAt == null)
        {
            Debug.LogWarning("Cannot sync spectator with third person: Missing CinemachineOrbitalFollow component or third person LookAt target.");
            return;
        }

        // Extract pitch/yaw from third-person target rotation and convert to signed degrees
        Vector3 eulerAngles = _thirdPersonCamera.LookAt.eulerAngles;
        float signedPitch = Mathf.DeltaAngle(0f, eulerAngles.x);
        float signedYaw = Mathf.DeltaAngle(0f, eulerAngles.y);

        spectatorOrbit.HorizontalAxis.Value = signedYaw;

        // Apply limits by snapping to the nearest bound if surpassed
        Vector2 limits = _spectatorCameraData.verticalAngleLimits;
        if (signedPitch < limits.x)
            spectatorOrbit.VerticalAxis.Value = limits.x;
        else if (signedPitch > limits.y)
            spectatorOrbit.VerticalAxis.Value = limits.y;
        else
            spectatorOrbit.VerticalAxis.Value = signedPitch;
    }
    #endregion

    #region SMOOTH MOVEMENT
    void SmoothVerticalMovementCoroutine(Transform lookAt, Vector3 newPosition, float speed, Action onComplete = null)
    {
        StartCoroutine(SmoothVerticalMovement(lookAt, newPosition, speed, onComplete));
    }

    IEnumerator SmoothVerticalMovement(Transform transform, Vector3 newPosition, float speed, Action onComplete)
    {
        if (transform == null)
            yield break;

        // Only interpolate vertical (Y) movement; horizontal (X, Z) is controlled by input
        float startHeight = transform.position.y;
        float endHeight = newPosition.y;
        float heightDifference = Mathf.Abs(endHeight - startHeight);

        if (heightDifference < 0.001f)
        {
            onComplete?.Invoke();
            yield break;
        }

        float totalTime = heightDifference / speed;
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / totalTime);

            // Interpolate only the vertical (Y) component; preserve current X and Z from input
            Vector3 currentPos = transform.position;
            currentPos.y = Mathf.Lerp(startHeight, endHeight, t);
            transform.position = currentPos;
            yield return null;
        }

        // Ensure final height is set correctly
        Vector3 finalPos = transform.position;
        finalPos.y = endHeight;
        transform.position = finalPos;

        onComplete?.Invoke();
    }
    #endregion

    #region EVENT CALLBACKS
    void OnPlayCharacterClicked() => SwitchToThirdPersonCamera();

    void OnContextualPanelHidden()
    {
        if (IsInOrbitalState)
            SwitchToSpectatorCamera();
        else if (IsInPOIState)
            SwitchToThirdPersonCamera();
    }

    void OnTourPOIVisited(PointOfInterest poi)
    {
        if (!poi.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"POI '{poi.name}' is not active in hierarchy.");
            return;
        }

        SwitchToPoiCamera(poi.Camera);
    }
    #endregion
}