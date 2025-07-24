using UnityEngine;
using Unity.Cinemachine;

public class Orbital_CameraState : ACameraState
{
    public readonly CinemachineOrbitalFollow _orbitalFollow;
    public readonly CinemachineCameraOffset _offsetComponent;

    public Orbital_CameraState(FiniteStateMachine<CameraManager> stateMachine, CinemachineCamera camera)
        : base("Orbitational camera", stateMachine, camera)
    {
        _orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
        _offsetComponent = camera.GetComponent<CinemachineCameraOffset>();
    }

    public override void StartState()
    {
        _camera.gameObject.SetActive(true);

        CameraManager.Instance.ZoomToCoroutine(_orbitalFollow, CameraManager.Instance._orbitalCameraZoomValue);
        CameraManager.Instance.HorizontalOffsetCoroutine(_offsetComponent, CameraManager.Instance._horizontalOffset);
    }

    public override void UpdateState()
    {
        // Orbit around target
        _orbitalFollow.HorizontalAxis.Value += CameraManager.Instance._orbitSpeed * Time.deltaTime;
    }

    public override void ExitState()
    {
        _camera.gameObject.SetActive(false);
        CameraManager.Instance.HorizontalOffsetCoroutine(_offsetComponent, 0);
    }
}