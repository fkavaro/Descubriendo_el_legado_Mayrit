
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
    public Spectator_CameraState(SpectatorCameraData spectatorCameraData)
    : base("Spectator camera", spectatorCameraData.Camera, spectatorCameraData.SimulationSpeed)
    {
        _cameraController = new(spectatorCameraData);
        _cameraSelector = new(spectatorCameraData);
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