using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class AGameState : AState
{
    protected GameManager _gameManager;
    protected UISystem UISystem => _gameManager.UISystem;
    protected CameraSystem CameraSystem => _gameManager.CameraSystem;
    protected PlayableCharacter PlayableCharacter => _gameManager.PlayableCharacter;

    protected AGameState(GameManager gameManager, string name)
    : base(name)
    {
        _gameManager = gameManager;
    }
}
