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
        base.StartState();

        // Load Game Scene
        ScenesController.Instance.NewTransitionPlan()
            .Load(SceneDatabase.Slot.Session, SceneDatabase.Name.GamePlayScene)
            .Load(SceneDatabase.Slot.Milestone, SceneDatabase.Name.Milestone1, setActive: true)
            .WithOverlay()
            .ClearAssets()
            .Perform();
    }

    public override void ExitState()
    {
        base.ExitState();

        // Unload Game Scene
        ScenesController.Instance.NewTransitionPlan()
            .Unload(SceneDatabase.Slot.Session)
            .Unload(SceneDatabase.Slot.Milestone)
            .WithOverlay()
            .ClearAssets()
            .Perform();
    }
}
