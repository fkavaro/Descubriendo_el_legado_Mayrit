using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesController : MonoBehaviour
{
    #region PROPERTIES
    public event Action<Dictionary<SceneDatabase.Slot, SceneDatabase.SceneName>, List<SceneDatabase.Slot>> ScenesLoadedPartiallyEvent;
    public event Action<Dictionary<SceneDatabase.Slot, SceneDatabase.SceneName>, List<SceneDatabase.Slot>> ScenesLoadedFullyEvent;

    public string currentSession;
    public string currentMilestone;

    // Key: Slot ID, Value: Scene Name
    readonly Dictionary<SceneDatabase.Slot, SceneDatabase.SceneName> _loadedBySlots = new();
    static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    public bool IsLoading { get; private set; } = false;

    UIManager _uiManager;
    #endregion

    #region LYFE CYCLE

    private void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    void Start()
    {
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
        if (IsLoading)
        {
            Debug.LogWarning("A scene transition is already in progress.");
            return null;
        }

        return StartCoroutine(ChangeSceneRoutine(plan));
    }
    #endregion

    #region CHANGE SCENES
    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        IsLoading = true;
        if (plan.Overlay)
        {
            yield return _uiManager.FadeInLoadingScreenCoroutine();
            yield return _waitForSeconds0_5;
        }

        foreach (SceneDatabase.Slot slotKey in plan.SlotsToUnload)
            yield return UnloadSlotRoutine(slotKey);

        if (plan.ClearUnusedAssets)
            yield return ClearUnusedAssetsRoutine();

        foreach (KeyValuePair<SceneDatabase.Slot, SceneDatabase.SceneName> kvp in plan.ScenesToLoad)
        {
            Debug.Log($"ScenesController: Loading scene '{kvp.Value}' into slot '{kvp.Key}'.");
            if (_loadedBySlots.ContainsKey(kvp.Key))
                yield return UnloadSlotRoutine(kvp.Key);

            yield return LoadAdditiveSceneRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }

        ScenesLoadedPartiallyEvent?.Invoke(plan.ScenesToLoad, plan.SlotsToUnload);

        // Only after all scenes are loaded, update slots and fire events
        currentSession = _loadedBySlots.ContainsKey(SceneDatabase.Slot.Session)
                    ? _loadedBySlots[SceneDatabase.Slot.Session].ToString()
                    : "Slot ID not found";
        currentMilestone = _loadedBySlots.ContainsKey(SceneDatabase.Slot.Milestone)
                    ? _loadedBySlots[SceneDatabase.Slot.Milestone].ToString()
                    : "Slot ID not found";

        if (plan.Overlay)
        {
            yield return _waitForSeconds0_5;
            yield return _uiManager.FadeOutLoadingScreenCoroutine();
            Debug.Log("ScenesController: Finished scene transition with overlay.");
        }

        ScenesLoadedFullyEvent?.Invoke(plan.ScenesToLoad, plan.SlotsToUnload);

        IsLoading = false;
    }
    #endregion

    #region LOAD SCENE
    private IEnumerator LoadAdditiveSceneRoutine(SceneDatabase.Slot slotKey, SceneDatabase.SceneName sceneName, bool setActive)
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

        _loadedBySlots[slotKey] = sceneName;
    }
    #endregion

    #region UNLOAD SCENE
    private IEnumerator UnloadSlotRoutine(SceneDatabase.Slot slotKey)
    {
        if (!_loadedBySlots.TryGetValue(slotKey, out SceneDatabase.SceneName sceneName))
            yield break;

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName.ToString());

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
