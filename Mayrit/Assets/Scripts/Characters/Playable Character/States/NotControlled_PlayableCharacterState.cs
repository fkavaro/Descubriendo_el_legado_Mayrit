using System;
using System.Collections.Generic;
using UnityEngine;

public class NotControlled_PlayableCharacterState : APlayableCharacterState
{
    public NotControlled_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Not controlled", playableCharacter) { }

    public override void StartState()
    {
        _playableCharacter.AnimationController.ChangeToIdle();
    }
}
