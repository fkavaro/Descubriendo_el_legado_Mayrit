using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
public class SpectatorCameraController
{
    #region  PROPERTIES
    readonly CinemachineCamera _camera;
    readonly Transform _cameraTarget;
    readonly CinemachineOrbitalFollow _orbitalFollow;
    readonly AnimationCurve _moveSpeedZoomCurve;

    // Edge scrolling
    bool _edgeScrolling;
    int _edgeScrollingMargin;

    // Movement
    float _moveSpeed,
        _acceleration,
        _deceleration,
        _printSpeedMultiplier = 2f;
    Vector3 _movementLimitsX
        , _movementLimitsZ;

    // Orbit
    float _orbitSpeed,
        _orbitSmoothing;

    // Zoom
    float _zoomSpeed,
        _zoomSmoothing;
    float ZoomLevel // value between 0 (zoomed in) and 1 (zoomed out)
    {
        get
        {
            InputAxis axis = _orbitalFollow.RadialAxis;

            return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
        }
    }

    Vector2 _edgeScrollInput;
    float _decelerationMultiplier = 1f,
        _currentZoomSpeed = 0f;
    Vector3 _velocity = Vector3.zero;

    // Input
    Vector2 _moveInput, _lookInput, _scrollInput;
    bool _sprintPressed, _middleClickPressed;

    // Dependency Injection
    readonly CameraManager _cameraManager;
    readonly GameManager _gameManager;
    readonly UIManager _uiManager;
    #endregion

    #region CONSTRUCTOR
    public SpectatorCameraController(CinemachineCamera camera)
    {
        _camera = camera;
        _cameraTarget = camera.Target.TrackingTarget.transform;
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();

        // Get dependencies from ServiceLocator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        _moveSpeedZoomCurve = _cameraManager._moveSpeedZoomCurve;
    }
    #endregion

    #region LIFE CYCLE
    public void Update()
    {
        _edgeScrolling = _cameraManager.EdgeScrolling;

        // TODO: move to constructor when final values
        _edgeScrollingMargin = _cameraManager._edgeScrollingMargin;
        _moveSpeed = _cameraManager._moveSpeed;
        _acceleration = _cameraManager._acceleration;
        _deceleration = _cameraManager._deceleration;
        _printSpeedMultiplier = _cameraManager._sprintSpeedMultiplier;
        _movementLimitsX = _cameraManager._movementLimitsX;
        _movementLimitsZ = _cameraManager._movementLimitsZ;
        _orbitSpeed = _cameraManager._spectatorCameraOrbitSpeed;
        _orbitSmoothing = _cameraManager._orbitSmoothing;
        _zoomSpeed = _cameraManager._zoomSpeed;
        _zoomSmoothing = _cameraManager._zoomSmoothing;
    }

    public void LateUpdate()
    {
        _moveInput = _gameManager.InputActions.Camera.Move.ReadValue<Vector2>();
        _sprintPressed = _gameManager.InputActions.Camera.Sprint.IsPressed();
        _lookInput = _gameManager.InputActions.Camera.Look.ReadValue<Vector2>();
        _middleClickPressed = _gameManager.InputActions.Camera.Rotate.IsPressed();

        // Cursor NOT over UI element
        if (!_uiManager.IsCursorOverUI())
            _scrollInput = _gameManager.InputActions.Camera.Zoom.ReadValue<Vector2>();

        if (_edgeScrolling)
            UpdateEdgeScrolling();

        UpdateMovement();
        UpdateZoom();
        UpdateOrbit();

        ClampPosition();
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateEdgeScrolling()
    {
        _edgeScrollInput = Vector2.zero;

        // Ensure mouse is available before reading position
        if (Mouse.current == null)
            return;

        // Return if mouse is over UI
        if (_uiManager.IsCursorOverUI())
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Only apply edge scrolling if mouse position is valid (not at origin on first frame)
        if (mousePosition.x > 0 || mousePosition.y > 0)
        {
            if (mousePosition.x <= _edgeScrollingMargin)
                _edgeScrollInput.x = -1f;
            else if (mousePosition.x >= Screen.width - _edgeScrollingMargin)
                _edgeScrollInput.x = 1f;

            if (mousePosition.y <= _edgeScrollingMargin)
                _edgeScrollInput.y = -1f;
            else if (mousePosition.y >= Screen.height - _edgeScrollingMargin)
                _edgeScrollInput.y = 1f;
        }
    }

    /// <summary>
    /// Moves the camera in its local XZ plane, but keeps its Y (height) unchanged.
    /// </summary>
    void UpdateMovement()
    {
        Vector3 forward = _camera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = _camera.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 inputVector = new(_moveInput.x + _edgeScrollInput.x, 0,
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
    void UpdateOrbit()
    {
        Vector2 orbitInput = _lookInput * (_middleClickPressed ? 1f : 0f);

        orbitInput *= _orbitSpeed;

        InputAxis horizontalRotation = _orbitalFollow.HorizontalAxis;
        InputAxis verticalRotation = _orbitalFollow.VerticalAxis;

        horizontalRotation.Value = Mathf.Lerp(horizontalRotation.Value, horizontalRotation.Value + orbitInput.x, _orbitSmoothing * Time.unscaledDeltaTime);
        verticalRotation.Value = Mathf.Lerp(verticalRotation.Value, verticalRotation.Value - orbitInput.y, _orbitSmoothing * Time.unscaledDeltaTime);

        verticalRotation.Value = Mathf.Clamp(verticalRotation.Value, verticalRotation.Range.x, verticalRotation.Range.y);

        _orbitalFollow.HorizontalAxis = horizontalRotation;
        _orbitalFollow.VerticalAxis = verticalRotation;
    }

    /// <summary>
    /// Handles zooming in/out with the mouse scroll wheel (moves toward/away from point under mouse).
    /// </summary>
    void UpdateZoom()
    {
        InputAxis zoomValue = _orbitalFollow.RadialAxis;

        float targetZoomSpeed = 0f;

        if (Mathf.Abs(_scrollInput.y) >= 0.01f)
        {
            targetZoomSpeed = _zoomSpeed * _scrollInput.y;
        }

        _currentZoomSpeed = Mathf.Lerp(_currentZoomSpeed, targetZoomSpeed, _zoomSmoothing * Time.unscaledDeltaTime);

        zoomValue.Value -= _currentZoomSpeed;
        zoomValue.Value = Mathf.Clamp(zoomValue.Value, zoomValue.Range.x, zoomValue.Range.y);

        _orbitalFollow.RadialAxis = zoomValue;
    }

    void ClampPosition()
    {
        Vector3 targetPos = _cameraTarget.position;
        targetPos.x = Mathf.Clamp(targetPos.x, _movementLimitsX.x, _movementLimitsX.y);
        targetPos.z = Mathf.Clamp(targetPos.z, _movementLimitsZ.x, _movementLimitsZ.y);
        _cameraTarget.position = targetPos;
    }
    #endregion
}