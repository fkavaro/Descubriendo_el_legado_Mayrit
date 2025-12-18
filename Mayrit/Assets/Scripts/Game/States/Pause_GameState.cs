using System;
using UnityEngine;

public class Pause_GameState : AGameState
{
    public event Action<bool> GamePausedEvent;

    // Dependency Injection
    protected TimeManager _timeManager;

    public Pause_GameState()
    : base("Pause") { }

    protected override void GetServicesDependenciesOnStart()
    {
        _timeManager = ServiceLocator.Instance.Get<TimeManager>();
    }

    public override void StartState()
    {
        base.StartState();

        Time.timeScale = 0f;

        GamePausedEvent?.Invoke(true);
    }

    public override void ExitState()
    {
        Time.timeScale = _timeManager.SimulationSpeed;
    }
}
