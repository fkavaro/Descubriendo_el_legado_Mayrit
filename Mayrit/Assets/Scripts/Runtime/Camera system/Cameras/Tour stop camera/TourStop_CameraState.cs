using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TourStop_CameraState : ACameraState
{
    public TourStop_CameraState(CameraSystem cameraSystem, float simulationSpeed)
    : base(cameraSystem, "TourStop camera", null, simulationSpeed) { }
}
