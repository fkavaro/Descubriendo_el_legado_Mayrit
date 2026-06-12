using System;
using UnityEngine;

public class Pause_GameState : AGameState
{
    public Pause_GameState(GameManager gameManager)
    : base(gameManager, "Pause") { }

    public override void StartState()
    {
        base.StartState();

        Time.timeScale = 0f;

        UISystem.SwitchToPauseState();
        // TODO: Disable player input and camera controls
        //CameraSystem.DisableCameraControllers();
        PlayableCharacter.SwitchToNotControlledState();

        _gameManager.InputActions.UI.Enable();
    }

    public override void ExitState()
    {
        Time.timeScale = _gameManager.SimulationSpeed;
        _gameManager.InputActions.UI.Disable();
    }
}
