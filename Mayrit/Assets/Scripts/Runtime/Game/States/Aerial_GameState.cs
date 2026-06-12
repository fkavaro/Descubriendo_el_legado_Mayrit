using System;
using UnityEngine;
public class Aerial_GameState : AGameState
{
    public Aerial_GameState(GameManager gameManager)
    : base(gameManager, "Aerial") { }

    public override void StartState()
    {
        base.StartState();

        UISystem.SwitchToAerialHUDState();
        CameraSystem.SwitchToAerialCamera();
        PlayableCharacter.SwitchToNotControlledState();

        _gameManager.InputActions.UI.Enable();
        _gameManager.InputActions.Camera.Enable();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.Camera.Disable();
    }
}