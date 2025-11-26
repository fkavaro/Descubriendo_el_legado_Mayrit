
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    readonly SpectatorCameraController _cameraController;
    readonly SpectatorCameraSelector _cameraSelector;

    public Spectator_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Spectator camera", camera, simulationSpeed)
    {
        _cameraController = new(camera, CameraManager.Instance._moveSpeedZoomCurve);
        _cameraSelector = new(CameraManager.Instance._selectableLayer);
    }

    public override void StartState()
    {
        GameManager.Instance.InputActions.Camera.Enable();

        _camera.gameObject.SetActive(true);

        // Change HUD
        UIManager.Instance.SwitchToSpectatorHUDState();

        // Adjust simulation speed
        TimeManager.Instance.SetSimulationSpeed(_simulationSpeed);
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
        GameManager.Instance.InputActions.Camera.Disable();
    }
}