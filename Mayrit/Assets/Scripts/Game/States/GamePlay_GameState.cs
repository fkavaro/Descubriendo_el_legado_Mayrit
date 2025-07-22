using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlay_GameState : AGameState
{
    public GamePlay_GameState(FiniteStateMachine<GameManager> stateMachine)
    : base("Gameplay", stateMachine) { }

    public override void AwakeState()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
            SceneManager.LoadScene("GameScene");
    }

    public override void StartState()
    {

    }

    public override void UpdateState()
    {

    }
}
