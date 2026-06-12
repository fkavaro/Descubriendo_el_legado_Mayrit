using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class LoadingScreen_UIState : AUIState
{
    #region PROPERTIES
    readonly ContextualPanelComponent _contextualPanelComponent;

    public bool IsContinueClicked { get; private set; }

    VisualElement _infoLoadingScreen,
        _blackLoadingScreen;

    public Milestone_DataSO MilestoneData;
    #endregion

    #region CONSTRUCTOR
    public LoadingScreen_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration, ContextualPanelComponent contextualPanelComponent)
    : base(uiSystem, "LoadingScreen", uiDocument, fadeInDuration, fadeOutDuration)
    {
        _contextualPanelComponent = contextualPanelComponent;
    }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _blackLoadingScreen = GetByName<VisualElement>("BlackLoadingScreen");
        _infoLoadingScreen = GetByName<VisualElement>("InfoLoadingScreen");

        _contextualPanelComponent.ContinueClickedEvent += OnContinueButtonClicked;
    }

    public override void StartState()
    {
        base.StartState();

        _infoLoadingScreen.style.opacity = 0f;
        _infoLoadingScreen.style.display = DisplayStyle.None;
        _blackLoadingScreen.style.opacity = 0f;
        _blackLoadingScreen.style.display = DisplayStyle.None;

        IsContinueClicked = false;

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;

        ProgressManager progressManager = ServiceLocator.Instance.Get<ProgressManager>();

        // Get current milestone data
        MilestoneData = progressManager.CurrentMilestoneData;
    }

    public override void ExitState()
    {
        //base.ExitState(); Dont hide on exit, hide after FadeOutCoroutine
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
    }
    #endregion

    #region COROUTINES
    public IEnumerator BlackFadeInCoroutine()
    {
        _blackLoadingScreen.style.display = DisplayStyle.Flex;
        yield return FadeToAlpha(_blackLoadingScreen, 1f, _fadeInDuration);
    }

    public IEnumerator BlackFadeOutCoroutine()
    {
        yield return FadeToAlpha(_blackLoadingScreen, 0f, _fadeOutDuration);
        _blackLoadingScreen.style.display = DisplayStyle.None;
        base.ExitState(); // Hide after fade out is complete
    }

    public new IEnumerator FadeInCoroutine()
    {
        yield return BlackFadeInCoroutine();
        _contextualPanelComponent.ShowDataWhileLoading(MilestoneData);
        _infoLoadingScreen.style.display = DisplayStyle.Flex;
        yield return FadeToAlpha(_infoLoadingScreen, 1f, _fadeInDuration);
    }

    public new IEnumerator FadeOutCoroutine()
    {
        yield return FadeToAlpha(_infoLoadingScreen, 0f, _fadeOutDuration);
        _infoLoadingScreen.style.display = DisplayStyle.None;
        yield return BlackFadeOutCoroutine();
    }
    #endregion

    void OnContinueButtonClicked()
    {
        IsContinueClicked = true;
        _soundSystem.PlayButtonClickSFX();
        _contextualPanelComponent.ExitState();
    }

    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (type == SceneDatabase.SceneType.Milestone)
            _contextualPanelComponent.AfterLoading();
    }
}
