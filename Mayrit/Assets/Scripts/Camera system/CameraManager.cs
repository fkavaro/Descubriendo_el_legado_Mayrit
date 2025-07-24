using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    #region PUBLIC PROPERTIES
    public event Action<ACameraState> OnCameraStateChange;

    // Finite State Machine
    public FiniteStateMachine<CameraManager> _fsm;
    public Spectator_CameraState _spectatorState;
    public ThirdPerson_CameraState _thirdPersonState;

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

    [Header("Camera transition")]
    public float _SpectatorTo3rdPersonTransitionDuration = 1f;
    public float _3rdPersonToSpectatorTransitionDuration = 3f;
    #endregion

    #region PRIVATE PROPERTIES  
    #endregion

    #region INHERITED PROPERTIES
    protected override void OnAwake()
    {

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
            _moveSpeedZoomCurve,
            _selectableLayer);

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
        // Update spectator camera target to player position
        _spectatorCamera.LookAt.position = _thirdPersonCamera.LookAt.position;

        // Fix to position height
        Vector3 fixedTargetPos = new(
            _spectatorCamera.LookAt.position.x,
            _spectatorTargetHeight, // At spectator height
            _spectatorCamera.LookAt.position.z
        );

        // Update spectator camera target to player position
        //_spectatorCamera.LookAt.position = fixedTargetPos;

        // Long distance to the target to avoid weird behaviour
        _spectatorCamera.GetComponent<CinemachineOrbitalFollow>().RadialAxis.Value = 0.5f;

        _fsm.SwitchState(_spectatorState);

        // Move spectator camera target smoothly to fixed position
        SmoothMoveCoroutine(_spectatorCamera.LookAt, fixedTargetPos, _3rdPersonToSpectatorTransitionDuration,
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
        SmoothMoveCoroutine(_spectatorCamera.LookAt, _thirdPersonCamera.LookAt.position, _SpectatorTo3rdPersonTransitionDuration,
            () =>
            {
                // Switch state when coroutine finished
                _fsm.SwitchState(_thirdPersonState);
            }
        );
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
    #endregion

    #region PRIVATE METHODS
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
    #endregion
}
