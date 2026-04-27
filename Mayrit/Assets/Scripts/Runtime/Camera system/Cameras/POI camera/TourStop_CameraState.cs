using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class TourStop_CameraState : ACameraState
{
    public TourStop_CameraState(float simulationSpeed)
    : base("TourStop camera", null, simulationSpeed) { }
}
