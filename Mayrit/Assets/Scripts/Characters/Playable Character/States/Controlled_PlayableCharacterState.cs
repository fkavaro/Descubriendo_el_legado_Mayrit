using System;
using System.Collections.Generic;
using UnityEngine;

public class Controlled_PlayableCharacterState : APlayableCharacterState
{
    public Controlled_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Free roam", playableCharacter) { }

    public override void UpdateState()
    {
        _playableCharacter._playerController.Update();
    }
}