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

    protected ContextualPanelUI _contextualPanel;
    protected CompassUI _compass;
    protected bool _wasContextualPanelShown;

    VisualElement _hudScreen,
        _contextualPanelRoot,
        _controlsVisualRoot,
        _compassVisualRoot;
    #endregion

    #region CONSTRUCTOR
    public AHUDState(string name, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(name, uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _controlsVisualRoot = GetByName<VisualElement>("ControlsVisual");
        _hudScreen = GetByName<VisualElement>("HUD", _UIDocument.rootVisualElement);
        _contextualPanelRoot = GetByName<VisualElement>("ContextualPanel", _hudScreen);
        _compassVisualRoot = GetByName<VisualElement>("Compass", _hudScreen);

        _contextualPanel = new(_UIDocument, _contextualPanelRoot);
        _contextualPanel.AwakeState();
        _compass = new(_UIDocument, _compassVisualRoot);
        _compass.AwakeState();
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

        _compass.Start();

        if (_wasContextualPanelShown)
        {
            _contextualPanelRoot.style.display = DisplayStyle.Flex;
            _compass.IsShown(false);
        }
        else
        {
            _contextualPanelRoot.style.display = DisplayStyle.None;
            _compass.IsShown(true);
        }

        // Show controls visual according to UIManager setting
        _controlsVisualRoot.style.display = _uiManager.ControlsVisibilityValueSet ?
            DisplayStyle.Flex :
            DisplayStyle.None;

        _gameManager.InputActions.UI.Enable();
        _gameManager.InputActions.UI.Pause.performed += OnPauseKeyPressed;
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

        _contextualPanelRoot.style.display = DisplayStyle.None;
        _compass.IsShown(false);

        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.UI.Pause.performed -= OnPauseKeyPressed;
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
        _controlsVisualRoot.style.display = DisplayStyle.None;
    }
    void HideContextualPanel()
    {
        _contextualPanel.Hide();
        OnContextualPanelHidden();
        _compass.IsShown(true);
        _controlsVisualRoot.style.display = _uiManager.ControlsVisibilityValueSet ?
            DisplayStyle.Flex :
            DisplayStyle.None;
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
