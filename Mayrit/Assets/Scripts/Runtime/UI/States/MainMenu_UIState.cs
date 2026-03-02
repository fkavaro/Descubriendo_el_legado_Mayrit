using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _newGameButton,
        _loadGameButton,
        _settingsButton,
        _quitButton,
        _confirmNewGameButton,
        _cancelNewGameButton;

    VisualElement _newGameWarningPopup,
        _buttons;
    #endregion

    #region CONSTRUCTOR
    public MainMenu_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("MainMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _buttons = GetByName<VisualElement>("Buttons");
        _newGameButton = GetButtonAndRegisterCallback("NewGameButton", OnNewGameClicked, _buttons);
        _loadGameButton = GetButtonAndRegisterCallback("LoadGameButton", OnLoadGameClicked, _buttons);
        _settingsButton = GetButtonAndRegisterCallback("SettingsButton", OnSettingsClicked, _buttons);
        _quitButton = GetButtonAndRegisterCallback("QuitButton", OnQuitClicked, _buttons);

        _newGameWarningPopup = GetByName<VisualElement>("NewGameWarning");
        _confirmNewGameButton = GetButtonAndRegisterCallback("ConfirmNewGameButton", OnConfirmNewGameClicked, _newGameWarningPopup);
        _cancelNewGameButton = GetButtonAndRegisterCallback("CancelNewGameButton", OnCancelNewGameClicked, _newGameWarningPopup);

        _newGameWarningPopup.style.display = DisplayStyle.None;
    }

    public override void StartState()
    {
        CheckLoadButtonAvailability();

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
        bool canLoadGame = GameSaveSystem.IsThereStoredData();
        _loadGameButton.SetEnabled(canLoadGame);
        _loadGameButton.pickingMode = canLoadGame ? PickingMode.Position : PickingMode.Ignore;
    }

    #region CALLBACK METHODS
    void OnNewGameClicked(ClickEvent evt)
    {
        _soundManager.PlayButtonClickSFX();

        if (GameSaveSystem.IsThereStoredData())
        {
            _newGameWarningPopup.style.display = DisplayStyle.Flex;
            _buttons.style.display = DisplayStyle.None;
        }
        else
        {
            GameSaveSystem.Clear();
            _gameManager.SwitchToGamePlayState();
        }
    }

    void OnConfirmNewGameClicked(ClickEvent evt)
    {
        _soundManager.PlayButtonClickSFX();
        GameSaveSystem.Clear();
        _gameManager.SwitchToGamePlayState();
    }

    void OnCancelNewGameClicked(ClickEvent evt)
    {
        _soundManager.PlayButtonClickSFX();
        _newGameWarningPopup.style.display = DisplayStyle.None;
        _buttons.style.display = DisplayStyle.Flex;
    }

    void OnLoadGameClicked(ClickEvent evt)
    {
        _gameManager.SwitchToGamePlayState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnSettingsClicked(ClickEvent evt)
    {
        _uiManager.SwitchToSettingsMenuState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnQuitClicked(ClickEvent evt)
    {
        _soundManager.PlayButtonClickSFX();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For convenience in the editor
#endif
    }
    #endregion
}