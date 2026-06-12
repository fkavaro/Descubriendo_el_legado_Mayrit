using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    #region PROPERTIES
    public event Action<SceneDatabase.SceneType, SceneDatabase.SceneName> SceneLoadedPartiallyEvent;
    public event Action<Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName>, List<SceneDatabase.SceneType>> ScenesLoadedFullyEvent;

    [SerializeField] bool _debugMode = false;
    [SerializeField] string currentSession;
    [SerializeField] string currentMilestone;
    [SerializeField] bool _isLoading = false;

    // Key: Scene Type, Value: Scene Name
    readonly Dictionary<SceneDatabase.SceneType, SceneDatabase.SceneName> _loadedByType = new();
    static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);

    UISystem _uiSystem;
    #endregion

    #region LYFE CYCLE

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
        _uiSystem = ServiceLocator.Instance.Get<UISystem>();
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
            if (_debugMode)
                Debug.LogWarning("A scene transition is already in progress.");
            return null;
        }

        return StartCoroutine(ChangeSceneRoutine(plan));
    }
    #endregion

    #region CHANGE SCENES
    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        _isLoading = true;
        if (plan.Overlay)
        {
            // Black if loading main menu
            if (plan.ScenesToLoad.ContainsValue(SceneDatabase.SceneName.MainMenuScene))
                yield return _uiSystem.FadeInBlackLoadingScreenCoroutine();
            else
                yield return _uiSystem.FadeInLoadingScreenCoroutine();

            yield return _waitForSeconds0_5;
        }

        foreach (SceneDatabase.SceneType type in plan.TypesToUnload)
            yield return UnloadTypeRoutine(type);

        if (plan.ClearUnusedAssets)
            yield return ClearUnusedAssetsRoutine();

        foreach (KeyValuePair<SceneDatabase.SceneType, SceneDatabase.SceneName> kvp in plan.ScenesToLoad)
        {
            if (_loadedByType.ContainsKey(kvp.Key))
                yield return UnloadTypeRoutine(kvp.Key);

            yield return LoadAdditiveSceneRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);

            if (_debugMode)
                Debug.Log($"[ScenesController] Scene '{kvp.Value}' loaded into type '{kvp.Key}'.");
            SceneLoadedPartiallyEvent?.Invoke(kvp.Key, kvp.Value);
        }

        // Only after all scenes are loaded, update types and fire events
        currentSession = _loadedByType.ContainsKey(SceneDatabase.SceneType.Session)
                    ? _loadedByType[SceneDatabase.SceneType.Session].ToString()
                    : "None";
        currentMilestone = _loadedByType.ContainsKey(SceneDatabase.SceneType.Milestone)
                    ? _loadedByType[SceneDatabase.SceneType.Milestone].ToString()
                    : "None";

        if (plan.Overlay)
        {
            yield return _waitForSeconds0_5;

            if (plan.ScenesToLoad.ContainsValue(SceneDatabase.SceneName.MainMenuScene))
                yield return _uiSystem.FadeOutBlackLoadingScreenCoroutine();
            else
                yield return _uiSystem.FadeOutLoadingScreenCoroutine();
        }

        ScenesLoadedFullyEvent?.Invoke(plan.ScenesToLoad, plan.TypesToUnload);

        _isLoading = false;
    }
    #endregion

    #region LOAD SCENE
    private IEnumerator LoadAdditiveSceneRoutine(SceneDatabase.SceneType type, SceneDatabase.SceneName sceneName, bool setActive)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
        if (loadOp == null) yield break;

        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        if (setActive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName.ToString());
            if (loadedScene.IsValid() && loadedScene.isLoaded)
                SceneManager.SetActiveScene(loadedScene);
        }

        _loadedByType[type] = sceneName;
    }
    #endregion

    #region UNLOAD SCENE
    private IEnumerator UnloadTypeRoutine(SceneDatabase.SceneType type)
    {
        if (!_loadedByType.TryGetValue(type, out SceneDatabase.SceneName sceneName))
            yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName.ToString());

        if (unloadOp != null)
            while (!unloadOp.isDone)
                yield return null;

        _loadedByType.Remove(type);
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
