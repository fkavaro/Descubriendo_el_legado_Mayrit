using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Pause_GameState : AGameState
{
    public Pause_GameState()
    : base("Pause") { }

    public override void StartState()
    {
        Time.timeScale = 0f;
        GameManager.Instance._inputActions.Camera.Disable();
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        Time.timeScale = 1f;
        GameManager.Instance._inputActions.Camera.Enable();
    }
}
