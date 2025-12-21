using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu_UIState : AUIState
{
    #region  PROPERTIES
    Button _playButton,
        _mainMenuButton,
        _settingsButton,
        _quitButton;
    #endregion

    #region CONSTRUCTOR
    public PauseMenu_UIState(UIDocument uiDocument)
    : base("PauseMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("PauseMenu");
        _playButton = _screen.Q<Button>("PlayButton");
        _mainMenuButton = _screen.Q<Button>("MainMenuButton");
        _settingsButton = _screen.Q<Button>("SettingsButton");
        _quitButton = _screen.Q<Button>("QuitButton");

        if (_playButton == null)
            Debug.LogWarning("_playButton not found");
        if (_mainMenuButton == null)
            Debug.LogWarning("_mainMenuButton not found");
        if (_settingsButton == null)
            Debug.LogWarning("_settingsButton not found");
        if (_quitButton == null)
            Debug.LogWarning("_quitButton not found");
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
        _mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuClicked);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
    }

    public override void StartState()
    {
        base.StartState();
        _gameManager.SwitchToPauseState();
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        _uiManager.BehaviourSystem.SwitchToPreviousStateInStack();
        _gameManager.SwitchToGamePlayState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnMainMenuClicked(ClickEvent evt)
    {
        _gameManager.SwitchToMainMenuState();
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
