using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
// [RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    [Header("Components")]
    [SerializeField] Transform CameraTarget;
    [SerializeField] CinemachineOrbitalFollow OrbitalFollow;

    [Header("Movement")]
    [Tooltip("Wether to move camera at screen margins or not.")]
    [SerializeField] bool edgeScrolling = true;
    [SerializeField] int EdgeScrollingMargin = 30;

    [Space(10)]
    [SerializeField] float MoveSpeed = 100f;
    [SerializeField] AnimationCurve MoveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);
    [SerializeField] float Acceleration = 100f;
    [SerializeField] float Deceleration = 150f;
    [SerializeField] float SprintSpeedMultiplier = 3f;


    [Header("Orbit")]
    [Tooltip("Mouse sensitivity for camera rotation.")]
    [SerializeField] float OrbitSensitivity = 0.5f;
    [SerializeField] float OrbitSmoothing = 5f;

    [Header("Zoom")]
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    [SerializeField] float ZoomSpeed = 0.5f;
    [SerializeField] float ZoomSmoothing = 5f;
    public float ZoomLevel // value between 0 (zoomed in) and 1 (zoomed out)
    {
        get
        {
            InputAxis axis = OrbitalFollow.RadialAxis;

            return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
        }
    }

    [Header("Movement Limits")]

    [Tooltip("Maximum allowed X, Y, Z positions (positive and negative) for the camera.")]
    [SerializeField] Vector3 movementLimits;
    #endregion

    #region PRIVATE PROPERTIES
    Vector2 edgeScrollInput;
    float decelerationMultiplier = 1f;
    Vector3 Velocity = Vector3.zero;
    float CurrentZoomSpeed = 0f;

    GameInputActions inputActions;
    InputAction moveAction, lookAction, scrollAction, rotateButtonAction, sprintAction;
    Vector2 moveInput, scrollInput, lookInput;
    bool sprintInput, middleClickInput;
    #endregion

    #region MONOBEHAVIOUR
    void Awake()
    {
        inputActions = new();
        inputActions.Camera.Enable();
        moveAction = inputActions.Camera.Move;
        lookAction = inputActions.Camera.Look;
        scrollAction = inputActions.Camera.Zoom;
        rotateButtonAction = inputActions.Camera.Rotate;
        sprintAction = inputActions.Camera.Sprint;
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
        if (edgeScrolling)
            UpdateEdgeScrolling();
        UpdateMovement();
        UpdateZoom();
        UpdateOrbit();
    }
    void UpdateEdgeScrolling()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        edgeScrollInput = Vector2.zero;

        if (mousePosition.x <= EdgeScrollingMargin)
            edgeScrollInput.x = -1f;
        else if (mousePosition.x >= Screen.width - EdgeScrollingMargin)
            edgeScrollInput.x = 1f;

        if (mousePosition.y <= EdgeScrollingMargin)
            edgeScrollInput.y = -1f;
        else if (mousePosition.y >= Screen.height - EdgeScrollingMargin)
            edgeScrollInput.y = 1f;
    }

    /// <summary>
    /// Moves the camera in its local XZ plane, but keeps its Y (height) unchanged.
    /// </summary>
    private void UpdateMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        sprintInput = sprintAction.ReadValue<float>() > 0.5f;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 inputVector = new Vector3(moveInput.x + edgeScrollInput.x, 0,
            moveInput.y + edgeScrollInput.y);
        inputVector.Normalize();

        float zoomMultiplier = MoveSpeedZoomCurve.Evaluate(ZoomLevel);

        Vector3 targetVelocity = inputVector * MoveSpeed * zoomMultiplier;

        float sprintFactor = 1f;
        if (sprintInput)
        {
            targetVelocity *= SprintSpeedMultiplier;
            sprintFactor = SprintSpeedMultiplier;
        }

        if (inputVector.sqrMagnitude > 0.01f)
        {
            Velocity = Vector3.MoveTowards(Velocity, targetVelocity, Acceleration * sprintFactor * Time.unscaledDeltaTime);

            if (sprintInput)
                decelerationMultiplier = SprintSpeedMultiplier;
        }
        else
            Velocity = Vector3.MoveTowards(Velocity, Vector3.zero, Deceleration * decelerationMultiplier * Time.unscaledDeltaTime);

        Vector3 motion = Velocity * Time.unscaledDeltaTime;

        CameraTarget.position += forward * motion.z + right * motion.x;

        if (Velocity.sqrMagnitude <= 0.01f)
            decelerationMultiplier = 1f;
    }


    /// <summary>
    /// Handles camera rotation with the right mouse button, orbiting around the point under the mouse.
    /// </summary>
    private void UpdateOrbit()
    {
        middleClickInput = rotateButtonAction.ReadValue<float>() > 0.5f;
        lookInput = lookAction.ReadValue<Vector2>();

        Vector2 orbitInput = lookInput * (middleClickInput ? 1f : 0f);

        orbitInput *= OrbitSensitivity;

        InputAxis horizontalAxis = OrbitalFollow.HorizontalAxis;
        InputAxis verticalAxis = OrbitalFollow.VerticalAxis;

        //horizontalAxis.Value += orbitInput.x;
        //verticalAxis.Value -= orbitInput.y;

        horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + orbitInput.x, OrbitSmoothing * Time.unscaledDeltaTime);
        verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - orbitInput.y, OrbitSmoothing * Time.unscaledDeltaTime);

        //horizontalAxis.Value = Mathf.Clamp(horizontalAxis.Value, horizontalAxis.Range.x, horizontalAxis.Range.y);
        verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

        OrbitalFollow.HorizontalAxis = horizontalAxis;
        OrbitalFollow.VerticalAxis = verticalAxis;
    }

    /// <summary>
    /// Handles zooming in/out with the mouse scroll wheel (moves toward/away from point under mouse).
    /// </summary>
    private void UpdateZoom()
    {
        scrollInput = scrollAction.ReadValue<Vector2>();

        InputAxis axis = OrbitalFollow.RadialAxis;

        float targetZoomSpeed = 0f;

        if (Mathf.Abs(scrollInput.y) >= 0.01f)
        {
            targetZoomSpeed = ZoomSpeed * scrollInput.y;
        }

        CurrentZoomSpeed = Mathf.Lerp(CurrentZoomSpeed, targetZoomSpeed, ZoomSmoothing * Time.unscaledDeltaTime);

        axis.Value -= CurrentZoomSpeed;
        axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

        OrbitalFollow.RadialAxis = axis;
    }

    void ClampPosition()
    {
        Vector3 targetPos = CameraTarget.position;
        targetPos.x = Mathf.Clamp(targetPos.x, -movementLimits.x, movementLimits.x);
        //pos.y = Mathf.Clamp(pos.y, minHeightLimit, movementLimits.y);
        targetPos.z = Mathf.Clamp(targetPos.z, -movementLimits.z, movementLimits.z);
        CameraTarget.position = targetPos;
    }
    #endregion
}