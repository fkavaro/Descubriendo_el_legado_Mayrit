using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Camera controller supporting WASD movement, edge scrolling, zoom and orbit relative to mouse pointer.
/// All movement is independent of Time.timeScale.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Wether to move camera at screen margins or not.")]
    public bool edgeScrolling = true;
    public int edgeSize = 30;

    [Tooltip("Speed of camera movement with WASD/arrow keys.")]
    public float movementSpeed = 5f;

    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float scrollSpeed = 10f;

    [Header("Rotation Settings")]
    [Tooltip("Mouse sensitivity for camera rotation.")]
    public float rotationSensitivity = 5f;

    [Header("Movement Limits")]
    [Tooltip("Minimum allowed Y position (height) for the camera.")]
    public float minHeightLimit = 2f;

    [Tooltip("Maximum allowed X, Y, Z positions (positive and negative) for the camera.")]
    public Vector3 movementLimits;

    float heightFactor = 1f; // Used to interpolate movement and zoom speed according to interpolated height
    float interpolatedHeight;

    GameInputActions inputActions;
    InputAction moveAction;
    InputAction lookAction;
    InputAction scrollAction;
    InputAction rotateButtonAction;

    private Camera cam;

    void Awake()
    {
        inputActions = new();
        inputActions.Camera.Enable();
        moveAction = inputActions.Camera.Move;
        lookAction = inputActions.Camera.Look;
        scrollAction = inputActions.Camera.Zoom;
        rotateButtonAction = inputActions.Camera.Rotate;
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleInput();
        ClampPosition();
    }

    private void HandleInput()
    {
        HandleMovementInput();
        HandleScrollInput();
        HandleRotationInput();
    }

    /// <summary>
    /// Moves the camera in its local XZ plane, but keeps its Y (height) unchanged.
    /// </summary>
    private void HandleMovementInput()
    {
        Vector2 movementInput = moveAction.ReadValue<Vector2>();

        if (edgeScrolling)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (mousePos.x < edgeSize)
                movementInput.x -= 1f;
            else if (mousePos.x > Screen.width - edgeSize)
                movementInput.x += 1f;

            if (mousePos.y < edgeSize)
                movementInput.y -= 1f;
            else if (mousePos.y > Screen.height - edgeSize)
                movementInput.y += 1f;
        }

        Vector3 localRight = transform.right;
        Vector3 localForward = transform.forward;
        localRight.y = 0f;
        localForward.y = 0f;
        localRight.Normalize();
        localForward.Normalize();

        Vector3 movement = localRight * movementInput.x + localForward * movementInput.y;

        if (movement.sqrMagnitude > 0.0001f)
        {
            movement = movementSpeed * heightFactor * Time.unscaledDeltaTime * movement.normalized;
            Vector3 newPosition = transform.position + movement;
            newPosition.y = transform.position.y; // Maintain height
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Handles zooming in/out with the mouse scroll wheel (moves toward/away from point under mouse).
    /// </summary>
    private void HandleScrollInput()
    {
        Vector2 scroll = scrollAction.ReadValue<Vector2>();
        float scrollInput = scroll.y;

        if (Mathf.Abs(scrollInput) > 0.0001f)
        {
            Vector3 mouseWorldPoint = GetMouseWorldPoint();
            float zoomAmount = scrollInput * scrollSpeed * heightFactor;

            if (mouseWorldPoint != Vector3.positiveInfinity)
            {
                Vector3 direction = (mouseWorldPoint - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, mouseWorldPoint);

                // Prevent overshooting the target point
                if (zoomAmount > 0 && zoomAmount > distance - 0.1f)
                    zoomAmount = distance - 0.1f;

                transform.position += direction * zoomAmount;
            }
            else
            {
                // fallback: zoom along forward
                transform.position += zoomAmount * transform.forward;
            }
        }
    }

    /// <summary>
    /// Handles camera rotation with the right mouse button, orbiting around the point under the mouse.
    /// </summary>
    private void HandleRotationInput()
    {
        bool rotatePressed = rotateButtonAction.ReadValue<float>() > 0.5f;

        if (rotatePressed)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
            Vector3 pivot = GetMouseWorldPoint();

            if (pivot == Vector3.positiveInfinity)
            {
                // fallback: orbit around camera's current position
                pivot = transform.position + transform.forward * 10f;
            }

            // Calculate angles
            float yaw = mouseDelta.x * rotationSensitivity * Time.unscaledDeltaTime;
            float pitch = -mouseDelta.y * rotationSensitivity * Time.unscaledDeltaTime;

            // Orbit horizontally (yaw)
            transform.RotateAround(pivot, Vector3.up, yaw);

            // Orbit vertically (pitch)
            Vector3 right = transform.right;
            transform.RotateAround(pivot, right, pitch);

            // Zero out roll
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// Raycasts from the mouse position to the ground plane (Y=0) and returns the hit point.
    /// Returns Vector3.positiveInfinity if nothing is hit.
    /// </summary>
    private Vector3 GetMouseWorldPoint()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        // Raycast to ground plane (Y=0)
        Plane ground = new(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.positiveInfinity;
    }

    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -movementLimits.x, movementLimits.x);
        pos.y = Mathf.Clamp(pos.y, minHeightLimit, movementLimits.y);
        pos.z = Mathf.Clamp(pos.z, -movementLimits.z, movementLimits.z);
        transform.position = pos;

        // Remap heightFactor: 0.1 at minHeightLimit, 1 at movementLimits.y
        interpolatedHeight = Mathf.InverseLerp(minHeightLimit, movementLimits.y, pos.y);
        heightFactor = Mathf.Lerp(0.1f, 1f, interpolatedHeight);
    }
}