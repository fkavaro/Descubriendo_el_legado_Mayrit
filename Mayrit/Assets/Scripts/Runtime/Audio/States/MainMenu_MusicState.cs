using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MainMenu_MusicState : AMusicState
{
    public MainMenu_MusicState(SoundController soundController)
    : base("Main menu", soundController, SoundDatabase.MusicType.MenuMusic) { }
}
