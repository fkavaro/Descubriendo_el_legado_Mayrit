using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_GameState : AGameState
{
    public MainMenu_GameState()
    : base("Main menu") { }

    public override void StartState()
    {
        if (SceneManager.GetActiveScene().name != "MainMenuScene")
            SceneManager.LoadScene("MainMenuScene");
    }
}
