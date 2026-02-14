
using System;
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    #region PROPERTIES
    //public event Action<SelectableObject> ObjectSelectedEvent; // TODO: remove later

    readonly SpectatorCameraController _cameraController;
    //readonly SpectatorCameraSelector _cameraSelector; // TODO: remove later
    #endregion

    #region CONSTRUCTOR
    public Spectator_CameraState(SpectatorCameraDataSO spectatorCameraData, CinemachineCamera camera)
    : base("Spectator camera", camera, spectatorCameraData.SimulationSpeed)
    {
        _cameraController = new(spectatorCameraData, camera);
        //_cameraSelector = new(spectatorCameraData); // TODO: remove later
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.Camera.Enable();
        //_cameraSelector.ObjectSelectedEvent += OnObjectSelected; // TODO: remove later
    }

    public override void UpdateState()
    {
        if (_gameManager.IsInPauseState || _uiManager.IsInLoadingScreenState)
            return;

        //_cameraSelector.Update(); // TODO: remove later
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState || _uiManager.IsInLoadingScreenState)
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
    // TODO: remove later
    // void OnObjectSelected(SelectableObject selectedObject)
    // {
    //     ObjectSelectedEvent?.Invoke(selectedObject);
    // }
    #endregion
}