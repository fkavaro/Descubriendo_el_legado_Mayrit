
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    readonly SpectatorCameraController _cameraController;
    readonly SpectatorCameraSelector _cameraSelector;

    public Spectator_CameraState(FiniteStateMachine stateMachine, CinemachineCamera camera, float simulationSpeed)
    : base("Spectator camera", stateMachine, camera, simulationSpeed)
    {
        _cameraController = new(camera, CameraManager.Instance._moveSpeedZoomCurve);
        _cameraSelector = new(CameraManager.Instance._selectableLayer);
    }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Camera.Enable();

        _camera.gameObject.SetActive(true);

        // Change HUD
        UIManager.Instance.BehaviourSystem.SwitchState(UIManager.Instance._spectatorHUDState);

        // Adjust simulation speed
        TimeManager.Instance.SetSimulationSpeed(_simulationSpeed);

        _cameraController.Start();
        _cameraSelector.Start();
    }

    public override void UpdateState()
    {
        _cameraController.Update();
        _cameraSelector.Update();
    }

    public override void LateUpdateState()
    {
        _cameraController.LateUpdate();
    }

    public override void ExitState()
    {
        _camera.gameObject.SetActive(false);
        GameManager.Instance._inputActions.Camera.Disable();
    }
}