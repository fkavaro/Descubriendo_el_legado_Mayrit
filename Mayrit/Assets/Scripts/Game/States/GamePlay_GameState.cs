using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePlay_GameState : AGameState
{
    public GamePlay_GameState()
    : base("Gameplay") { }

    public override void StartState()
    {
        if (SceneManager.GetActiveScene().name != "GameScene")
            SceneManager.LoadScene("GameScene");
    }
}
