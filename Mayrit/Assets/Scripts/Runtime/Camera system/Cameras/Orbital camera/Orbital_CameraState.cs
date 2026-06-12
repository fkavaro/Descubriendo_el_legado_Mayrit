using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class Orbital_CameraState : ACameraState
{
    #region PROPERTIES
    readonly OrbitalCameraController _controller;
    public OrbitalCameraSettings Setting;
    #endregion

    #region CONSTRUCTOR
    public Orbital_CameraState(CameraSystem cameraSystem, OrbitalCameraDataSO orbitalCameraData, CinemachineCamera camera)
    : base(cameraSystem, "Orbital camera", camera, orbitalCameraData.SimulationSpeed)
    {
        _controller = new(orbitalCameraData, camera);
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        _controller.Start(Setting);
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState)
            return;

        _controller.LateUpdate();
    }
    #endregion
}