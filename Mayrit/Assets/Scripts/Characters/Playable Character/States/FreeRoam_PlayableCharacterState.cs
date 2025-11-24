using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FreeRoam_PlayableCharacterState : APlayableCharacterState
{
    public FreeRoam_PlayableCharacterState(PlayableCharacter playableCharacter)
    : base("Free roam", playableCharacter)
    {
    }

    public override void StartState()
    {

    }

    public override void UpdateState()
    {
        _playableCharacter._playerController.Update();
    }
}