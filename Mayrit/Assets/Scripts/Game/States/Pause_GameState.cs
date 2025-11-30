using System;
using UnityEngine;

public class Pause_GameState : AGameState
{
    public event Action<bool> GamePausedEvent;

    public Pause_GameState()
    : base("Pause") { }

    public override void StartState()
    {
        Time.timeScale = 0f;

        GamePausedEvent?.Invoke(true);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        Time.timeScale = 1f;

        GamePausedEvent?.Invoke(false);
    }
}
