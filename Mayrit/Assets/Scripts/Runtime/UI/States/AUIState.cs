using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public abstract class AUIState : AState
{
    #region PROPERTIES
    public UIDocument _UIDocument;
    public VisualElement _screen;

    // Dependency Injection
    protected ScenesController _scenesController;
    protected UIManager _uiManager;
    protected GameManager _gameManager;
    protected SoundManager _soundManager;
    protected ProgressManager _progressManager;
    #endregion

    #region CONSTRUCTOR
    protected AUIState(string name, UIDocument uiDocument)
    : base(name)
    {
        _UIDocument = uiDocument;
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

        _screen = _UIDocument.rootVisualElement.Q<VisualElement>(_stateName);

        if (_screen == null)
        {
            Debug.LogWarning($"{_stateName} UI State: No VisualElement with name '{_stateName}' found.");
            return;
        }

        ConfigureUIElementsOnAwake();
        RegisterUICallbacksOnAwake();
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
            Debug.LogWarning($"{_stateName} UI State: _screen is null.");
            return;
        }

        _screen.style.display = DisplayStyle.Flex; // Show
        base.StartState();
    }

    public override void ExitState()
    {
        base.ExitState();
        _screen.style.display = DisplayStyle.None; // Hide
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Returns true if the cursor is over any UI element that is a descendant of _screen.
    /// </summary>
    /// <param name="cursorPos">Screen-space position of the cursor (Input.mousePosition).</param>
    public virtual bool IsCursorOverUI()
    {
        return IsCursorOver(_screen);
    }
    #endregion

    #region PRIVATE METHODS
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
    protected abstract void RegisterUICallbacksOnAwake();
    #endregion
}
