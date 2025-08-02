using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cinemachine;

public class ThirdPerson_CameraState : ACameraState
{
    public ThirdPerson_CameraState(FiniteStateMachine<CameraManager> stateMachine,
        CinemachineCamera camera)
    : base("Third person camera", stateMachine, camera) { }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Player.Enable();

        var playerTransform = GameManager.Instance.GetCurrentPlayableCharacter().transform;

        // TODO this should work
        // Set camera follow and look at targets
        _camera.Follow = playerTransform;
        _camera.LookAt = playerTransform;

        Debug.Log("Camera Follow set to: " + _camera.Follow?.name);
        Debug.Log("Camera LookAt set to: " + _camera.LookAt?.name);

        _camera.gameObject.SetActive(true);

        // Change HUD
        UIManager.Instance._fsm.SwitchState(UIManager.Instance._playerHUDState);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        GameManager.Instance._inputActions.Player.Disable();
        _camera.gameObject.SetActive(false);
    }
}
