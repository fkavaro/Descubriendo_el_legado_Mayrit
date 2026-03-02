using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

public abstract class AUIState : AState
{
    #region PROPERTIES
    readonly protected UIDocument _UIDocument;
    readonly protected float _fadeInDuration;
    readonly protected float _fadeOutDuration;

    VisualElement _screen;

    // Dependency Injection
    protected ScenesController _scenesController;
    protected UIManager _uiManager;
    protected GameManager _gameManager;
    protected SoundManager _soundManager;
    protected ProgressManager _progressManager;
    #endregion

    #region CONSTRUCTOR
    protected AUIState(string name, UIDocument uiDocument, float fadeInDuration = 0f, float fadeOutDuration = 0f)
    : base(name)
    {
        _UIDocument = uiDocument;
        _fadeInDuration = fadeInDuration;
        _fadeOutDuration = fadeOutDuration;
    }
    #endregion

    #region INHERITED METHODS
    public override void AwakeState()
    {
        if (_UIDocument == null)
        {
            Debug.LogError($"{_stateName} UI State: UIDocument is null!");
            return;
        }

        if (_UIDocument.rootVisualElement == null)
        {
            Debug.LogWarning($"{_stateName} UI State: UIDocument rootVisualElement is null!");
            return;
        }

        _screen = GetByName<VisualElement>(_stateName, _UIDocument.rootVisualElement);
        _screen.style.display = DisplayStyle.None;

        ConfigureUIElementsOnAwake();
    }

    protected override void GetServicesDependenciesOnStart()
    {
        if (_scenesController == null)
            _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        if (_uiManager == null)
            _uiManager = ServiceLocator.Instance.Get<UIManager>();
        if (_gameManager == null)
            _gameManager = ServiceLocator.Instance.Get<GameManager>();
        if (_soundManager == null)
            _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        if (_progressManager == null)
            _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
    }

    public override void StartState()
    {
        if (_screen == null)
        {
            Debug.LogWarning($"{_stateName}: Cannot start state because screen VisualElement is null.");
            return;
        }

        _screen.style.display = DisplayStyle.Flex;
        base.StartState();
    }

    public override void ExitState()
    {
        base.ExitState();
        _screen.style.display = DisplayStyle.None;
    }
    #endregion

    #region PUBLIC METHODS
    public virtual bool IsCursorOverUI()
    {
        return IsCursorOver(_screen);
    }
    #endregion

    #region PRIVATE METHODS
    protected T GetByName<T>(string elementName, VisualElement parent = null) where T : VisualElement
    {
        parent ??= _screen;

        if (parent is null)
        {
            Debug.LogWarning($"{_stateName} UI State: Cannot search for '{elementName}' in null parent.");
            return null;
        }

        T element = parent.Q<T>(elementName);
        if (element is null)
            Debug.LogWarning($"{_stateName}: No VisualElement with name '{elementName}' found.");

        return element;
    }

    protected Button GetButtonAndRegisterCallback(string buttonName, EventCallback<ClickEvent> callbackMethod, VisualElement parent = null)
    {
        if (GetByName<Button>(buttonName, parent) is not Button button)
            return null;

        button.RegisterCallback(callbackMethod);

        return button;
    }

    protected Switch GetSwitchAndRegisterCallback(string switchName, System.Action<bool> callbackMethod, VisualElement parent = null)
    {
        if (GetByName<Switch>(switchName, parent) is not Switch switchElement)
            return null;

        switchElement.Toggled += callbackMethod;

        return switchElement;
    }

    protected Slider GetSliderAndRegisterCallback(string sliderName, EventCallback<ChangeEvent<float>> callbackMethod, VisualElement parent = null)
    {
        if (GetByName<Slider>(sliderName, parent) is not Slider slider)
            return null;

        slider.RegisterCallback(callbackMethod);

        return slider;
    }

    protected bool IsCursorOver(VisualElement uiElement)
    {
        // Get the current mouse position
        Vector2 cursorPos = Mouse.current.position.ReadValue();

        // Convert to UI Toolkit coordinates (Y is flipped)
        Vector2 panelPosition = new(cursorPos.x, Screen.height - cursorPos.y);

        // Pick the topmost VisualElement at the given position
        var panel = uiElement.panel;
        VisualElement pickedElement = panel?.Pick(panelPosition);

        // Check if the picked element is a descendant of element (and not element itself)
        if (pickedElement != null && pickedElement != uiElement && uiElement.Contains(pickedElement))
            return true;

        return false;
    }
    #endregion

    #region ABSTRACT METHODS
    protected abstract void ConfigureUIElementsOnAwake();
    #endregion

    #region COROUTINES
    public virtual IEnumerator FadeInCoroutine()
    {
        _screen.style.opacity = 0f;
        _screen.style.display = DisplayStyle.Flex;
        yield return FadeToAlpha(_screen, 1f, _fadeInDuration);
    }

    public virtual IEnumerator FadeOutCoroutine()
    {
        yield return FadeToAlpha(_screen, 0f, _fadeOutDuration);
        _screen.style.display = DisplayStyle.None;
    }

    public virtual IEnumerator FadeInCoroutine(VisualElement visualElement)
    {
        visualElement.style.opacity = 0f;
        visualElement.style.display = DisplayStyle.Flex;
        yield return FadeToAlpha(visualElement, 1f, _fadeInDuration);
    }

    public virtual IEnumerator FadeOutCoroutine(VisualElement visualElement)
    {
        yield return FadeToAlpha(visualElement, 0f, _fadeOutDuration);
        visualElement.style.display = DisplayStyle.None;
    }

    protected IEnumerator FadeToAlpha(VisualElement visualElement, float targetAlpha, float duration)
    {
        if (duration <= 0f)
        {
            visualElement.style.opacity = targetAlpha;
            yield break;
        }

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

        Debug.Log($"{_stateName}: Finished fade animation to {targetAlpha} alpha for {visualElement.name}.");
    }
    #endregion
}
