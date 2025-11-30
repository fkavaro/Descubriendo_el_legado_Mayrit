using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPerson_CameraState : ACameraState
{
    public event Action ExitThirdPersonCameraEvent;

    readonly ThirdPersonCameraController _cameraController;

    public ThirdPerson_CameraState(CinemachineCamera camera, float simulationSpeed)
    : base("Third person camera", camera, simulationSpeed)
    {
        _cameraController = new(_camera);
    }

    public override void OnStateStarted()
    {
        GameManager.Instance.InputActions.Camera.Enable();
        GameManager.Instance.InputActions.Camera.ExitMode.performed += OnExitThirdPersonModePressed;
    }

    public override void LateUpdateState()
    {
        _cameraController.TargetSmoothFolow();

        // Only allow camera rotation if the playable character is being controlled
        if (GameManager.Instance.PlayableCharacter.IsBeingControlled)
            _cameraController.MouseTracking();
    }

    public override void OnStateExited()
    {
        GameManager.Instance.InputActions.Camera.Disable();
        GameManager.Instance.InputActions.Camera.ExitMode.performed -= OnExitThirdPersonModePressed;
    }

    void OnExitThirdPersonModePressed(InputAction.CallbackContext context)
    {
        ExitThirdPersonCameraEvent?.Invoke();
    }
}
