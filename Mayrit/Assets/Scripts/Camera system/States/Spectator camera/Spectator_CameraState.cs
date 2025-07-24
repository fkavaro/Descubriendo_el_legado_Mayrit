using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    readonly CameraController _cameraController;
    readonly SelectorCamera _selectorCamera;

    public Spectator_CameraState(FiniteStateMachine<CameraManager> stateMachine,
        CinemachineCamera camera,
        AnimationCurve moveSpeedZoomCurve,
        LayerMask selectableLayer)
    : base("Spectator camera", stateMachine, camera)
    {
        _cameraController = new(camera, moveSpeedZoomCurve);
        _selectorCamera = new(selectableLayer);
    }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Camera.Enable();
        _camera.gameObject.SetActive(true);

        // Change HUD
        UIManager.Instance._fsm.SwitchState(UIManager.Instance._spectatorHUDState);

        _cameraController.Start();
        _selectorCamera.Start();
    }

    public override void UpdateState()
    {
        _cameraController.Update();
        _selectorCamera.Update();
    }

    public override void ExitState()
    {
        GameManager.Instance._inputActions.Camera.Disable();
        _camera.gameObject.SetActive(false);
    }
}