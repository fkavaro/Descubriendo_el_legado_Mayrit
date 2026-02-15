using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class LoadingScreen_UIState : AUIState
{
    #region PROPERTIES
    public bool ContinueIsClicked { get; private set; }

    readonly float _fadeInDuration;
    readonly float _fadeOutDuration;

    Label _header,
        _subHeader,
        _description;

    Button _continueButton;

    VisualElement _infoLoadingScreen,
        _blackLoadingScreen,
        _image,
        _loadingAnimation;

    DataSO _currentMilestone;
    #endregion

    #region CONSTRUCTOR
    public LoadingScreen_UIState(UIDocument uiDocument,
    float fadeInDuration = 0.5f,
    float fadeOutDuration = 0.5f)
    : base("LoadingScreen", uiDocument)
    {
        _fadeInDuration = fadeInDuration;
        _fadeOutDuration = fadeOutDuration;
    }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _infoLoadingScreen = _screen.Q<VisualElement>("InfoLoadingScreen");
        _blackLoadingScreen = _screen.Q<VisualElement>("BlackLoadingScreen");
        _header = _infoLoadingScreen.Q<Label>("Header");
        _subHeader = _infoLoadingScreen.Q<Label>("SubHeader");
        _description = _infoLoadingScreen.Q<Label>("Description");
        _continueButton = _infoLoadingScreen.Q<Button>("ContinueButton");
        _image = _infoLoadingScreen.Q<VisualElement>("Image");
        _loadingAnimation = _infoLoadingScreen.Q<VisualElement>("LoadingAnimation");

        if (_infoLoadingScreen == null)
            Debug.LogWarning("LoadingScreenController: No VisualElement with name 'LoadingScreen' found.");
        if (_header == null)
            Debug.LogWarning("LoadingScreenController: No Label with name 'Header' found.");
        if (_subHeader == null)
            Debug.LogWarning("LoadingScreenController: No Label with name 'SubHeader' found.");
        if (_description == null)
            Debug.LogWarning("LoadingScreenController: No Label with name 'Description' found.");
        if (_continueButton == null)
            Debug.LogWarning("LoadingScreenController: No Button with name 'ContinueButton' found.");
        if (_image == null)
            Debug.LogWarning("LoadingScreenController: No VisualElement with name 'Image' found.");
        if (_loadingAnimation == null)
            Debug.LogWarning("LoadingScreenController: No VisualElement with name 'LoadingAnimation' found.");
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _continueButton.clicked += OnContinueButtonClicked;
    }

    public override void StartState()
    {
        base.StartState();

        _infoLoadingScreen.style.opacity = 0f;
        _infoLoadingScreen.style.display = DisplayStyle.None;
        _blackLoadingScreen.style.opacity = 0f;
        _blackLoadingScreen.style.display = DisplayStyle.None;

        ContinueIsClicked = false;

        _loadingAnimation.style.display = DisplayStyle.Flex;
        _continueButton.style.display = DisplayStyle.None;

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;

        // Get current milestone data
        _currentMilestone = _progressManager.CurrentMilestoneMapping;

        if (_currentMilestone != null)
        {
            // Update UI with milestone data
            _header.text = _currentMilestone.Header;
            _subHeader.text = _currentMilestone.SubHeader;
            _description.text = _currentMilestone.Description;

            if (_currentMilestone.Image != null)
            {
                _image.style.backgroundImage = new StyleBackground(_currentMilestone.Image);
                _image.style.display = DisplayStyle.Flex;
            }
            // else
            // {
            //     _image.style.backgroundImage = new StyleBackground();
            //     _image.style.display = DisplayStyle.None;
            // }
        }
        else
        {
            Debug.LogWarning("LoadingScreenController: No current milestone data found.");
        }
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

    public IEnumerator FadeInCoroutine()
    {
        yield return BlackFadeInCoroutine();
        _infoLoadingScreen.style.display = DisplayStyle.Flex;
        yield return FadeToAlpha(_infoLoadingScreen, 1f, _fadeInDuration);
    }

    public IEnumerator FadeOutCoroutine()
    {
        yield return FadeToAlpha(_infoLoadingScreen, 0f, _fadeOutDuration);
        _infoLoadingScreen.style.display = DisplayStyle.None;
        yield return BlackFadeOutCoroutine();
    }

    IEnumerator FadeToAlpha(VisualElement visualElement, float targetAlpha, float duration)
    {
        if (visualElement.style.display == DisplayStyle.None)
            visualElement.style.display = DisplayStyle.Flex;

        if (visualElement.resolvedStyle.opacity == targetAlpha)
            Debug.LogWarning("FadeToAlpha called with targetAlpha equal to current alpha.");

        float startAlpha = visualElement.resolvedStyle.opacity;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            visualElement.style.opacity = newAlpha;
            yield return null;
        }
        visualElement.style.opacity = targetAlpha;
    }
    #endregion

    void OnContinueButtonClicked()
    {
        ContinueIsClicked = true;
    }

    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (type == SceneDatabase.SceneType.Milestone)
        {
            _loadingAnimation.style.display = DisplayStyle.None;
            _continueButton.style.display = DisplayStyle.Flex;
        }
    }
}
