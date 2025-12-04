using System;
using System.Collections.Generic;
using UnityEngine;

public class Controlled_PlayableCharacterState : APlayableCharacterState
{
    public Controlled_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Controlled", playableCharacter) { }

    public override void StartState()
    {
        _gameManager.InputActions.Player.Enable();
    }

    public override void UpdateState()
    {
        _playableCharacter.MovementController.UpdateInputMovement();
    }

    public override void ExitState()
    {
        _gameManager.InputActions.Player.Disable();
    }
}