
using System;
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    public event Action<SelectableObject> ObjectSelectedEvent;

    readonly SpectatorCameraController _cameraController;
    readonly SpectatorCameraSelector _cameraSelector;

    public Spectator_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Spectator camera", camera, simulationSpeed)
    {
        _cameraController = new(camera);
        _cameraSelector = new();
    }

    public override void OnStateStarted()
    {
        _gameManager.InputActions.Camera.Enable();
        _uiManager.SwitchToSpectatorHUDState();
        _cameraSelector.ObjectSelectedEvent += OnObjectSelected;
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

    public override void OnStateExited()
    {
        _gameManager.InputActions.Camera.Disable();
    }

    void OnObjectSelected(SelectableObject selectedObject)
    {
        ObjectSelectedEvent?.Invoke(selectedObject);
    }
}