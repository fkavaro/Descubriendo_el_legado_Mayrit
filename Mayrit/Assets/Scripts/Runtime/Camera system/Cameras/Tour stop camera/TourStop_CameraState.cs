using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TourStop_CameraState : ACameraState
{
    public TourStop_CameraState(CameraSystem cameraManager, float simulationSpeed)
    : base(cameraManager, "TourStop camera", null, simulationSpeed) { }
}
