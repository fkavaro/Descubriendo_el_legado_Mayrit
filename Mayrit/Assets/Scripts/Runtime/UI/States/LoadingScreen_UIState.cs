using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class LoadingScreen_UIState : AUIState
{
    #region PROPERTIES
    public bool ContinueIsClicked { get; private set; }

    Label _header,
        _subHeader,
        _description,
        _imageCaption;

    Button _continueButton;

    VisualElement _infoLoadingScreen,
        _blackLoadingScreen,
        _image,
        _loadingAnimation;

    DataSO _milestoneData;
    #endregion

    #region CONSTRUCTOR
    public LoadingScreen_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("LoadingScreen", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _blackLoadingScreen = GetByName<VisualElement>("BlackLoadingScreen");
        _infoLoadingScreen = GetByName<VisualElement>("InfoLoadingScreen");
        _header = GetByName<Label>("Header", _infoLoadingScreen);
        _subHeader = GetByName<Label>("SubHeader", _infoLoadingScreen);
        _description = GetByName<Label>("Description", _infoLoadingScreen);
        _continueButton = GetButtonAndRegisterCallback("ContinueButton", OnContinueButtonClicked, _infoLoadingScreen);
        _image = GetByName<VisualElement>("Image", _infoLoadingScreen);
        _imageCaption = GetByName<Label>("Caption", _infoLoadingScreen);
        _loadingAnimation = GetByName<VisualElement>("LoadingAnimation", _infoLoadingScreen);
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
        _milestoneData = _progressManager.CurrentMilestoneData;

        if (_milestoneData != null)
        {
            // Update UI with milestone data
            _header.text = _milestoneData.Header;
            _subHeader.text = _milestoneData.SubHeader;
            _description.text = _milestoneData.Description;

            if (_milestoneData.Image != null)
            {
                _image.style.backgroundImage = new StyleBackground(_milestoneData.Image);
                _imageCaption.text = _milestoneData.ImageCaption;
                _image.style.display = DisplayStyle.Flex;
                _imageCaption.style.display = DisplayStyle.Flex;
            }
            else
            {
                _image.style.backgroundImage = new StyleBackground();
                _image.style.display = DisplayStyle.None;
                _imageCaption.style.display = DisplayStyle.None;
                _imageCaption.text = string.Empty;
            }
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

    public new IEnumerator FadeInCoroutine()
    {
        yield return BlackFadeInCoroutine();
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

    void OnContinueButtonClicked(ClickEvent evt)
    {
        ContinueIsClicked = true;
        _soundManager.PlayButtonClickSFX();
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
