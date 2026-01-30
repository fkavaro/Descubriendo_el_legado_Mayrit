using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPlan
{
    public Dictionary<string, string> ScenesToLoad { get; } = new();
    public List<string> SlotsToUnload { get; } = new();
    public string ActiveSceneName { get; private set; } = "";
    public bool ClearUnusedAssets { get; private set; } = false;
    public bool Overlay { get; private set; } = false;

    public SceneTransitionPlan Load(string slotId, string sceneName, bool setActive = false)
    {
        ScenesToLoad[slotId] = sceneName;
        if (setActive)
            ActiveSceneName = sceneName;
        return this;
    }

    public SceneTransitionPlan Unload(string slotKey)
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
