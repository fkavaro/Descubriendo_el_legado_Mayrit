using UnityEngine;

/// <summary>
/// Camera controller supporting WASD movement, scroll zoom, right-click rotation, and middle-click panning.
/// All movement is independent of Time.timeScale.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of camera movement with WASD/arrow keys.")]
    public float movementSpeed = 5f;

    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float scrollSpeed = 10f;

    [Tooltip("Speed of camera panning with middle mouse button.")]
    public float panSpeed = 0.5f;

    [Header("Rotation Settings")]
    [Tooltip("Mouse sensitivity for camera rotation.")]
    public float rotationSensitivity = 5f;

    [Header("Movement Limits")]
    [Tooltip("Minimum allowed Y position (height) for the camera.")]
    public float minHeightLimit = 2f;

    [Tooltip("Maximum allowed X, Y, Z positions (positive and negative) for the camera.")]
    public Vector3 movementLimits;

    private Vector3 lastMousePosition;
    private bool isPanning = false;

    void Update()
    {
        HandleInput();
        ClampPosition();
    }

    /// <summary>
    /// Handles all input for movement, rotation, zoom, and panning.
    /// </summary>
    private void HandleInput()
    {
        HandleMovementInput();
        HandleScrollZoom();
        HandlePanningInput();
        HandleRotationInput();
    }

    /// <summary>
    /// Handles WASD/arrow key movement.
    /// </summary>
    private void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // Move in local X and Y (XZ plane)
        Vector3 move = new(moveX, 0, moveY);
        if (move.sqrMagnitude > 0.0001f)
        {
            transform.Translate(move.normalized * movementSpeed * Time.unscaledDeltaTime, Space.Self);
        }
    }

    /// <summary>
    /// Handles zooming in/out with the mouse scroll wheel (moves along camera's forward axis).
    /// </summary>
    private void HandleScrollZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.0001f)
        {
            // Move along camera's forward axis for intuitive zoom
            Vector3 zoom = scrollInput * scrollSpeed * transform.forward;
            transform.position += zoom;
        }
    }

    /// <summary>
    /// Handles panning with the middle mouse button.
    /// </summary>
    private void HandlePanningInput()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }
        if (isPanning)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            // Pan in local X and Y axes (screen space to world space)
            Vector3 pan = panSpeed * Time.unscaledDeltaTime * (-mouseDelta.x * transform.right + -mouseDelta.y * transform.up);
            transform.position += pan;
        }
    }

    /// <summary>
    /// Handles camera rotation with the right mouse button.
    /// </summary>
    private void HandleRotationInput()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Yaw (horizontal), Pitch (vertical)
            float yaw = mouseX * rotationSensitivity * 50f * Time.unscaledDeltaTime;
            float pitch = -mouseY * rotationSensitivity * 50f * Time.unscaledDeltaTime;

            // Apply rotation
            transform.Rotate(Vector3.up, yaw, Space.World);
            transform.Rotate(Vector3.right, pitch, Space.Self);

            // Zero out roll
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }
        else if (!isPanning)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// Clamps the camera's position within the specified movement limits.
    /// </summary>
    private void ClampPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -movementLimits.x, movementLimits.x);
        pos.y = Mathf.Clamp(pos.y, minHeightLimit, movementLimits.y);
        pos.z = Mathf.Clamp(pos.z, -movementLimits.z, movementLimits.z);
        transform.position = pos;
    }
}