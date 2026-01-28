using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    #region FIELDS
    readonly Dictionary<string, string> _loadedBySlots = new(); // Key: Slot ID, Value: Scene Name
    static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    bool _isLoading = false;

    // Dependency Injection
    UIManager _uiManager;
    #endregion

    #region SINGLETON
    public static ScenesController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _uiManager = ServiceLocator.Instance.Get<UIManager>();
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
            yield return _uiManager.FadeInLoadingScreen();
            yield return _waitForSeconds0_5;
        }

        foreach (string slotKey in plan.ScenesToUnload)
            yield return UnloadSceneRoutine(slotKey);

        if (plan.ClearUnusedAssets)
            yield return ClearUnusedAssetsRoutine();

        foreach (KeyValuePair<string, string> kvp in plan.ScenesToLoad)
        {
            if (_loadedBySlots.ContainsKey(kvp.Key))
                yield return UnloadSceneRoutine(kvp.Key);

            yield return LoadAdditiveSceneRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }

        if (plan.Overlay)
        {
            yield return _waitForSeconds0_5;
            yield return _uiManager.FadeOutLoadingScreen();
        }

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
    private IEnumerator UnloadSceneRoutine(string slotKey)
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
