using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class Orbital_CameraState : ACameraState
{
    #region PROPERTIES HELPERS
    // TODO: check
    // public SelectableObject SelectedObject
    // {
    //     get => _selectedObject;
    //     set => _selectedObject = value;
    // }

    public OrbitalStateSetting Setting
    {
        get => _setting;
        set => _setting = value;
    }
    #endregion

    #region PROPERTIES

    readonly OrbitalCameraController _controller;

    //SelectableObject _selectedObject;
    OrbitalStateSetting _setting;
    #endregion

    #region CONSTRUCTOR
    public Orbital_CameraState(OrbitalCameraDataSO orbitalCameraData, CinemachineCamera camera)
    : base("Orbital camera", camera, orbitalCameraData.SimulationSpeed)
    {
        _controller = new(orbitalCameraData, camera);
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        //Transform objectToOrbitAround = _selectedObject.transform;

        _gameManager.InputActions.Camera.Enable();
        _gameManager.InputActions.Camera.ExitMode.performed += OnExitCameraMode;

        _controller.Start(_setting);
        _uiManager.ShowContextualPanel(_setting.DataToShow, _setting.IsForCharacter);
    }

    public override void UpdateState()
    {
        // Do not update controller if game paused
        if (_gameManager.IsInPauseState) return;

        _controller.Update();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.Camera.Disable();
        _gameManager.InputActions.Camera.ExitMode.performed -= OnExitCameraMode;
    }

    #endregion

    #region CALLBACK METHODS
    void OnExitCameraMode(InputAction.CallbackContext context)
    {
        _cameraManager.SwitchToSpectatorCamera();
        _uiManager.HideContextualPanel();
    }
    #endregion
}