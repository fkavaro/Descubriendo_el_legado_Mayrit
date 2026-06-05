using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class AHUDState : AUIState
{
    #region PROPERTIES
    protected CompassUI _compass;

    VisualElement _hudScreen,
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
        _compassVisualRoot = GetByName<VisualElement>("Compass", _hudScreen);

        _compass = new(_UIDocument, _compassVisualRoot);
        _compass.AwakeState();
        _hudScreen.style.display = DisplayStyle.None;
    }

    public override void StartState()
    {
        _hudScreen.style.display = DisplayStyle.Flex;

        base.StartState();
        _compass.StartState();

        // Show controls visual according to UIManager setting
        _controlsVisualRoot.style.display = _uiManager.ControlsVisibilityValueSet ?
            DisplayStyle.Flex :
            DisplayStyle.None;

        _compass.IsShown = true;

        _gameManager.InputActions.UI.Enable();
        _gameManager.InputActions.UI.Pause.performed += OnPauseKeyPressed;
    }

    public override void UpdateState()
    {
        base.UpdateState();
        _compass.UpdateState();
    }

    public override void ExitState()
    {
        _hudScreen.style.display = DisplayStyle.None;

        base.ExitState();
        _compass.IsShown = false;

        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.UI.Pause.performed -= OnPauseKeyPressed;
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
    #endregion
}
