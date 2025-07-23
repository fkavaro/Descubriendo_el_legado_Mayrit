using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class Spectator_CameraState : ACameraState
{
    readonly Transform _camera;
    readonly CameraController _cameraController;
    readonly SelectorCamera _selectorCamera;


    public Spectator_CameraState(FiniteStateMachine<CameraManager> stateMachine,
        Transform camera,
        Transform cameraTarget,
        AnimationCurve moveSpeedZoomCurve,
        LayerMask selectableLayer)
    : base("Spectator camera", stateMachine)
    {
        _camera = camera;
        _cameraController = new(camera, cameraTarget, moveSpeedZoomCurve);
        _selectorCamera = new(selectableLayer);
    }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Camera.Enable();

        _camera.gameObject.SetActive(true);

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