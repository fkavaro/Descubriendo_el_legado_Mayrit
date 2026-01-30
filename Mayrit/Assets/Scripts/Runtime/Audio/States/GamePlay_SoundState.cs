using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GamePlay_MusicState : AMusicState
{
    public GamePlay_MusicState(SoundController soundController)
    : base("Gameplay", soundController, SoundDatabase.MusicType.GameplayMusic) { }
}
