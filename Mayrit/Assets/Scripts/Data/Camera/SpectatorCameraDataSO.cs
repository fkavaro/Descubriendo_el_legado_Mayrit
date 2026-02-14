using UnityEngine;

[CreateAssetMenu(fileName = "SpectatorCameraDataSO", menuName = "Scriptable Objects/Camera/SpectatorCameraDataSO")]
public class SpectatorCameraDataSO : ScriptableObject
{
    public CameraDataSO data;

    public float SimulationSpeed => data.simulationSpeed;
    public float MovementSpeed => data.movementSpeed;
    public float OrbitSpeed => data.orbitSpeed;

    [Header("Movement")]
    public float acceleration = 700f;
    public float deceleration = 1000f;
    public float sprintSpeedMultiplier = 2f;
    public AnimationCurve zoomSpeedCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);

    [Header("Movement limits")]
    [Tooltip("Camera limits in X axis (min, max)")]
    public Vector2 movementLimitsX = new(-1000, 700);
    [Tooltip("Camera limits in Y axis (min: at which target will be positioned, max: max distance from target)")]
    public Vector2 movementLimitsY = new(120, 400);
    [Tooltip("Camera limits in Z axis (min, max)")]
    public Vector2 movementLimitsZ = new(-800, 800);

    [Header("Edge scrolling")]
    public bool isEdgeScrolling = true;
    public int edgeScrollingMargin = 100;

    [Header("Orbit")]
    public float orbitSmoothing = 5f;

    [Header("Zoom")]
    [Tooltip("Speed of camera zoom with scroll wheel.")]
    public float zoomSpeed = 0.1f;
    public float zoomSmoothing = 5f;

    // TODO remove later
    // [Header("Selection")]
    // [Tooltip("Layer mask to define which objects are selectable.")]
    // public LayerMask selectableLayer;

    [Header("Third-Person Transition")]
    [Tooltip("Speed at which to move the spectator camera target when switching from third-person camera.")]
    public float targetPositionFixSpeed = 50f;

    public void OnIsEdgeScrollingToggled(bool newValue)
    {
        isEdgeScrolling = newValue;
    }
}
