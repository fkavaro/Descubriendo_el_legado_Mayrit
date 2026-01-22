
using System;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Base camera data shared across all camera modes.
/// </summary>
[Serializable]
public class CameraData
{
    [Range(0.1f, 10f)]
    [Tooltip("Simulation speed multiplier for camera movement.")]
    public float simulationSpeed = 1f;

    [Tooltip("Reference to the Cinemachine camera component.")]
    public CinemachineCamera camera;

    [Tooltip("Speed of camera movement.")]
    public float movementSpeed;

    [Tooltip("Speed of orbital camera rotation.")]
    public float orbitSpeed;
}

/// <summary>
/// Configuration data for spectator camera mode with edge scrolling, zoom, and movement limits.
/// </summary>
[Serializable]
public class SpectatorCameraData
{
    public CameraData data;

    public CinemachineCamera Camera => data.camera;
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

    [Header("Selection")]
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask selectableLayer;

    [Header("Third-Person Transition")]
    [Tooltip("Speed at which to move the spectator camera target when switching from third-person camera.")]
    public float targetPositionFixSpeed = 50f;

    public void OnIsEdgeScrollingToggled(bool newValue)
    {
        isEdgeScrolling = newValue;
    }
}

/// <summary>
/// Configuration data for orbital camera mode.
/// </summary>
[Serializable]
public class OrbitalCameraData
{
    public CameraData cameraData;
    public CinemachineCamera Camera => cameraData.camera;
    public float SimulationSpeed => cameraData.simulationSpeed;
    public float MovementSpeed => cameraData.movementSpeed;
    public float OrbitSpeed => cameraData.orbitSpeed;
}

/// <summary>
/// Configuration data for third-person camera mode with orbital clamping.
/// </summary>
[Serializable]
public class ThirdPersonCameraData
{
    public CameraData cameraData;
    public CinemachineCamera Camera => cameraData.camera;
    public float SimulationSpeed => cameraData.simulationSpeed;
    public float MovementSpeed => cameraData.movementSpeed;
    public float OrbitSpeed => cameraData.orbitSpeed;

    [Header("Orbit")]
    [Tooltip("Vertical orbit angle clamp in degrees (min, max).")]
    public Vector2 _orbitClamp = new(-30f, 40f);
}