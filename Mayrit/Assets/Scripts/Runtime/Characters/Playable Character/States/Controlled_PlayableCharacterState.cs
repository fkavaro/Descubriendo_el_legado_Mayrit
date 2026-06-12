using System;
using System.Collections.Generic;
using UnityEngine;

public class Controlled_PlayableCharacterState : APlayableCharacterState
{
    public Controlled_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Controlled", playableCharacter) { }

    public override void UpdateState()
    {
        if (_gameManager.IsInPauseState)
            return;

        _playableCharacter.MovementController.UpdateInputMovement();
    }
}