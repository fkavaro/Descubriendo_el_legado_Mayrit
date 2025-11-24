using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class APlayableCharacterState : AState
{
    protected readonly PlayableCharacter _playableCharacter;

    protected APlayableCharacterState(string name, PlayableCharacter playableCharacter)
    : base(name)
    {
        _playableCharacter = playableCharacter;
    }
}
