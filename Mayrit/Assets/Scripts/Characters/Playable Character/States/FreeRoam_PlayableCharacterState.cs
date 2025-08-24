using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class FreeRoam_PlayableCharacterState : APlayableCharacterState
{
    public FreeRoam_PlayableCharacterState(FiniteStateMachine stateMachine, PlayableCharacter playableCharacter, CharacterController playerCharacterController)
    : base("Free roam", stateMachine, playableCharacter, playerCharacterController)
    {
    }

    public override void StartState()
    {

    }

    public override void UpdateState()
    {
        _playerController.Update();
    }
}