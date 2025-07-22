using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Pause_GameState : AState<GameManager, FiniteStateMachine<GameManager>>
{
    public Pause_GameState(FiniteStateMachine<GameManager> stateMachine)
    : base("Pause", stateMachine) { }

    public override void StartState()
    {
        Time.timeScale = 0f;
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        Time.timeScale = 1f;
    }
}
