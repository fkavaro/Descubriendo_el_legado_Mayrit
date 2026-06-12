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
    public ThirdPerson_CameraState(CameraSystem cameraManager, ThirdPersonCameraDataSO thirdPersonCameraData, CinemachineCamera camera)
    : base(cameraManager, "Third person camera", camera, thirdPersonCameraData.SimulationSpeed)
    {
        _cameraController = new(thirdPersonCameraData, camera);
    }
    #endregion

    #region INHERITED METHODS
    public override void StartState()
    {
        base.StartState();

        // Keep controller yaw/pitch in sync with the current target rotation
        _cameraController.SyncFromTargetRotation();

        // Lock cursor to screen center and hide it
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SyncToRotation(float pitch, float yaw)
    {
        _cameraController.SetTargetRotation(pitch, yaw);
    }

    public override void LateUpdateState()
    {
        if (_gameManager.IsInPauseState)
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
        if (_gameManager.IsInThirdPersonState
            && Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
    }

    public override void ExitState()
    {
        base.ExitState();

        // Unlock cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion
}
