using System;
using System.Collections.Generic;
using UnityEngine;

public class Controlled_PlayableCharacterState : APlayableCharacterState
{
    public Controlled_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Controlled", playableCharacter) { }

    public override void StartState()
    {
        GameManager.ExistingInstance.InputActions.Player.Enable();
    }

    public override void UpdateState()
    {
        _playableCharacter.MovementController.UpdateInputMovement();
    }

    public override void ExitState()
    {
        GameManager.ExistingInstance.InputActions.Player.Disable();
    }
}