using UnityEngine;

[CreateAssetMenu(fileName = "ThirdPersonCameraDataSO", menuName = "Scriptable Objects/Camera/ThirdPersonCameraDataSO")]
public class ThirdPersonCameraDataSO : ScriptableObject
{
    public CameraDataSO data;

    public float SimulationSpeed => data.simulationSpeed;
    public float MovementSpeed => data.movementSpeed;
    public float OrbitSpeed => data.orbitSpeed;

    [Header("Orbit")]
    [Tooltip("Vertical orbit angle clamp in degrees (min, max).")]
    public Vector2 _orbitClamp = new(-30f, 40f);
}
