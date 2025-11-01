using System;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class ThirdPerson_CameraState : ACameraState
{
    ThirdPersonCameraController _cameraController;

    public ThirdPerson_CameraState(FiniteStateMachine stateMachine,
        CinemachineCamera camera)
    : base("Third person camera", stateMachine, camera) { }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Player.Enable();
        GameManager.Instance._inputActions.Camera.ExitMode.Enable();
        GameManager.Instance._inputActions.Camera.ExitMode.performed += SwicthToSpectatorCamera;

        _camera.gameObject.SetActive(true);

        // Change HUD
        UIManager.Instance.BehaviourSystem.SwitchState(UIManager.Instance._playerHUDState);

        _cameraController = new(_camera);
    }

    public override void LateUpdateState()
    {
        _cameraController.LateUpdate();
    }

    public override void ExitState()
    {
        GameManager.Instance._inputActions.Player.Disable();
        GameManager.Instance._inputActions.Camera.ExitMode.Disable();
        GameManager.Instance._inputActions.Camera.ExitMode.performed -= SwicthToSpectatorCamera;
        _camera.gameObject.SetActive(false);
    }

    void SwicthToSpectatorCamera(InputAction.CallbackContext context)
    {
        CameraManager.Instance.SwitchToSpectatorCamera();
    }
}
