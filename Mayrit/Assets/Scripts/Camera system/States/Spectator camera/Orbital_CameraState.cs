using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    public readonly CinemachineOrbitalFollow _orbitalFollow;

    public Orbital_CameraState(FiniteStateMachine<CameraManager> stateMachine, CinemachineCamera camera)
        : base("Orbitational camera", stateMachine, camera)
    {
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
    }

    public override void StartState()
    {
        _camera.gameObject.SetActive(true);
        CameraManager.Instance.ZoomToCoroutine(_orbitalFollow, CameraManager.Instance._orbitalCameraZoomValue);
    }

    public override void UpdateState()
    {
        // Orbit around target
        _orbitalFollow.HorizontalAxis.Value += CameraManager.Instance._orbitSpeed * Time.deltaTime;
    }

    public override void ExitState()
    {
        _camera.gameObject.SetActive(false);
    }
}