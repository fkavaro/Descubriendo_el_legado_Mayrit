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
    #endregion

    #region EDITOR PROPERTIES
    [Space] public SpectatorCameraData _spectatorCamera;
    [Space] public OrbitalCameraData _orbitalCamera;
    [Space] public ThirdPersonCameraData _thirdPersonCamera;
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
        _spectatorState = new(_spectatorCamera);
        _orbitalState = new(_orbitalCamera);
        _thirdPersonState = new(_thirdPersonCamera);
        _poiState = new(_thirdPersonCamera.SimulationSpeed);

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
        _uiManager.EdgeScrollingToggledEvent += _spectatorCamera.OnIsEdgeScrollingToggled;
        _spectatorState.ObjectSelectedEvent += SwitchToOrbitalCamera;
        _thirdPersonState.ExitThirdPersonCameraEvent += OnExitThirdPersonCamera;
        _uiManager.OnContextualPanelHiddenEvent += OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent += OnPlayCharacterClicked;
        _tourManager.POIVisitedEvent += OnTourPOIVisited;

        // Set camera target at min height
        CinemachineOrbitalFollow _orbitalFollow = _spectatorCamera.Camera.GetComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.Radius = _spectatorCamera.movementLimitsY.y;

        if (_spectatorCamera.Camera.LookAt.position.y != _spectatorCamera.movementLimitsY.x)
        {
            // Fix spectator target height
            _spectatorCamera.Camera.LookAt.position = new(
                _spectatorCamera.Camera.LookAt.position.x,
                _spectatorCamera.movementLimitsY.x,
                _spectatorCamera.Camera.LookAt.position.z);
        }

        // Check edge scrolling initial state
        _spectatorCamera.isEdgeScrolling = _uiManager.EdgeScrollingValueSet;

        base.Start();
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _spectatorState.ObjectSelectedEvent -= SwitchToOrbitalCamera;
        _thirdPersonState.ExitThirdPersonCameraEvent -= OnExitThirdPersonCamera;
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

    /// <summary>
    /// Switches to orbital camera mode around the specified object.
    /// </summary>
    /// <param name="selectedElement">The object to orbit around.</param>
    public void SwitchToOrbitalCamera(SelectableObject selectedElement)
    {
        _orbitalState.SelectedObject = selectedElement;
        _soundManager.PlayCameraTransitionSFX();

        SyncOrbitalCameraWithSpectator();
        _fsm.SwitchState(_orbitalState);

        if (DebugMode)
            Debug.Log($"Switched to orbital camera around '{selectedElement.name}'.");

        CameraStateChangedEvent?.Invoke();
    }

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

        _thirdPersonCamera.Camera.LookAt.position = _playableCharacter.transform.position;
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
        _spectatorCamera.Camera.LookAt.position = _thirdPersonCamera.Camera.LookAt.position;
        Vector3 spectatorLookAt = GetFixedSpectatorLookAtPosition(_spectatorCamera.Camera.LookAt.position);
        _fsm.SwitchState(_spectatorState);
        SmoothVerticalMovementCoroutine(_spectatorCamera.Camera.LookAt, spectatorLookAt, _spectatorCamera.targetPositionFixSpeed);

        if (DebugMode)
            Debug.Log("Switched to spectator camera from third person.");

        CameraStateChangedEvent?.Invoke();
    }

    void TransitionFromOrbitalToSpectator()
    {
        _soundManager.PlayCameraTransitionSFX();
        _spectatorCamera.Camera.LookAt.position = _orbitalCamera.Camera.LookAt.position;
        SyncSpectatorCameraWithOrbital();
        Vector3 spectatorLookAt = GetFixedSpectatorLookAtPosition(_spectatorCamera.Camera.LookAt.position);
        _spectatorCamera.Camera.LookAt.position = spectatorLookAt;
        _fsm.SwitchState(_spectatorState);

        if (DebugMode)
            Debug.Log("Switched to spectator camera from orbital.");

        CameraStateChangedEvent?.Invoke();
    }

    Vector3 GetFixedSpectatorLookAtPosition(Vector3 currentPosition)
    {
        return new(
            currentPosition.x,
            _spectatorCamera.movementLimitsY.x,
            currentPosition.z
        );
    }

    void SyncSpectatorCameraWithOrbital()
    {
        CinemachineOrbitalFollow spectatorOrbit = _spectatorCamera.Camera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.Camera.GetComponent<CinemachineOrbitalFollow>();

        spectatorOrbit.HorizontalAxis.Value = orbitalOrbit.HorizontalAxis.Value;
        spectatorOrbit.VerticalAxis.Value = orbitalOrbit.VerticalAxis.Value;
    }

    void SyncOrbitalCameraWithSpectator()
    {
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.Camera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow spectatorOrbit = _spectatorCamera.Camera.GetComponent<CinemachineOrbitalFollow>();

        orbitalOrbit.HorizontalAxis.Value = spectatorOrbit.HorizontalAxis.Value;
        orbitalOrbit.VerticalAxis.Value = spectatorOrbit.VerticalAxis.Value;
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
    void OnExitThirdPersonCamera() => SwitchToSpectatorCamera();
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