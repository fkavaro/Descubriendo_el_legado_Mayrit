using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalCameraDataSO", menuName = "Scriptable Objects/Camera/OrbitalCameraDataSO")]
public class OrbitalCameraDataSO : ScriptableObject
{
    public CameraDataSO data;

    public float SimulationSpeed => data.simulationSpeed;
    public float MovementSpeed => data.movementSpeed;
    public float OrbitSpeed => data.orbitSpeed;
}
