
using System;
using Unity.Cinemachine;

public class Aerial_CameraState : ACameraState
{
    #region PROPERTIES
    readonly AerialCameraController _cameraController;
    #endregion

    #region CONSTRUCTOR
    public Aerial_CameraState(CameraSystem cameraSystem, AerialCameraDataSO aerialCameraData, CinemachineCamera camera)
    : base(cameraSystem, "Aerial camera", camera, aerialCameraData.SimulationSpeed)
    {
        _cameraController = new(aerialCameraData, camera);
    }
    #endregion

    #region INHERITED METHODS

    public override void LateUpdateState()
    {
        if (_gameManager.IsInAerialState)
            _cameraController.LateUpdate();
    }
    #endregion
}