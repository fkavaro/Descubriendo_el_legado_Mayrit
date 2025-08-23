using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    #region PUBLIC PROPERTIES
    // Finite State Machine
    public FiniteStateMachine<CameraManager> _fsm;
    public Spectator_CameraState _spectatorState;
    public ThirdPerson_CameraState _thirdPersonState;
    public Orbital_CameraState _orbitalState;

    [Header("Spectator camera")]
    public CinemachineCamera _spectatorCamera;
    [Tooltip("Wether to move camera at screen margins or not.")]
    public bool _edgeScrolling = false;
    public int _edgeScrollingMargin = 30;
    public float _moveSpeed = 500f;
    public AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    public float _acceleration = 200f;
    public float _deceleration = 250f;
    public float _sprintSpeedMultiplier = 2f;
    [Tooltip("Camera limits in X axis (min, max)")]
    public Vector2 _movementLimitsX = new(-1000, 700);
    [Tooltip("Camera limits in Y axis (min: at which target will be positioned, max: max distance from target)")]
    public Vector2 _movementLimitsY = new(120, 400);
    [Tooltip("Camera limits in Z axis (min, max)")]
    public Vector2 _movementLimitsZ = new(-800, 800);
    [Tooltip("Mouse sensitivity for camera rotation.")]
    public float _spectatorCameraOrbitSpeed = 0.5f;
    public float _orbitSmoothing = 5f;
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float _zoomSpeed = 0.1f;
    public float _zoomSmoothing = 5f;
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask _selectableLayer;
    [Tooltip("Speed to move the spectator camera target when switching from third person camera.")]
    public float _spectatorTargetFixingSpeed = 40f;

    [Header("Orbital camera")]
    public CinemachineCamera _orbitalCamera;
    public float _orbitalBuildingOrbitSpeed = 30f;
    public float _orbitalBuildingZoom;
    public float _orbitalBuildingOffset = 20f;
    public float _orbitalCharacterOrbitSpeed = 15f;
    public float _orbitalCharacterZoom;
    public float _orbitalCharacterOffset = 10f;

    [Header("Third Person Camera")]
    public CinemachineCamera _thirdPersonCamera;
    public float _3rdPersonCameraOrbitSpeed = 3f,
        _bottomClamp = -30f,
        _topClamp = 40f;

    // [Header("Camera transitions")]
    // public float _3rdPersonTransitionDuration = 1f;
    // public float _orbitalMoveTransitionDuration = 1f;
    // public float _orbitalOffsetTransitionDuration = 1f;
    // public float _orbitalZoomTransitionSpeed = 1f;
    #endregion

    #region PRIVATE PROPERTIES  
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {
        // Singleton
        base.OnAwake();

        // Set camera target at min height
        CinemachineOrbitalFollow _orbitalFollow = _spectatorCamera.GetComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.Radius = _movementLimitsY.y;
    }

    protected override void OnStart()
    {
        if (_spectatorCamera.LookAt.position.y != _movementLimitsY.x)
        {
            // Fix spectator target height
            _spectatorCamera.LookAt.position = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x,
                _spectatorCamera.LookAt.position.z);
        }
    }

    protected override void OnUpdate()
    {

    }

    protected override ADecisionSystem<CameraManager> CreateDecisionSystem()
    {
        _fsm = new(this);

        _spectatorState = new(_fsm,
            _spectatorCamera);

        _orbitalState = new(_fsm,
            _orbitalCamera);

        _thirdPersonState = new(_fsm,
            _thirdPersonCamera);

        _fsm.SetInitialState(_spectatorState);

        return _fsm;
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Switches to the spectator camera and its target is moved smoothly above player position 
    /// </summary>
    public void SwitchToSpectatorCamera()
    {
        if (_debugMode) Debug.Log("Switching to spectator camera");

        if (_thirdPersonState.IsCurrentState())
        {
            _spectatorCamera.LookAt.position = _thirdPersonCamera.LookAt.position;

            // Fix to look at target height
            Vector3 fixedSpectatorLookAt = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x, // At spectator height
                _spectatorCamera.LookAt.position.z
            );

            _fsm.SwitchState(_spectatorState);

            SmoothMoveCoroutine(_spectatorCamera.LookAt, fixedSpectatorLookAt, _spectatorTargetFixingSpeed);
        }
        else if (_orbitalState.IsCurrentState())
        {
            _spectatorCamera.LookAt.position = _orbitalCamera.LookAt.position;

            // Same orbit values as orbital camera
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value =
                _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value;
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value =
                _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value;

            // Fix to look at target height
            Vector3 fixedSpectatorLookAt = new(
                _spectatorCamera.LookAt.position.x,
                _movementLimitsY.x, // At spectator height
                _spectatorCamera.LookAt.position.z
            );

            _spectatorCamera.LookAt.position = fixedSpectatorLookAt;

            _fsm.SwitchState(_spectatorState);
        }
    }

    public void SwitchToOrbitalCamera(Transform objectToOrbitAround, AInformationSO information)
    {
        if (_debugMode) Debug.Log("Switching to orbital camera");

        // Hide contextual panel
        UIManager.Instance._spectatorHUDState._contextualPanel.Hide();

        _orbitalState._information = information;
        _orbitalCamera.Follow = objectToOrbitAround;
        _orbitalCamera.LookAt = objectToOrbitAround;

        // Same orbit values as spectator camera
        _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value =
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value;
        _orbitalCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value =
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value;

        _fsm.SwitchState(_orbitalState);
    }

    /// <summary>
    /// Switches to the third person camera after spectator camera target is moved smoothly to player position    
    /// </summary>
    public void SwitchToThirdPersonCamera()
    {
        if (_debugMode) Debug.Log("Switching to third person camera");

        // Update third person camera target to current playable character
        PlayableCharacter playerTransform = GameManager.Instance.GetCurrentPlayableCharacter();

        // Set camera follow and look at targets
        _thirdPersonCamera.Follow = playerTransform._orientation;
        _thirdPersonCamera.LookAt = playerTransform._orientation;

        _fsm.SwitchState(_thirdPersonState);
    }

    /// <summary>
    /// Moves smoothly the given transform to the new position in given duration.
    /// </summary>
    public void SmoothMoveCoroutine(Transform lookAt, Vector3 newPosition, float speed, Action onComplete = null)
    {
        StartCoroutine(SmoothMove(lookAt, newPosition, speed, onComplete));
    }

    // public void ZoomToCoroutine(CinemachineOrbitalFollow orbitalFollow, float targetZoom, Action onComplete = null)
    // {
    //     StartCoroutine(ZoomTo(orbitalFollow, targetZoom, onComplete));
    // }

    // public void ApplyContextualPanelOffset(CinemachineCamera camera, float offset)
    // {
    //     StartCoroutine(SmoothHorizontalOffset(camera.GetComponent<CinemachineCameraOffset>(), offset));
    // }

    // public void ResetContextualPanelOffset(CinemachineCamera camera)
    // {
    //     StartCoroutine(SmoothHorizontalOffset(camera.GetComponent<CinemachineCameraOffset>(), 0));
    // }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Smoothly moves the given transform to the new position in given speed.
    /// </summary>
    IEnumerator SmoothMove(Transform transform, Vector3 newPosition, float speed, Action onComplete)
    {
        if (transform == null)
            yield break;

        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, newPosition);
        if (distance < 0.001f)
        {
            transform.position = newPosition;
            onComplete?.Invoke();
            yield break;
        }

        float moved = 0f;
        while (moved < distance)
        {
            float step = speed * Time.unscaledDeltaTime;
            moved += step;
            float t = Mathf.Clamp01(moved / distance);
            transform.position = Vector3.Lerp(startPosition, newPosition, t);
            yield return null;
        }

        transform.position = newPosition;
        onComplete?.Invoke();
    }

    // /// <summary>
    // /// Smoothly zooms the given CinemachineOrbitalFollow component to the target zoom value.
    // /// </summary>
    // IEnumerator ZoomTo(CinemachineOrbitalFollow orbitalFollow, float targetZoom, Action onComplete)
    // {
    //     float zoomSpeed = _orbitalZoomTransitionSpeed;
    //     float startZoom = orbitalFollow.RadialAxis.Value;
    //     float elapsed = 0f;
    //     float duration = Mathf.Abs(targetZoom - startZoom) / (zoomSpeed > 0 ? zoomSpeed : 1f);

    //     // If duration is very small, snap to target
    //     if (duration < 0.01f)
    //     {
    //         orbitalFollow.RadialAxis.Value = targetZoom;
    //         yield break;
    //     }

    //     while (elapsed < duration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = Mathf.Clamp01(elapsed / duration);
    //         orbitalFollow.RadialAxis.Value = Mathf.Lerp(startZoom, targetZoom, t);
    //         yield return null;
    //     }

    //     orbitalFollow.RadialAxis.Value = targetZoom;

    //     onComplete?.Invoke();
    // }

    // /// <summary>
    // /// Smoothly interpolates the camera's horizontal offset to the target value.
    // /// </summary>
    // IEnumerator SmoothHorizontalOffset(CinemachineCameraOffset offsetComponent, float targetOffset, Action onComplete = null)
    // {
    //     float startOffset = offsetComponent.Offset.x;
    //     float elapsed = 0f;

    //     while (elapsed < _orbitalOffsetTransitionDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = Mathf.Clamp01(elapsed / _orbitalOffsetTransitionDuration);
    //         offsetComponent.Offset.x = Mathf.Lerp(startOffset, targetOffset, t);
    //         yield return null;
    //     }

    //     offsetComponent.Offset.x = targetOffset;

    //     onComplete?.Invoke();
    // }
    #endregion
}
