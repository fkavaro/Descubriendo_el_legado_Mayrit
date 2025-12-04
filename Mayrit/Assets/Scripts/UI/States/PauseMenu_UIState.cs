using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu_UIState : AUIState
{
    #region  PROPERTIES
    Button _playButton,
        _mainMenuButton,
        _quitButton;
    #endregion

    #region CONSTRUCTOR
    public PauseMenu_UIState(UIDocument uiDocument)
    : base("PauseMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElements()
    {
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("PauseMenu");
        _playButton = _screen.Q<Button>("PlayButton");
        _mainMenuButton = _screen.Q<Button>("MainMenuButton");
        _quitButton = _screen.Q<Button>("QuitButton");

        if (_playButton == null)
            Debug.LogWarning("_playButton not found");
        if (_mainMenuButton == null)
            Debug.LogWarning("_mainMenuButton not found");
        if (_quitButton == null)
            Debug.LogWarning("_quitButton not found");
    }

    protected override void RegisterCallbacks()
    {
        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
        _mainMenuButton.RegisterCallback<ClickEvent>(OnMainMenuClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
    }

    protected override void OnStartState()
    {
        _gameManager.SwitchToPauseState();
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        _uiManager.BehaviourSystem.SwitchToPreviousStateInStack(); // Switch to previous state: player or spectator HUD
        _gameManager.SwitchToGamePlayState();
    }

    void OnMainMenuClicked(ClickEvent evt)
    {
        _gameManager.SwitchToMainMenuState();
    }

    void OnQuitClicked(ClickEvent evt)
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For convenience in the editor
#endif
    }
    #endregion
}
