using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class LoadingScreen_UIState : AUIState
{
    VisualElement _loadingScreenElement;
    readonly float _fadeInDuration;
    readonly float _fadeOutDuration;

    public LoadingScreen_UIState(UIDocument uiDocument,
    float fadeInDuration = 0.5f,
    float fadeOutDuration = 0.5f)
    : base("LoadingScreen", uiDocument)
    {
        _fadeInDuration = fadeInDuration;
        _fadeOutDuration = fadeOutDuration;
    }

    protected override void ConfigureUIElementsOnAwake()
    {
        _loadingScreenElement = _UIDocument.rootVisualElement.Q<VisualElement>("LoadingScreen");
        if (_loadingScreenElement == null)
            Debug.LogWarning("LoadingScreenController: No VisualElement with name 'LoadingScreen' found.");
    }

    protected override void RegisterUICallbacksOnAwake()
    {

    }

    public override void StartState()
    {
        base.StartState();

        _uiManager.StartCoroutine(FadeInCoroutine());
    }

    public override void ExitState()
    {
        base.ExitState();

        _uiManager.StartCoroutine(FadeOutCoroutine());
    }

    #region COROUTINES
    IEnumerator FadeInCoroutine()
    {
        yield return FadeToAlpha(1f, _fadeInDuration);
    }

    IEnumerator FadeOutCoroutine()
    {
        yield return FadeToAlpha(0f, _fadeOutDuration);
    }

    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        if (_loadingScreenElement == null)
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
            _loadingScreenElement.style.opacity = newAlpha;
            yield return null;
        }
        _loadingScreenElement.style.opacity = targetAlpha;


    }
    #endregion

    #region HELPERS
    private float GetCurrentAlpha()
    {
        if (_loadingScreenElement.resolvedStyle.opacity >= 0f)
            return _loadingScreenElement.resolvedStyle.opacity;
        return 0f;
    }
    #endregion
}
