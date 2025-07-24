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
    : base("Third person", stateMachine, camera) { }

    public override void StartState()
    {
        GameManager.Instance._inputActions.Player.Enable();
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
