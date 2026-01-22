using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Manages camera states and transitions between spectator, orbital, third-person, and POI camera modes.
/// Uses a finite state machine to handle state logic and provides methods for switching between camera perspectives.
/// </summary>
public class CameraManager : ABehaviourEntity<FiniteStateMachine<ACameraState>>
{
    #region STATE PROPERTIES
    /// <summary>Gets whether the camera is currently in spectator mode.</summary>
    public bool IsInSpectatorState => _fsm.IsCurrentState(_spectatorState);

    /// <summary>Gets whether the camera is currently in orbital mode (orbiting around a selected object).</summary>
    public bool IsInOrbitalState => _fsm.IsCurrentState(_orbitalState);

    /// <summary>Gets whether the camera is currently in third-person mode (following the playable character).</summary>
    public bool IsInThirdPersonState => _fsm.IsCurrentState(_thirdPersonState);

    /// <summary>Gets whether the camera is currently in POI (Point of Interest) mode.</summary>
    public bool IsInPOIState => _fsm.IsCurrentState(_poiState);
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
    GameManager _gameManager;
    SoundManager _soundManager;
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
    protected override void Start()
    {
        base.Start();

        // Get dependencies from ServiceLocator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
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
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        _spectatorState.ObjectSelectedEvent -= SwitchToOrbitalCamera;
        _thirdPersonState.ExitThirdPersonCameraEvent -= OnExitThirdPersonCamera;
        _uiManager.OnContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _uiManager.PlayCharacterClickedEvent -= OnPlayCharacterClicked;
        _tourManager.POIVisitedEvent -= OnTourPOIVisited;
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
        if (_gameManager.PlayableCharacter == null)
        {
            Debug.LogError("Cannot switch to third person camera: PlayableCharacter is null.");
            return;
        }

        _thirdPersonCamera.Camera.LookAt.position = _gameManager.PlayableCharacter.transform.position;
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
        SmoothMoveCoroutine(_spectatorCamera.Camera.LookAt, spectatorLookAt, _spectatorCamera.targetPositionFixSpeed);

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
    void SmoothMoveCoroutine(Transform lookAt, Vector3 newPosition, float speed, Action onComplete = null)
    {
        StartCoroutine(SmoothMove(lookAt, newPosition, speed, onComplete));
    }

    IEnumerator SmoothMove(Transform transform, Vector3 newPosition, float speed, Action onComplete)
    {
        if (transform == null)
            yield break;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = newPosition;
        float distance = Vector3.Distance(startPosition, endPosition);

        if (distance < 0.001f)
        {
            onComplete?.Invoke();
            yield break;
        }

        float totalTime = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / totalTime);
            Vector3 currentPos = InterpolatePosition(transform.position, startPosition, endPosition, t);
            transform.position = currentPos;
            yield return null;
        }

        transform.position = endPosition;
        onComplete?.Invoke();
    }

    Vector3 InterpolatePosition(Vector3 currentPos, Vector3 startPos, Vector3 endPos, float t)
    {
        const float threshold = 0.001f;

        if (Mathf.Abs(endPos.x - startPos.x) > threshold)
            currentPos.x = Mathf.Lerp(startPos.x, endPos.x, t);
        if (Mathf.Abs(endPos.y - startPos.y) > threshold)
            currentPos.y = Mathf.Lerp(startPos.y, endPos.y, t);
        if (Mathf.Abs(endPos.z - startPos.z) > threshold)
            currentPos.z = Mathf.Lerp(startPos.z, endPos.z, t);

        return currentPos;
    }
    #endregion

    #region EVENT CALLBACKS
    /// <summary>Called when exiting third-person camera mode.</summary>
    void OnExitThirdPersonCamera() => SwitchToSpectatorCamera();

    /// <summary>Called when the player clicks on a playable character.</summary>
    void OnPlayCharacterClicked() => SwitchToThirdPersonCamera();

    /// <summary>Called when a contextual panel is hidden to reset camera if needed.</summary>
    void OnContextualPanelHidden()
    {
        if (IsInOrbitalState)
            SwitchToSpectatorCamera();
        else if (IsInPOIState)
            SwitchToThirdPersonCamera();
    }

    /// <summary>Called when a tour POI is visited.</summary>
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