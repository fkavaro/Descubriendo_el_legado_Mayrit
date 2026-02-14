using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPerson_CameraState : ACameraState
{
    #region PROPERTIES
    readonly ThirdPersonCameraController _cameraController;
    #endregion

    #region CONSTRUCTOR
    public ThirdPerson_CameraState(ThirdPersonCameraDataSO thirdPersonCameraData, CinemachineCamera camera)
    : base("Third person camera", camera, thirdPersonCameraData.SimulationSpeed)
    {
        _cameraController = new(thirdPersonCameraData, camera);
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.Camera.Enable();
        _gameManager.InputActions.Camera.ExitMode.performed += OnExitCameraMode;

        // Lock cursor to screen center and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState || _uiManager.IsInLoadingScreenState)
            return;

        if (_cameraManager.PlayableCharacter == null)
        {
            Debug.LogWarning("ThirdPerson_CameraState: No playable character found for third-person camera.");
            return;
        }

        _cameraController.TargetSmoothFollow(_cameraManager.PlayableCharacter.transform);

        // Only allow camera rotation if the playable character is being controlled
        if (_cameraManager.PlayableCharacter.IsBeingControlled)
            _cameraController.MouseTracking();

        // Ensure cursor remains locked during gameplay
        if (_uiManager.IsInPlayerHUDState
            && _gameManager.IsInGamePlayState
            && Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.Camera.Disable();
        _gameManager.InputActions.Camera.ExitMode.performed -= OnExitCameraMode;

        // Unlock cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion

    #region CALLBACK METHODS
    void OnExitCameraMode(InputAction.CallbackContext context)
    {
        _cameraManager.SwitchToSpectatorCamera();
    }
    #endregion
}
