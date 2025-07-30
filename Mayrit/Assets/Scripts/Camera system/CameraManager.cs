using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    #region PUBLIC PROPERTIES
    public event Action<ACameraState> OnCameraStateChange;
    public CameraController _cameraController;
    public SelectorCamera _selectorCamera;

    // Finite State Machine
    public FiniteStateMachine<CameraManager> _fsm;
    public Spectator_CameraState _spectatorState;
    public ThirdPerson_CameraState _thirdPersonState;
    public Orbital_CameraState _orbitalState;

    [Header("Spectator camera")]
    public CinemachineCamera _spectatorCamera;
    public int _spectatorTargetHeight = 120;

    [Space]
    [Tooltip("Wether to move camera at screen margins or not.")]
    public bool _edgeScrolling = false;
    public int _edgeScrollingMargin = 30;

    [Space]
    public float _moveSpeed = 500f;
    public AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    public float _acceleration = 200f;
    public float _deceleration = 250f;
    public float _printSpeedMultiplier = 2f;
    [Tooltip("Maximum allowed X, Y, Z positions (positive and negative) for the camera.")]
    public Vector3 _movementLimits = new(800, 0, 800);

    [Space]
    [Tooltip("Mouse sensitivity for camera rotation.")]
    public float _orbitSensitivity = 0.5f;
    public float _orbitSmoothing = 5f;

    [Space]
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float _zoomSpeed = 0.1f;
    public float _zoomSmoothing = 5f;

    [Space]
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask _selectableLayer;

    [Header("Third Person Camera")]
    public CinemachineCamera _thirdPersonCamera;

    [Header("Orbital camera")]
    public float _orbitSpeed = 30f;
    public float _orbitalCameraZoomValue = 0.2f;
    public float _orbitalTransitionSpeed = 1f;
    public float _horizontalOffset = -10f;

    [Header("Camera transition")]
    public float _3rdPersonTransitionDuration = 1f;
    public float _spectatorTransitionDuration = 3f;
    public float _offsetTransitionDuration = 1f;
    #endregion

    #region PRIVATE PROPERTIES  
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {
        // Singleton
        base.OnAwake();

        _cameraController = new(_spectatorCamera, _moveSpeedZoomCurve);
        _selectorCamera = new(_selectableLayer);
    }

    protected override void OnStart()
    {
        if (_spectatorCamera.LookAt.position.y != _spectatorTargetHeight)
        {
            // Fix spectator target height
            _spectatorCamera.LookAt.position = new(
                _spectatorCamera.LookAt.position.x,
                _spectatorTargetHeight,
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
            _spectatorCamera,
            _cameraController,
            _selectorCamera);

        _thirdPersonState = new(_fsm,
            _thirdPersonCamera);

        _orbitalState = new(_fsm,
        _spectatorCamera);

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
        // Update spectator camera target to player position
        if (_fsm.IsCurrentState(_thirdPersonState))
            _spectatorCamera.LookAt.position = _thirdPersonCamera.LookAt.position;

        // Fix to position height
        Vector3 fixedTargetPos = new(
            _spectatorCamera.LookAt.position.x,
            _spectatorTargetHeight, // At spectator height
            _spectatorCamera.LookAt.position.z
        );

        // Long distance to the target to avoid weird behaviour
        if (_fsm.IsCurrentState(_thirdPersonState))
            // Directly
            _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().RadialAxis.Value = 0.5f;
        else if (_fsm.IsCurrentState(_orbitalState))
        {
            // Transitions
            ZoomToCoroutine(_spectatorCamera.GetComponent<CinemachineOrbitalFollow>(), 0.5f);
            //ResetContextualPanelOffset();
        }

        _fsm.SwitchState(_spectatorState);

        // Move spectator camera target smoothly to fixed position
        SmoothMoveCoroutine(_spectatorCamera.LookAt, fixedTargetPos, _spectatorTransitionDuration,
            () =>
            {
                OnCameraStateChange?.Invoke(_spectatorState);
            }
        );
    }

    /// <summary>
    /// Switches to the third person camera after spectator camera target is moved smoothly to player position    
    /// </summary>
    public void SwitchToThirdPersonCamera()
    {
        OnCameraStateChange?.Invoke(_thirdPersonState);

        // Move spectator camera target smoothly to third person camera target
        SmoothMoveCoroutine(_spectatorCamera.LookAt, _thirdPersonCamera.LookAt.position, _3rdPersonTransitionDuration,
            () =>
            {
                // Switch state when coroutine finished
                _fsm.SwitchState(_thirdPersonState);
            }
        );
    }

    public void SwitchToOrbitalCamera(Transform objectToOrbitAround)
    {
        ApplyContextualPanelOffset();

        // Move spectator target to object position
        SmoothMoveCoroutine(_spectatorCamera.LookAt, objectToOrbitAround.position, _3rdPersonTransitionDuration,
        () =>
        {
            // When reached, switch state
            _fsm.SwitchState(_orbitalState);
        });
    }

    public void ToggleCameraState()
    {
        if (_fsm.IsCurrentState(_spectatorState))
            SwitchToThirdPersonCamera();
        else if (_fsm.IsCurrentState(_thirdPersonState))
            SwitchToSpectatorCamera();
    }

    /// <summary>
    /// Moves smoothly the given transform to the new position in given duration.
    /// </summary>
    public void SmoothMoveCoroutine(Transform lookAt, Vector3 newPosition, float duration = 1f, Action onComplete = null)
    {
        StartCoroutine(SmoothMove(lookAt, newPosition, duration, onComplete));
    }

    public void ZoomToCoroutine(CinemachineOrbitalFollow orbitalFollow, float targetZoom, Action onComplete = null)
    {
        StartCoroutine(ZoomTo(orbitalFollow, targetZoom, onComplete));
    }

    public void ApplyContextualPanelOffset()
    {
        StartCoroutine(SmoothHorizontalOffset(_spectatorCamera.GetComponent<CinemachineCameraOffset>(), _horizontalOffset));
    }

    public void ResetContextualPanelOffset()
    {
        StartCoroutine(SmoothHorizontalOffset(_spectatorCamera.GetComponent<CinemachineCameraOffset>(), 0));
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Smoothly moves the given transform to the new position in given duration.
    /// </summary>
    IEnumerator SmoothMove(Transform transform, Vector3 newPosition, float duration, Action onComplete)
    {
        if (transform == null || newPosition == null)
            yield break;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        // Duration remaining
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, newPosition, t);
            yield return null;
        }

        transform.position = newPosition;

        onComplete?.Invoke();
    }

    /// <summary>
    /// Smoothly zooms the given CinemachineOrbitalFollow component to the target zoom value.
    /// </summary>
    IEnumerator ZoomTo(CinemachineOrbitalFollow orbitalFollow, float targetZoom, Action onComplete)
    {
        float zoomSpeed = _orbitalTransitionSpeed;
        float startZoom = orbitalFollow.RadialAxis.Value;
        float elapsed = 0f;
        float duration = Mathf.Abs(targetZoom - startZoom) / (zoomSpeed > 0 ? zoomSpeed : 1f);

        // If duration is very small, snap to target
        if (duration < 0.01f)
        {
            orbitalFollow.RadialAxis.Value = targetZoom;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            orbitalFollow.RadialAxis.Value = Mathf.Lerp(startZoom, targetZoom, t);
            yield return null;
        }

        orbitalFollow.RadialAxis.Value = targetZoom;

        onComplete?.Invoke();
    }

    /// <summary>
    /// Smoothly interpolates the camera's horizontal offset to the target value.
    /// </summary>
    IEnumerator SmoothHorizontalOffset(CinemachineCameraOffset offsetComponent, float targetOffset, float duration = 1f, Action onComplete = null)
    {
        float startOffset = offsetComponent.Offset.x;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            offsetComponent.Offset.x = Mathf.Lerp(startOffset, targetOffset, t);
            yield return null;
        }

        offsetComponent.Offset.x = targetOffset;

        onComplete?.Invoke();
    }
    #endregion
}
