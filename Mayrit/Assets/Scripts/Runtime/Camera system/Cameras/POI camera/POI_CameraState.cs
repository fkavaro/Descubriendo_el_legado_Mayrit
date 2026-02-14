using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class POI_CameraState : ACameraState
{
    public POI_CameraState(float simulationSpeed)
    : base("POI camera", null, simulationSpeed) { }

    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.Camera.Enable();
        _gameManager.InputActions.Camera.ExitMode.performed += OnExitCameraMode;
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.Camera.Disable();
        _gameManager.InputActions.Camera.ExitMode.performed -= OnExitCameraMode;
    }

    void OnExitCameraMode(InputAction.CallbackContext context)
    {
        _cameraManager.SwitchToThirdPersonCamera();
        _uiManager.HideContextualPanel();
    }
}
