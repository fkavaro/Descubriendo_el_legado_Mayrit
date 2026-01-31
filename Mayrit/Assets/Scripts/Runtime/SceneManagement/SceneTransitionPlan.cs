using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPlan
{
    public Dictionary<SceneDatabase.Slot, SceneDatabase.SceneName> ScenesToLoad { get; } = new();
    public List<SceneDatabase.Slot> SlotsToUnload { get; } = new();
    public SceneDatabase.SceneName ActiveSceneName { get; private set; }
    public bool ClearUnusedAssets { get; private set; } = false;
    public bool Overlay { get; private set; } = false;

    public SceneTransitionPlan Load(SceneDatabase.Slot slotId, SceneDatabase.SceneName sceneName, bool setActive = false)
    {
        ScenesToLoad[slotId] = sceneName;
        if (setActive)
            ActiveSceneName = sceneName;
        return this;
    }

    public SceneTransitionPlan Unload(SceneDatabase.Slot slotKey)
    {
        SlotsToUnload.Add(slotKey);
        return this;
    }

    public SceneTransitionPlan WithOverlay()
    {
        Overlay = true;
        return this;
    }

    public SceneTransitionPlan ClearAssets()
    {
        ClearUnusedAssets = true;
        return this;
    }

    public Coroutine Perform()
    {
        ScenesController scenesController = ServiceLocator.Instance.Get<ScenesController>();
        return scenesController.ExecutePlan(this);
    }
}
