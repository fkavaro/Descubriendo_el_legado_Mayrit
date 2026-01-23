using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPerson_CameraState : ACameraState
{
    #region PROPERTIES
    public event Action ExitThirdPersonCameraEvent;

    readonly ThirdPersonCameraController _cameraController;
    #endregion

    #region CONSTRUCTOR
    public ThirdPerson_CameraState(ThirdPersonCameraData thirdPersonCameraData)
    : base("Third person camera", thirdPersonCameraData.Camera, thirdPersonCameraData.SimulationSpeed)
    {
        _cameraController = new(thirdPersonCameraData);
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.Camera.Enable();
        _gameManager.InputActions.Camera.ExitMode.performed += OnExitThirdPersonModePressed;

        // Lock cursor to screen center and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState)
            return;

        _cameraController.TargetSmoothFollow();

        // Only allow camera rotation if the playable character is being controlled
        if (_gameManager.PlayableCharacter.IsBeingControlled)
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
        _gameManager.InputActions.Camera.ExitMode.performed -= OnExitThirdPersonModePressed;

        // Unlock cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion

    #region CALLBACK METHODS
    void OnExitThirdPersonModePressed(InputAction.CallbackContext context)
    {
        ExitThirdPersonCameraEvent?.Invoke();
    }
    #endregion
}
