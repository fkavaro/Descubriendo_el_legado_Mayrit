
using System;
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    #region PROPERTIES
    public event Action<SelectableObject> ObjectSelectedEvent;

    readonly SpectatorCameraController _cameraController;
    readonly SpectatorCameraSelector _cameraSelector;
    #endregion

    #region CONSTRUCTOR
    public Spectator_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Spectator camera", camera, simulationSpeed)
    {
        _cameraController = new(camera);
        _cameraSelector = new();
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.Camera.Enable();
        _uiManager.SwitchToSpectatorHUDState();
        _cameraSelector.ObjectSelectedEvent += OnObjectSelected;
    }

    public override void UpdateState()
    {
        if (_gameManager.IsInPauseState)
            return;

        _cameraController.Update();
        _cameraSelector.Update();
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState)
            return;

        _cameraController.LateUpdate();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.Camera.Disable();
    }
    #endregion

    #region CALLBACK METHODS
    void OnObjectSelected(SelectableObject selectedObject)
    {
        ObjectSelectedEvent?.Invoke(selectedObject);
    }
    #endregion
}