using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    // Scenes loaded, slots unloaded
    public event Action<Dictionary<string, string>, List<string>> SceneChangedEvent;
    public event Action ShowLoadScreenEvent;

    public string currentSession;
    public string currentMilestone;

    #region FIELDS
    // Key: Slot ID, Value: Scene Name
    readonly Dictionary<string, string> _loadedBySlots = new();
    static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    bool _isLoading = false;
    #endregion

    #region LYFE CYCLE

    private void Awake()
    {
        // Only allow the registered ScenesController to initialize
        var registered = ServiceLocator.Instance.Get<ScenesController>();
        if (registered != null && registered != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register to Service Locator
        ServiceLocator.Instance.Register(this);
    }
    #endregion

    #region API
    public SceneTransitionPlan NewTransitionPlan()
    {
        return new SceneTransitionPlan();
    }

    public Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (_isLoading)
        {
            Debug.LogWarning("A scene transition is already in progress.");
            return null;
        }

        _isLoading = true;
        return StartCoroutine(ChangeSceneRoutine(plan));
    }
    #endregion

    #region CHANGE SCENES
    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay)
        {
            ShowLoadScreenEvent?.Invoke();
            yield return _waitForSeconds0_5;
        }

        foreach (string slotKey in plan.SlotsToUnload)
            yield return UnloadSlotRoutine(slotKey);

        if (plan.ClearUnusedAssets)
            yield return ClearUnusedAssetsRoutine();

        foreach (KeyValuePair<string, string> kvp in plan.ScenesToLoad)
        {
            if (_loadedBySlots.ContainsKey(kvp.Key))
                yield return UnloadSlotRoutine(kvp.Key);

            yield return LoadAdditiveSceneRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }

        // if (plan.Overlay)
        // {
        //     yield return _waitForSeconds0_5;
        // }

        currentSession = _loadedBySlots.ContainsKey(SceneDatabase.Slot.Session)
                    ? _loadedBySlots[SceneDatabase.Slot.Session]
                    : "Slot ID not found";
        currentMilestone = _loadedBySlots.ContainsKey(SceneDatabase.Slot.Milestone)
                    ? _loadedBySlots[SceneDatabase.Slot.Milestone]
                    : "Slot ID not found";

        SceneChangedEvent?.Invoke(plan.ScenesToLoad, plan.SlotsToUnload);

        _isLoading = false;
    }
    #endregion

    #region LOAD SCENE
    private IEnumerator LoadAdditiveSceneRoutine(string slotKey, string sceneName, bool setActive)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) yield break;

        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        if (setActive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
                SceneManager.SetActiveScene(loadedScene);
        }

        _loadedBySlots[slotKey] = sceneName;
    }
    #endregion

    #region UNLOAD SCENE
    private IEnumerator UnloadSlotRoutine(string slotKey)
    {
        if (!_loadedBySlots.TryGetValue(slotKey, out string sceneName))
            yield break;

        if (string.IsNullOrEmpty(sceneName))
            yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);

        if (unloadOp != null)
        {
            while (!unloadOp.isDone)
                yield return null;
        }

        _loadedBySlots.Remove(slotKey);
    }
    #endregion

    #region CLEAR UNUSED ASSETS
    private IEnumerator ClearUnusedAssetsRoutine()
    {
        AsyncOperation clearOp = Resources.UnloadUnusedAssets();

        if (clearOp != null)
        {
            while (!clearOp.isDone)
                yield return null;
        }
    }
    #endregion
}
