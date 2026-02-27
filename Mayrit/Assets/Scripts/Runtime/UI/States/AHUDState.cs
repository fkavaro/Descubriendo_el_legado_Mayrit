using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class AHUDState : AUIState
{
    #region PROPERTIES
    public event Action PlayTourEvent;
    public event Action ResetTourEvent;
    public event Action ContextualPanelHiddenEvent;

    protected ContextualPanel _contextualPanel;
    protected CompassUI _compass;
    protected bool _wasContextualPanelShown;

    VisualElement _hudScreen,
        _contextualPanelRoot,
        _controlsVisualRoot,
        _compassVisualRoot;
    #endregion

    #region CONSTRUCTOR
    public AHUDState(string name, UIDocument uiDocument)
    : base(name, uiDocument) { }
    #endregion

    #region INHERITED METHODS
    public override void AwakeState()
    {
        _hudScreen = _UIDocument.rootVisualElement.Q<VisualElement>("HUD");

        if (_hudScreen == null)
        {
            Debug.LogWarning($"{_stateName} HUD element not found in UIDocument");
        }

        if (_contextualPanelRoot == null)
            InitializeContextualPanelOnAwake();

        if (_contextualPanel == null)
        {
            Debug.LogWarning($"{_stateName}: Contextual Panel is null!");
            return;
        }

        if (_compassVisualRoot == null)
            InitializeCompassOnAwake();

        if (_compass == null)
        {
            Debug.LogWarning($"{_stateName}: Compass is null!");
            return;
        }

        base.AwakeState();
    }

    protected override void ConfigureUIElementsOnAwake()
    {
        _controlsVisualRoot = _screen.Q<VisualElement>("ControlsVisual");

        if (_controlsVisualRoot == null)
            Debug.LogWarning($"{_stateName} HUD State: _controlsVisualRoot not found");
    }

    protected override void SubscribeToServicesEventsOnStart()
    {
        _contextualPanel.PlayTourClickedEvent += OnPlayTourClicked;
        _contextualPanel.ResetTourClickedEvent += OnResetTourClicked;
        _contextualPanel.ShownEvent += OnContextualPanelShownCallback;
        _contextualPanel.ClosedEvent += OnContextualPanelClosedCallback;
        _uiManager.ContextualPanelShownEvent += ShowContextualPanel;
        _uiManager.ContextualPanelHiddenEvent += HideContextualPanel;
    }

    protected override void UnsubscribeToServicesEventsOnExit()
    {
        _contextualPanel.PlayTourClickedEvent -= OnPlayTourClicked;
        _contextualPanel.ResetTourClickedEvent -= OnResetTourClicked;
        _contextualPanel.ShownEvent -= OnContextualPanelShownCallback;
        _contextualPanel.ClosedEvent -= OnContextualPanelClosedCallback;
        _uiManager.ContextualPanelShownEvent -= ShowContextualPanel;
        _uiManager.ContextualPanelHiddenEvent -= HideContextualPanel;
    }

    public override void StartState()
    {
        _hudScreen.style.display = DisplayStyle.Flex;

        base.StartState();

        // Show contextual panel root if it was shown before
        if (_wasContextualPanelShown)
            _contextualPanelRoot.style.display = DisplayStyle.Flex;

        // Show controls visual according to UIManager setting
        _controlsVisualRoot.style.display = _uiManager.ControlsVisibilityValueSet ?
            DisplayStyle.Flex :
            DisplayStyle.None;

        _gameManager.InputActions.UI.Enable();
        _gameManager.InputActions.UI.Pause.performed += OnPauseKeyPressed;

        _compass.Start();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        _compass.Update();
    }

    public override void ExitState()
    {
        _hudScreen.style.display = DisplayStyle.None;

        base.ExitState();

        // Hide contextual panel root
        _contextualPanelRoot.style.display = DisplayStyle.None;

        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.UI.Pause.performed -= OnPauseKeyPressed;

        _compass.IsShown(false);
    }

    public override bool IsCursorOverUI()
    {
        // Check base UI elements and contextual panel
        return base.IsCursorOverUI() || IsCursorOver(_contextualPanelRoot);
    }
    #endregion

    #region PROTECTED METHODS
    protected void ShowContextualPanel(DataSO data, bool isCharacterData = false)
    {
        _wasContextualPanelShown = true;
        _contextualPanel.ShowInfo(data, isCharacterData);
        _compass.IsShown(false);
    }
    void HideContextualPanel()
    {
        _contextualPanel.Hide();
        OnContextualPanelHidden();
        _compass.IsShown(true);
    }
    #endregion

    #region PRIVATE METHODS
    bool ValidateUIDocument()
    {
        if (_UIDocument == null)
        {
            Debug.LogError($"{_stateName} HUD State: UIDocument is null!");
            return false;
        }

        if (_UIDocument.rootVisualElement == null)
        {
            Debug.LogError($"{_stateName} HUD State: UIDocument rootVisualElement is null!");
            return false;
        }

        return true;
    }

    void InitializeContextualPanelOnAwake()
    {
        if (!ValidateUIDocument())
            return;

        _contextualPanelRoot = _hudScreen.Q<VisualElement>("ContextualPanel");

        if (_contextualPanelRoot == null)
        {
            Debug.LogWarning($"{_stateName} _contextualPanel not found");
            return;
        }

        _contextualPanel = new(_contextualPanelRoot);
    }

    void InitializeCompassOnAwake()
    {
        if (!ValidateUIDocument())
            return;

        _compassVisualRoot = _hudScreen.Q<VisualElement>("Compass");

        if (_compassVisualRoot == null)
        {
            Debug.LogWarning($"{_stateName} _compassVisualRoot not found");
            return;
        }

        _compass = new(_compassVisualRoot);
    }
    #endregion

    #region CALLBACK METHODS
    protected void OnPauseClicked(ClickEvent evt)
    {
        _uiManager.SwitchToPauseState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnPauseKeyPressed(InputAction.CallbackContext context)
    {
        OnPauseClicked(null);
    }

    void OnContextualPanelShownCallback()
    {
        _wasContextualPanelShown = true;
        OnContextualPanelShown();
    }

    void OnPlayTourClicked()
    {
        _wasContextualPanelShown = false;
        PlayTourEvent?.Invoke();
    }

    void OnResetTourClicked()
    {
        _wasContextualPanelShown = false;
        ResetTourEvent?.Invoke();
    }

    void OnContextualPanelClosedCallback()
    {
        ContextualPanelHiddenEvent?.Invoke();
        _wasContextualPanelShown = false;
        OnContextualPanelHidden();
    }
    #endregion

    #region VIRTUAL METHODS
    protected abstract void OnContextualPanelShown();
    protected abstract void OnContextualPanelHidden();
    #endregion
}
