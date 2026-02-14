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

    VisualElement _root,
        _image;

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
        _root = _UIDocument.rootVisualElement.Q<VisualElement>("LoadingScreen");

        _header = _root.Q<Label>("Header");
        _subHeader = _root.Q<Label>("SubHeader");
        _description = _root.Q<Label>("Description");
        _continueButton = _root.Q<Button>("ContinueButton");
        _image = _root.Q<VisualElement>("Image");


        if (_root == null)
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
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _continueButton.clicked += OnContinueButtonClicked;
    }

    public override void StartState()
    {
        ContinueIsClicked = false;
        _root.style.opacity = 0f;
        _continueButton.text = "Cargando...";
        _continueButton.SetEnabled(false);

        base.StartState();

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

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

    private void OnMilestoneChanged(Milestone_DataSO sO)
    {

    }

    public override void ExitState()
    {
        //base.ExitState(); Dont hide on exit, hide after FadeOutCoroutine
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
    }
    #endregion

    #region COROUTINES
    public IEnumerator FadeInCoroutine()
    {
        yield return FadeToAlpha(1f, _fadeInDuration);
    }

    public IEnumerator FadeOutCoroutine()
    {
        yield return FadeToAlpha(0f, _fadeOutDuration);
        base.ExitState(); // Hide after fade out is complete
    }

    IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        if (_root == null)
        {
            Debug.LogWarning("LoadingScreenController: No VisualElement assigned.");
            yield break;
        }

        float startAlpha = GetCurrentAlpha();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            _root.style.opacity = newAlpha;
            yield return null;
        }
        _root.style.opacity = targetAlpha;
    }
    #endregion

    #region HELPERS
    private float GetCurrentAlpha()
    {
        if (_root.resolvedStyle.opacity >= 0f)
            return _root.resolvedStyle.opacity;
        return 0f;
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
            _continueButton.text = "Continuar";
            _continueButton.SetEnabled(true);
        }
    }
}
