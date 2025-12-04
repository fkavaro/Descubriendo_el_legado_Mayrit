using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class APlayableCharacterState : AState
{
    protected readonly PlayableCharacter _playableCharacter;
    protected readonly GameManager _gameManager;

    protected APlayableCharacterState(string name, PlayableCharacter playableCharacter)
    : base(name)
    {
        _playableCharacter = playableCharacter;

        // Get dependency from Service Locator
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        if (_gameManager == null)
            Debug.LogError("APlayableCharacterState: GameManager not found in ServiceLocator!");
    }
}
