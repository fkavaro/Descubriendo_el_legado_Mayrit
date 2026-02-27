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
    public MainMenu_UIState(UIDocument uiDocument)
    : base("MainMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _buttons = _screen.Q<VisualElement>("Buttons");
        _newGameButton = _buttons.Q<Button>("NewGameButton");
        _loadGameButton = _buttons.Q<Button>("LoadGameButton");
        _settingsButton = _buttons.Q<Button>("SettingsButton");
        _quitButton = _buttons.Q<Button>("QuitButton");

        _newGameWarningPopup = _screen.Q<VisualElement>("NewGameWarning");
        _confirmNewGameButton = _newGameWarningPopup.Q<Button>("ConfirmNewGameButton");
        _cancelNewGameButton = _newGameWarningPopup.Q<Button>("CancelNewGameButton");

        if (_buttons == null)
            Debug.LogWarning($"{_stateName}: 'Buttons' not found");
        if (_newGameButton == null)
            Debug.LogWarning($"{_stateName}: 'NewGameButton' not found");
        if (_loadGameButton == null)
            Debug.LogWarning($"{_stateName}: 'LoadGameButton' not found");
        if (_settingsButton == null)
            Debug.LogWarning($"{_stateName}: 'SettingsButton' not found");
        if (_quitButton == null)
            Debug.LogWarning($"{_stateName}: 'QuitButton' not found");
        if (_newGameWarningPopup == null)
            Debug.LogWarning($"{_stateName}: 'NewGameWarning' not found");
        if (_confirmNewGameButton == null)
            Debug.LogWarning($"{_stateName}: 'ConfirmNewGameButton' not found");
        if (_cancelNewGameButton == null)
            Debug.LogWarning($"{_stateName}: 'CancelNewGameButton' not found");

        _newGameWarningPopup.style.display = DisplayStyle.None;
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _newGameButton.RegisterCallback<ClickEvent>(OnNewGameClicked);
        _loadGameButton.RegisterCallback<ClickEvent>(OnLoadGameClicked);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
        _confirmNewGameButton.RegisterCallback<ClickEvent>(OnConfirmNewGameClicked);
        _cancelNewGameButton.RegisterCallback<ClickEvent>(OnCancelNewGameClicked);
    }

    public override void StartState()
    {
        CheckLoadButtonAvailability();

        base.StartState();
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