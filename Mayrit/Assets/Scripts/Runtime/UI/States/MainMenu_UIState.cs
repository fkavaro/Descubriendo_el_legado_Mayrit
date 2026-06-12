using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _newGameButton,
        _loadGameButton,
        _settingsButton,
        _creditsButton,
        _quitButton,
        _confirmNewGameButton,
        _cancelNewGameButton;

    VisualElement _newGameWarningPopup,
        _buttons,
        _logoArea,
        _rightPanel;

    public event Action NewGameClickedEvent;
    public event Action LoadGameClickedEvent;
    public event Action SettingsClickedEvent;
    public event Action CreditsClickedEvent;
    public event Action QuitClickedEvent;
    #endregion

    #region CONSTRUCTOR
    public MainMenu_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "MainMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _logoArea = GetByName<VisualElement>("LogoArea");
        _rightPanel = GetByName<VisualElement>("RightPanel");

        _buttons = GetByName<VisualElement>("Buttons");
        _newGameButton = GetButtonAndRegisterCallback("NewGameButton", OnNewGameClicked, _buttons);
        _loadGameButton = GetButtonAndRegisterCallback("LoadGameButton", OnLoadGameClicked, _buttons);
        _settingsButton = GetButtonAndRegisterCallback("SettingsButton", OnSettingsClicked, _buttons);
        _creditsButton = GetButtonAndRegisterCallback("CreditsButton", OnCreditsClicked, _buttons);
        _quitButton = GetButtonAndRegisterCallback("QuitButton", OnQuitClicked, _buttons);

        _newGameWarningPopup = GetByName<VisualElement>("NewGameWarning");
        _confirmNewGameButton = GetButtonAndRegisterCallback("ConfirmNewGameButton", OnConfirmNewGameClicked, _newGameWarningPopup);
        _cancelNewGameButton = GetButtonAndRegisterCallback("CancelNewGameButton", OnCancelNewGameClicked, _newGameWarningPopup);

        _newGameWarningPopup.style.display = DisplayStyle.None;
    }

    public override void StartState()
    {
        _screen ??= GetByName<VisualElement>(_stateName, _UIDocument.rootVisualElement);

        CheckLoadButtonAvailability();

        _logoArea.style.display = DisplayStyle.Flex;
        _rightPanel.style.display = DisplayStyle.Flex;

        base.StartState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }
    #endregion

    void CheckLoadButtonAvailability()
    {
        // Check if game can be loaded to enable/disable Load Game button
        bool canLoadGame = GameSaveSystem.IsThereStoredMilestoneIdx();
        _loadGameButton.SetEnabled(canLoadGame);
        _loadGameButton.pickingMode = canLoadGame ? PickingMode.Position : PickingMode.Ignore;
    }

    #region CALLBACK METHODS
    void OnNewGameClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();

        if (GameSaveSystem.IsThereStoredMilestoneIdx())
        {
            _newGameWarningPopup.style.display = DisplayStyle.Flex;
            _buttons.style.display = DisplayStyle.None;
        }
        else
        {
            NewGameClickedEvent?.Invoke();
        }
    }

    void OnConfirmNewGameClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        NewGameClickedEvent?.Invoke();
        _newGameWarningPopup.style.display = DisplayStyle.None;
    }

    void OnCancelNewGameClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        _newGameWarningPopup.style.display = DisplayStyle.None;
        _buttons.style.display = DisplayStyle.Flex;
    }

    void OnLoadGameClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        LoadGameClickedEvent?.Invoke();

    }

    void OnSettingsClicked(ClickEvent evt)
    {
        _logoArea.style.display = DisplayStyle.None;
        _rightPanel.style.display = DisplayStyle.None;
        _screen = null; // To avoid hiding the background image
        _soundSystem.PlayButtonClickSFX();
        SettingsClickedEvent?.Invoke();
    }

    void OnCreditsClicked(ClickEvent evt)
    {
        _rightPanel.style.display = DisplayStyle.None;
        _screen = null; // To avoid hiding the background image
        _soundSystem.PlayButtonClickSFX();
        CreditsClickedEvent?.Invoke();
    }

    void OnQuitClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        QuitClickedEvent?.Invoke();
    }
    #endregion
}