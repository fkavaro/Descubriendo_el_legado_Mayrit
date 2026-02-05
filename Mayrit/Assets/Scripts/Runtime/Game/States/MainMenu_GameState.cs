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
        // Load Main Menu Scene, if not already loaded
        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.MainMenuScene.ToString()).isLoaded)
            _scenesController.NewTransitionPlan()
                .Load(SceneDatabase.SceneType.Session, SceneDatabase.SceneName.MainMenuScene, setActive: true)
                .Unload(SceneDatabase.SceneType.Milestone) // Unload Milestone scene if it was loaded in a previous session
                .WithOverlay()
                .ClearAssets()
                .Perform();
    }
}
