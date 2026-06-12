using UnityEngine.SceneManagement;

public class LoadGame_GameState : AGameState
{
    public SceneDatabase.SceneName MilestoneToLoad;

    public LoadGame_GameState(GameManager gameManager)
    : base(gameManager, "LoadGame") { }

    public override void StartState()
    {
        base.StartState();

        if (!SceneManager.GetSceneByName(SceneDatabase.SceneName.GameplayScene.ToString()).isLoaded)
            _gameManager.LoadGame();
        else
            _gameManager.LoadMilestone(MilestoneToLoad);

        // Loading screen already in ScenesController
    }
}
