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
        // Load Game Scene, if not already loaded
        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.GameplayScene.ToString()).isLoaded)
            _scenesController.NewTransitionPlan()
                .Load(SceneDatabase.SceneType.Session, SceneDatabase.SceneName.GameplayScene, setActive: true)
                .Load(SceneDatabase.SceneType.Milestone, SceneDatabase.SceneName.Milestone) // TODO: load restored milestone from local memory
                .WithOverlay()
                .ClearAssets()
                .Perform();
    }
}
