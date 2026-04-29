
using System;
using Unity.Cinemachine;

public class Aerial_CameraState : ACameraState
{
    #region PROPERTIES
    //public event Action<SelectableObject> ObjectSelectedEvent; // TODO: remove later

    readonly AerialCameraController _cameraController;
    //readonly AerialCameraSelector _cameraSelector; // TODO: remove later
    #endregion

    #region CONSTRUCTOR
    public Aerial_CameraState(AerialCameraDataSO aerialCameraData, CinemachineCamera camera)
    : base("Aerial camera", camera, aerialCameraData.SimulationSpeed)
    {
        _cameraController = new(aerialCameraData, camera);
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