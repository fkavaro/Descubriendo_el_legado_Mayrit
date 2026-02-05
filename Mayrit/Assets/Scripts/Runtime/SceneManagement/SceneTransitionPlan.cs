using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPlan
{
    public Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> ScenesToLoad { get; } = new();
    public List<SceneDatabase.SceneType> TypesToUnload { get; } = new();
    public SceneDatabase.SceneName ActiveSceneName { get; private set; }
    public bool ClearUnusedAssets { get; private set; } = false;
    public bool Overlay { get; private set; } = false;

    public SceneTransitionPlan Load(SceneDatabase.SceneType type, SceneDatabase.SceneName sceneName, bool setActive = false)
    {
        ScenesToLoad[type] = sceneName;
        if (setActive)
            ActiveSceneName = sceneName;
        return this;
    }

    public SceneTransitionPlan Unload(SceneDatabase.SceneType type)
    {
        TypesToUnload.Add(type);
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
