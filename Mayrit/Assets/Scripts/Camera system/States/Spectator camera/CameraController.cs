using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
public class CameraController
{
    #region PUBLIC PROPERTIES
    public readonly Transform _camera, _cameraTarget;
    public readonly CinemachineOrbitalFollow _orbitalFollow;
    public readonly AnimationCurve _moveSpeedZoomCurve;

    // Edge scrolling
    public bool _edgeScrolling;
    public int _edgeScrollingMargin;

    // Movement
    public float _moveSpeed,
        _acceleration,
        _deceleration,
        _printSpeedMultiplier = 2f;
    public Vector3 _movementLimits;

    // Orbit
    public float _orbitSensitivity,
        _orbitSmoothing;

    // Zoom
    public float _zoomSpeed,
        _zoomSmoothing;
    public float ZoomLevel // value between 0 (zoomed in) and 1 (zoomed out)
    {
        get
        {
            InputAxis axis = _orbitalFollow.RadialAxis;

            return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
        }
    }
    #endregion

    #region PRIVATE PROPERTIES
    Vector2 _edgeScrollInput;
    float _decelerationMultiplier = 1f,
        _currentZoomSpeed = 0f;
    Vector3 _velocity = Vector3.zero;

    // Input
    Vector2 _moveInput, _lookInput, _scrollInput;
    bool _sprintPressed, _middleClickPressed;
    #endregion


    public CameraController(Transform camera,
        Transform cameraTarget,
        CinemachineOrbitalFollow orbitalFollow,
        AnimationCurve moveSpeedZoomCurve)
    {
        _camera = camera;
        _cameraTarget = cameraTarget;
        _orbitalFollow = orbitalFollow;
        _moveSpeedZoomCurve = moveSpeedZoomCurve;
    }


    #region MONOBEHAVIOUR

    public void Start()
    {

    }

    public void Update()
    {
        _edgeScrolling = CameraManager.Instance._edgeScrolling;
        _edgeScrollingMargin = CameraManager.Instance._edgeScrollingMargin;
        _moveSpeed = CameraManager.Instance._moveSpeed;
        _acceleration = CameraManager.Instance._acceleration;
        _deceleration = CameraManager.Instance._deceleration;
        _printSpeedMultiplier = CameraManager.Instance._printSpeedMultiplier;
        _movementLimits = CameraManager.Instance._movementLimits;
        _orbitSensitivity = CameraManager.Instance._orbitSensitivity;
        _orbitSmoothing = CameraManager.Instance._orbitSmoothing;
        _zoomSpeed = CameraManager.Instance._zoomSpeed;
        _zoomSmoothing = CameraManager.Instance._zoomSmoothing;

        // TODO: LateUpdateState
        {
            _moveInput = GameManager.Instance._inputActions.Camera.Move.ReadValue<Vector2>();
            _sprintPressed = GameManager.Instance._inputActions.Camera.Sprint.IsPressed();
            _lookInput = GameManager.Instance._inputActions.Camera.Look.ReadValue<Vector2>();
            _middleClickPressed = GameManager.Instance._inputActions.Camera.Rotate.IsPressed();
            _scrollInput = GameManager.Instance._inputActions.Camera.Zoom.ReadValue<Vector2>();

            if (_edgeScrolling)
                UpdateEdgeScrolling();
            UpdateMovement();
            UpdateZoom();
            UpdateOrbit();

            ClampPosition();
        }
    }
    #endregion

    #region METHODS
    void UpdateEdgeScrolling()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        _edgeScrollInput = Vector2.zero;

        if (mousePosition.x <= _edgeScrollingMargin)
            _edgeScrollInput.x = -1f;
        else if (mousePosition.x >= Screen.width - _edgeScrollingMargin)
            _edgeScrollInput.x = 1f;

        if (mousePosition.y <= _edgeScrollingMargin)
            _edgeScrollInput.y = -1f;
        else if (mousePosition.y >= Screen.height - _edgeScrollingMargin)
            _edgeScrollInput.y = 1f;
    }

    /// <summary>
    /// Moves the camera in its local XZ plane, but keeps its Y (height) unchanged.
    /// </summary>
    private void UpdateMovement()
    {
        Vector3 forward = _camera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = _camera.right;
        right.y = 0f;
        right.Normalize();

        Vector3 inputVector = new Vector3(_moveInput.x + _edgeScrollInput.x, 0,
            _moveInput.y + _edgeScrollInput.y);
        inputVector.Normalize();

        float zoomMultiplier = _moveSpeedZoomCurve.Evaluate(ZoomLevel);

        Vector3 targetVelocity = inputVector * _moveSpeed * zoomMultiplier;

        float sprintFactor = 1f;
        if (_sprintPressed)
        {
            targetVelocity *= _printSpeedMultiplier;
            sprintFactor = _printSpeedMultiplier;
        }

        if (inputVector.sqrMagnitude > 0.01f)
        {
            _velocity = Vector3.MoveTowards(_velocity, targetVelocity, _acceleration * sprintFactor * Time.unscaledDeltaTime);

            if (_sprintPressed)
                _decelerationMultiplier = _printSpeedMultiplier;
        }
        else
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _deceleration * _decelerationMultiplier * Time.unscaledDeltaTime);

        Vector3 motion = _velocity * Time.unscaledDeltaTime;

        _cameraTarget.position += forward * motion.z + right * motion.x;

        if (_velocity.sqrMagnitude <= 0.01f)
            _decelerationMultiplier = 1f;
    }


    /// <summary>
    /// Handles camera rotation with the right mouse button, orbiting around the point under the mouse.
    /// </summary>
    private void UpdateOrbit()
    {
        Vector2 orbitInput = _lookInput * (_middleClickPressed ? 1f : 0f);

        orbitInput *= _orbitSensitivity;

        InputAxis horizontalAxis = _orbitalFollow.HorizontalAxis;
        InputAxis verticalAxis = _orbitalFollow.VerticalAxis;

        horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + orbitInput.x, _orbitSmoothing * Time.unscaledDeltaTime);
        verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - orbitInput.y, _orbitSmoothing * Time.unscaledDeltaTime);

        verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

        _orbitalFollow.HorizontalAxis = horizontalAxis;
        _orbitalFollow.VerticalAxis = verticalAxis;
    }

    /// <summary>
    /// Handles zooming in/out with the mouse scroll wheel (moves toward/away from point under mouse).
    /// </summary>
    private void UpdateZoom()
    {
        InputAxis axis = _orbitalFollow.RadialAxis;

        float targetZoomSpeed = 0f;

        if (Mathf.Abs(_scrollInput.y) >= 0.01f)
        {
            targetZoomSpeed = _zoomSpeed * _scrollInput.y;
        }

        _currentZoomSpeed = Mathf.Lerp(_currentZoomSpeed, targetZoomSpeed, _zoomSmoothing * Time.unscaledDeltaTime);

        axis.Value -= _currentZoomSpeed;
        axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

        _orbitalFollow.RadialAxis = axis;
    }

    void ClampPosition()
    {
        Vector3 targetPos = _cameraTarget.position;
        targetPos.x = Mathf.Clamp(targetPos.x, -_movementLimits.x, _movementLimits.x);
        //pos.y = Mathf.Clamp(pos.y, minHeightLimit, movementLimits.y);
        targetPos.z = Mathf.Clamp(targetPos.z, -_movementLimits.z, _movementLimits.z);
        _cameraTarget.position = targetPos;
    }
    #endregion
}