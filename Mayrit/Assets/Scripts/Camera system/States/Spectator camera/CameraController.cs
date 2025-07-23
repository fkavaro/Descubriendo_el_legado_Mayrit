using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
public class CameraController : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    [Header("Components")]
    [SerializeField] Transform _cameraTarget;
    [SerializeField] CinemachineOrbitalFollow _orbitalFollow;

    [Header("Movement")]
    [Tooltip("Wether to move camera at screen margins or not.")]
    [SerializeField] bool _edgeScrolling = false;
    [SerializeField] int _edgeScrollingMargin = 30;

    [Space(10)]
    [SerializeField] float _moveSpeed = 500f;
    [SerializeField] AnimationCurve _moveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [SerializeField] float _acceleration = 200f;
    [SerializeField] float _deceleration = 250f;
    [SerializeField] float _printSpeedMultiplier = 2f;


    [Header("Orbit")]
    [Tooltip("Mouse sensitivity for camera rotation.")]
    [SerializeField] float _orbitSensitivity = 0.5f;
    [SerializeField] float _orbitSmoothing = 5f;

    [Header("Zoom")]
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    [SerializeField] float _zoomSpeed = 0.1f;
    [SerializeField] float _zoomSmoothing = 5f;
    public float ZoomLevel // value between 0 (zoomed in) and 1 (zoomed out)
    {
        get
        {
            InputAxis axis = _orbitalFollow.RadialAxis;

            return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
        }
    }

    [Header("Movement Limits")]

    [Tooltip("Maximum allowed X, Y, Z positions (positive and negative) for the camera.")]
    [SerializeField] Vector3 _movementLimits = new(800, 0, 800);
    #endregion

    #region PRIVATE PROPERTIES
    Vector2 _edgeScrollInput;
    float _decelerationMultiplier = 1f;
    Vector3 _velocity = Vector3.zero;
    float _currentZoomSpeed = 0f;

    Vector2 _moveInput, _lookInput, _scrollInput;
    bool _sprintPressed, _middleClickPressed;
    #endregion

    #region MONOBEHAVIOUR
    void Awake()
    {

    }

    void LateUpdate()
    {
        HandleInput();
        ClampPosition();
    }
    #endregion

    #region METHODS
    private void HandleInput()
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
    }
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
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
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