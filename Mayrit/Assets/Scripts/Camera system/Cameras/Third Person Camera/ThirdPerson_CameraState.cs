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

    public override void StartState()
    {
        GameManager.Instance.InputActions.Player.Enable();
        GameManager.Instance.InputActions.Camera.ExitMode.Enable();
        GameManager.Instance.InputActions.Camera.ExitMode.performed += OnExitThirdPersonModePressed;

        _camera.gameObject.SetActive(true);

        // Adjust simulation speed
        TimeManager.Instance.SetSimulationSpeed(_simulationSpeed);
    }

    public override void LateUpdateState()
    {
        // Don't update camera if player is not being controlled
        if (!GameManager.Instance.PlayableCharacter.IsBeingControlled)
            return;

        _cameraController.Update();
    }

    public override void ExitState()
    {
        GameManager.Instance.InputActions.Player.Disable();
        GameManager.Instance.InputActions.Camera.ExitMode.Disable();
        GameManager.Instance.InputActions.Camera.ExitMode.performed -= OnExitThirdPersonModePressed;
        _camera.gameObject.SetActive(false);
    }

    void OnExitThirdPersonModePressed(InputAction.CallbackContext context)
    {
        ExitThirdPersonCameraEvent?.Invoke();
    }
}
