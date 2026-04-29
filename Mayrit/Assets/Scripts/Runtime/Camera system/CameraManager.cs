using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CameraManager : ABehaviourEntity<FiniteStateMachine<ACameraState>>
{
    #region GETTERS
    public bool IsInAerialState => _fsm.IsCurrentState(_aerialState);

    public bool IsInOrbitalState => _fsm.IsCurrentState(_orbitalState);

    public bool IsInThirdPersonState => _fsm.IsCurrentState(_thirdPersonState);

    public bool IsInTourStopState => _fsm.IsCurrentState(_tourStopState);

    public PlayableCharacter PlayableCharacter => _playableCharacter;

    public CinemachineCamera AerialCamera => _aerialCamera;
    public CinemachineCamera OrbitalCamera => _orbitalCamera;
    public CinemachineCamera ThirdPersonCamera => _thirdPersonCamera;
    #endregion

    #region EDITOR PROPERTIES
    [Space]
    [SerializeField] AerialCameraDataSO _aerialCameraData;
    [SerializeField] CinemachineCamera _aerialCamera;

    [Space]
    [SerializeField] OrbitalCameraDataSO _orbitalCameraData;
    [SerializeField] CinemachineCamera _orbitalCamera;

    [Space]
    [SerializeField] ThirdPersonCameraDataSO _thirdPersonCameraData;
    [SerializeField] CinemachineCamera _thirdPersonCamera;
    #endregion

    #region EVENTS
    /// <summary>Invoked whenever the camera state changes (aerial, orbital, third-person, tour stop).</summary>
    public event Action CameraStateChangedEvent;
    #endregion

    #region INTERNAL PROPERTIES
    // Finiste State Machine and states
    FiniteStateMachine<ACameraState> _fsm;
    Aerial_CameraState _aerialState;
    ThirdPerson_CameraState _thirdPersonState;
    Orbital_CameraState _orbitalState;
    TourStop_CameraState _tourStopState;

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
        _aerialState = new(_aerialCameraData, _aerialCamera);
        _orbitalState = new(_orbitalCameraData, _orbitalCamera);
        _thirdPersonState = new(_thirdPersonCameraData, _thirdPersonCamera);
        _tourStopState = new(_thirdPersonCameraData.SimulationSpeed);

        _fsm.SetInitialState(_aerialState);

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
        _uiManager.EdgeScrollingToggledEvent += _aerialCameraData.OnIsEdgeScrollingToggled;
        _uiManager.ContextualPanelHiddenEvent += OnContextualPanelHidden;
        _uiManager.PlayTourClickedEvent += SwitchToThirdPersonCamera;
        _tourManager.TourStopVisitedEvent += OnTourStopVisited;

        // Set camera target at min height
        CinemachineOrbitalFollow _orbitalFollow = _aerialCamera.GetComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.Radius = _aerialCameraData.movementLimitsY.y;

        if (_aerialCamera.LookAt.position.y != _aerialCameraData.movementLimitsY.x)
        {
            // Fix aerial target height
            _aerialCamera.LookAt.position = new(
                _aerialCamera.LookAt.position.x,
                _aerialCameraData.movementLimitsY.x,
                _aerialCamera.LookAt.position.z);
        }

        // Check edge scrolling initial state
        _aerialCameraData.isEdgeScrolling = _uiManager.EdgeScrollingValueSet;

        base.Start();
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _uiManager.ContextualPanelHiddenEvent -= OnContextualPanelHidden;
        _uiManager.PlayTourClickedEvent -= SwitchToThirdPersonCamera;
        _tourManager.TourStopVisitedEvent -= OnTourStopVisited;

        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region STATE TRANSITIONS
    /// <summary>
    /// Switches to aerial camera mode. Handles transitions from third-person and orbital modes.
    /// </summary>
    public void SwitchToAerialCamera()
    {
        if (IsInThirdPersonState)
            TransitionFromThirdPersonToAerial();
        else if (IsInOrbitalState)
            TransitionFromOrbitalToAerial();

        if (_playableCharacter != null)
            _playableCharacter.PositionResetEvent -= SwitchToThirdPersonCamera;
    }

    public void SwitchToOrbitalCamera(OrbitalStateSetting orbitalStateSetting)
    {
        _orbitalState.Setting = orbitalStateSetting;
        _soundManager.PlayCameraTransitionSFX();

        SyncOrbitalWithAerial();
        _fsm.SwitchState(_orbitalState);

        if (DebugMode)
            Debug.Log($"Switched to orbital camera around '{orbitalStateSetting.Target.name}'.");

        CameraStateChangedEvent?.Invoke();

        if (orbitalStateSetting.IsForCharacter)
        {
            _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();
            _playableCharacter.PositionResetEvent += SwitchToThirdPersonCamera;
        }
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

    //     SyncOrbitalCameraWithAerial();
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

        if (IsInOrbitalState)
            SyncThirdPersonWithOrbital();
        else if (IsInTourStopState)
            SyncThirdPersonWithTourStop();

        _fsm.SwitchState(_thirdPersonState);

        if (DebugMode)
            Debug.Log("Switched to third person camera.");

        CameraStateChangedEvent?.Invoke();
    }

    /// <summary>
    /// Switches to TourStop camera mode.
    /// </summary>
    /// <param name="camera">The TourStop camera to switch to.</param>
    public void SwitchToTourStopCamera(CinemachineCamera camera)
    {
        _soundManager.PlayCameraTransitionSFX();
        _tourStopState.Camera = camera;
        _fsm.SwitchState(_tourStopState);
        CameraStateChangedEvent?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS - STATE TRANSITIONS
    void TransitionFromThirdPersonToAerial()
    {
        _soundManager.PlayCameraTransitionSFX();
        _aerialCamera.LookAt.position = _thirdPersonCamera.LookAt.position;
        Vector3 aerialLookAt = GetFixedAerialLookAtPosition(_aerialCamera.LookAt.position);
        SyncAerialWithThirdPerson();
        _fsm.SwitchState(_aerialState);
        SmoothVerticalMovementCoroutine(_aerialCamera.LookAt, aerialLookAt, _aerialCameraData.targetPositionFixSpeed);

        if (DebugMode)
            Debug.Log("Switched to aerial camera from third person.");

        CameraStateChangedEvent?.Invoke();
    }

    void TransitionFromOrbitalToAerial()
    {
        _soundManager.PlayCameraTransitionSFX();
        _aerialCamera.LookAt.position = _orbitalCamera.LookAt.position;
        SyncAerialWithOrbital();
        Vector3 aerialLookAt = GetFixedAerialLookAtPosition(_aerialCamera.LookAt.position);
        _aerialCamera.LookAt.position = aerialLookAt;
        _fsm.SwitchState(_aerialState);

        if (DebugMode)
            Debug.Log("Switched to aerial camera from orbital.");

        CameraStateChangedEvent?.Invoke();
    }

    Vector3 GetFixedAerialLookAtPosition(Vector3 currentPosition)
    {
        return new(
            currentPosition.x,
            _aerialCameraData.movementLimitsY.x,
            currentPosition.z
        );
    }

    void SyncAerialWithOrbital()
    {
        CinemachineOrbitalFollow aerialOrbit = _aerialCamera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.GetComponent<CinemachineOrbitalFollow>();

        aerialOrbit.HorizontalAxis.Value = orbitalOrbit.HorizontalAxis.Value;
        aerialOrbit.VerticalAxis.Value = orbitalOrbit.VerticalAxis.Value;
    }

    void SyncOrbitalWithAerial()
    {
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.GetComponent<CinemachineOrbitalFollow>();
        CinemachineOrbitalFollow aerialOrbit = _aerialCamera.GetComponent<CinemachineOrbitalFollow>();

        orbitalOrbit.HorizontalAxis.Value = aerialOrbit.HorizontalAxis.Value;
        orbitalOrbit.VerticalAxis.Value = aerialOrbit.VerticalAxis.Value;
    }

    void SyncAerialWithThirdPerson()
    {
        CinemachineOrbitalFollow aerialOrbit = _aerialCamera.GetComponent<CinemachineOrbitalFollow>();

        // Extract pitch/yaw from third-person target rotation and convert to signed degrees
        Vector3 eulerAngles = _thirdPersonCamera.LookAt.eulerAngles;
        float signedPitch = Mathf.DeltaAngle(0f, eulerAngles.x);
        float signedYaw = Mathf.DeltaAngle(0f, eulerAngles.y);

        aerialOrbit.HorizontalAxis.Value = signedYaw;

        // Apply limits by snapping to the nearest bound if surpassed
        Vector2 limits = _aerialCameraData.verticalAngleLimits;
        if (signedPitch < limits.x)
            aerialOrbit.VerticalAxis.Value = limits.x;
        else if (signedPitch > limits.y)
            aerialOrbit.VerticalAxis.Value = limits.y;
        else
            aerialOrbit.VerticalAxis.Value = signedPitch;
    }

    void SyncThirdPersonWithOrbital()
    {
        CinemachineOrbitalFollow orbitalOrbit = _orbitalCamera.GetComponent<CinemachineOrbitalFollow>();

        float yaw = Mathf.Repeat(orbitalOrbit.HorizontalAxis.Value, 360f);
        float pitch = Mathf.DeltaAngle(0f, orbitalOrbit.VerticalAxis.Value);

        Vector2 limits = _thirdPersonCameraData._orbitClamp;
        float clampedPitch = Mathf.Clamp(pitch, limits.x, limits.y);

        _thirdPersonState.SyncToRotation(clampedPitch, yaw);
    }

    void SyncThirdPersonWithTourStop()
    {
        Transform tourStopCameraTransform = _tourStopState.Camera.transform;
        Vector3 tourStopForward = tourStopCameraTransform.forward;

        Vector3 eulerAngles = Quaternion.LookRotation(tourStopForward, Vector3.up).eulerAngles;
        float pitch = Mathf.DeltaAngle(0f, eulerAngles.x);
        float yaw = Mathf.Repeat(eulerAngles.y, 360f);

        Vector2 limits = _thirdPersonCameraData._orbitClamp;
        float clampedPitch = Mathf.Clamp(pitch, limits.x, limits.y);

        _thirdPersonState.SyncToRotation(clampedPitch, yaw);
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
    void OnContextualPanelHidden()
    {
        if (IsInOrbitalState)
            SwitchToAerialCamera();
        else if (IsInTourStopState)
            SwitchToThirdPersonCamera();
    }

    void OnTourStopVisited(TourStop tourStop)
    {
        if (!tourStop.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"TourStop '{tourStop.name}' is not active in hierarchy.");
            return;
        }

        SwitchToTourStopCamera(tourStop.Camera);
    }
    #endregion
}