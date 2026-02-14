using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
public class SpectatorCameraController
{
    #region PROPERTIES
    // Constants
    const float MIN_VELOCITY_THRESHOLD = 0.01f;
    const float MIN_INPUT_THRESHOLD = 0.01f;

    // Camera references
    readonly CinemachineCamera _camera;
    readonly Transform _cameraTarget;
    readonly CinemachineOrbitalFollow _orbitalFollow;

    // Configuration
    readonly AnimationCurve _moveSpeedZoomCurve;
    readonly int _edgeScrollingMargin;
    readonly float _moveSpeed;
    readonly float _acceleration;
    readonly float _deceleration;
    readonly float _sprintSpeedMultiplier;
    readonly Vector3 _movementLimitsX;
    readonly Vector3 _movementLimitsZ;
    readonly float _orbitSpeed;
    readonly float _orbitSmoothing;
    readonly Vector2 _verticalAngleLimits;
    readonly float _zoomSpeed;
    readonly float _zoomSmoothing;

    // Runtime state - Movement
    Vector3 _velocity = Vector3.zero;
    float _decelerationMultiplier = 1f;
    Vector2 _edgeScrollInput;

    // Runtime state - Zoom
    float _currentZoomSpeed = 0f;

    // Input state
    Vector2 _moveInput;
    Vector2 _lookInput;
    Vector2 _scrollInput;
    bool _sprintPressed;
    bool _middleClickPressed;

    // Computed properties
    float ZoomLevel // Normalized zoom level: 0 (zoomed in) to 1 (zoomed out)
    {
        get
        {
            InputAxis radialAxis = _orbitalFollow.RadialAxis;
            return Mathf.InverseLerp(radialAxis.Range.x, radialAxis.Range.y, radialAxis.Value);
        }
    }

    // Dependencies
    readonly SpectatorCameraDataSO _spectatorCameraData;
    readonly GameManager _gameManager;
    readonly UIManager _uiManager;
    #endregion

    #region CONSTRUCTOR
    public SpectatorCameraController(SpectatorCameraDataSO spectatorCameraData, CinemachineCamera camera)
    {
        _spectatorCameraData = spectatorCameraData;

        // Initialize camera references
        _camera = camera;
        _cameraTarget = _camera.Target.TrackingTarget.transform;
        _orbitalFollow = _camera.GetComponent<CinemachineOrbitalFollow>();

        // Initialize configuration from data
        _edgeScrollingMargin = spectatorCameraData.edgeScrollingMargin;
        _moveSpeed = spectatorCameraData.MovementSpeed;
        _acceleration = spectatorCameraData.acceleration;
        _deceleration = spectatorCameraData.deceleration;
        _sprintSpeedMultiplier = spectatorCameraData.sprintSpeedMultiplier;
        _movementLimitsX = spectatorCameraData.movementLimitsX;
        _movementLimitsZ = spectatorCameraData.movementLimitsZ;
        _orbitSpeed = spectatorCameraData.OrbitSpeed;
        _orbitSmoothing = spectatorCameraData.orbitSmoothing;
        _verticalAngleLimits = spectatorCameraData.verticalAngleLimits;
        _zoomSpeed = spectatorCameraData.zoomSpeed;
        _zoomSmoothing = spectatorCameraData.zoomSmoothing;
        _moveSpeedZoomCurve = spectatorCameraData.zoomSpeedCurve;

        // Get dependencies from ServiceLocator
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        // Set initial orbital limits
        _orbitalFollow.VerticalAxis.Range = _verticalAngleLimits;
    }
    #endregion

    #region LIFE CYCLE
    public void LateUpdate()
    {
        // Read input state
        _moveInput = _gameManager.InputActions.Camera.Move.ReadValue<Vector2>();
        _sprintPressed = _gameManager.InputActions.Camera.Sprint.IsPressed();
        _lookInput = _gameManager.InputActions.Camera.Look.ReadValue<Vector2>();
        _middleClickPressed = _gameManager.InputActions.Camera.Rotate.IsPressed();

        // Only read zoom input when cursor is not over UI
        if (!_uiManager.IsCursorOverUI)
            _scrollInput = _gameManager.InputActions.Camera.Zoom.ReadValue<Vector2>();

        // Update camera state
        if (_spectatorCameraData.isEdgeScrolling)
            UpdateEdgeScrolling();

        UpdateMovement();
        UpdateZoom();
        UpdateOrbit();
        ClampPosition();
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Detects when the cursor is near screen edges and generates scroll input.
    /// </summary>
    void UpdateEdgeScrolling()
    {
        _edgeScrollInput = Vector2.zero;

        // Early exit if mouse is unavailable or over UI
        if (Mouse.current == null || _uiManager.IsCursorOverUI)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Skip invalid positions (origin on first frame)
        if (mousePosition.x <= 0 && mousePosition.y <= 0)
            return;

        // Check horizontal edges
        if (mousePosition.x <= _edgeScrollingMargin)
            _edgeScrollInput.x = -1f;
        else if (mousePosition.x >= Screen.width - _edgeScrollingMargin)
            _edgeScrollInput.x = 1f;

        // Check vertical edges
        if (mousePosition.y <= _edgeScrollingMargin)
            _edgeScrollInput.y = -1f;
        else if (mousePosition.y >= Screen.height - _edgeScrollingMargin)
            _edgeScrollInput.y = 1f;
    }

    /// <summary>
    /// Moves the camera in its local XZ plane, keeping Y (height) unchanged.
    /// Supports WASD input, edge scrolling, sprint modifier, and zoom-based speed scaling.
    /// </summary>
    void UpdateMovement()
    {
        // Get camera-relative movement directions (flattened to XZ plane)
        Vector3 forward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(_camera.transform.right, Vector3.up).normalized;

        // Combine keyboard and edge scroll input
        Vector3 combinedInput = new Vector3(
            _moveInput.x + _edgeScrollInput.x,
            0f,
            _moveInput.y + _edgeScrollInput.y
        ).normalized;

        // Scale movement speed based on zoom level (slower when zoomed in)
        float zoomSpeedMultiplier = _moveSpeedZoomCurve.Evaluate(ZoomLevel);
        float sprintMultiplier = _sprintPressed ? _sprintSpeedMultiplier : 1f;
        Vector3 targetVelocity = _moveSpeed * sprintMultiplier * zoomSpeedMultiplier * combinedInput;

        // Accelerate or decelerate based on input
        bool hasInput = combinedInput.sqrMagnitude > MIN_INPUT_THRESHOLD;
        if (hasInput)
        {
            float accelRate = _acceleration * sprintMultiplier * Time.unscaledDeltaTime;
            _velocity = Vector3.MoveTowards(_velocity, targetVelocity, accelRate);

            // Remember sprint state for deceleration phase
            if (_sprintPressed)
                _decelerationMultiplier = _sprintSpeedMultiplier;
        }
        else
        {
            float decelRate = _deceleration * _decelerationMultiplier * Time.unscaledDeltaTime;
            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, decelRate);

            // Reset deceleration multiplier when stopped
            if (_velocity.sqrMagnitude <= MIN_VELOCITY_THRESHOLD)
                _decelerationMultiplier = 1f;
        }

        // Apply movement
        Vector3 motion = _velocity * Time.unscaledDeltaTime;
        _cameraTarget.position += forward * motion.z + right * motion.x;
    }


    /// <summary>
    /// Handles camera orbital rotation when middle mouse button is held.
    /// </summary>
    void UpdateOrbit()
    {
        // Only rotate when middle mouse button is pressed
        if (!_middleClickPressed)
            return;

        Vector2 rotationDelta = _lookInput * _orbitSpeed;

        // Get current rotation axes
        InputAxis horizontalAxis = _orbitalFollow.HorizontalAxis;
        InputAxis verticalAxis = _orbitalFollow.VerticalAxis;

        // Apply rotation with smoothing
        float smoothFactor = _orbitSmoothing * Time.unscaledDeltaTime;
        horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + rotationDelta.x, smoothFactor);
        verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - rotationDelta.y, smoothFactor);

        // Clamp vertical rotation to prevent flipping
        verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

        // Write back modified axes
        _orbitalFollow.HorizontalAxis = horizontalAxis;
        _orbitalFollow.VerticalAxis = verticalAxis;
    }

    /// <summary>
    /// Handles camera zoom with mouse scroll wheel, smoothly adjusting the radial distance.
    /// </summary>
    void UpdateZoom()
    {
        InputAxis radialAxis = _orbitalFollow.RadialAxis;

        // Calculate target zoom speed from scroll input
        float targetZoomSpeed = (Mathf.Abs(_scrollInput.y) >= MIN_INPUT_THRESHOLD)
            ? _zoomSpeed * _scrollInput.y
            : 0f;

        // Smooth zoom speed changes
        _currentZoomSpeed = Mathf.Lerp(
            _currentZoomSpeed,
            targetZoomSpeed,
            _zoomSmoothing * Time.unscaledDeltaTime
        );

        // Apply zoom and clamp to valid range
        radialAxis.Value = Mathf.Clamp(
            radialAxis.Value - _currentZoomSpeed,
            radialAxis.Range.x,
            radialAxis.Range.y
        );

        _orbitalFollow.RadialAxis = radialAxis;
    }

    /// <summary>
    /// Constrains the camera target position within the configured world bounds.
    /// </summary>
    void ClampPosition()
    {
        Vector3 position = _cameraTarget.position;
        position.x = Mathf.Clamp(position.x, _movementLimitsX.x, _movementLimitsX.y);
        position.z = Mathf.Clamp(position.z, _movementLimitsZ.x, _movementLimitsZ.y);
        _cameraTarget.position = position;
    }
    #endregion
}