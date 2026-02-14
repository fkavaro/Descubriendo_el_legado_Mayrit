using UnityEngine;

[CreateAssetMenu(fileName = "CameraDataSO", menuName = "Scriptable Objects/Camera/CameraDataSO")]
public class CameraDataSO : ScriptableObject
{
    [Range(0.1f, 10f)]
    [Tooltip("Simulation speed multiplier for this camera.")]
    public float simulationSpeed = 1f;

    [Tooltip("Speed of camera movement.")]
    public float movementSpeed;

    [Tooltip("Speed of orbital camera rotation.")]
    public float orbitSpeed;
}
