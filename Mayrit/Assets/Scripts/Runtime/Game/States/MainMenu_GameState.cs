using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_GameState : AGameState
{
    public MainMenu_GameState(GameManager gameManager)
    : base(gameManager, "Main menu") { }

    public override void StartState()
    {
        base.StartState();

        _gameManager.ScenesController.ScenesLoadedFullyEvent += OnScenesLoadedFully;
        _gameManager.LoadMainMenuScene();
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.ScenesController.ScenesLoadedFullyEvent -= OnScenesLoadedFully;
    }

    private void OnScenesLoadedFully(Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> loadedScenes, List<SceneDatabase.SceneType> unloadedScenes)
    {
        if (loadedScenes.TryGetValue(SceneDatabase.SceneType.Session, out var scene)
            && scene == SceneDatabase.SceneName.MainMenuScene)
        {
            UISystem.SwitchToMainMenuState();
        }
    }
}
