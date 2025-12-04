using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _playButton,
        _settingsButton,
        _quitButton;
    #endregion

    #region CONSTRUCTOR
    public MainMenu_UIState(UIDocument uiDocument)
    : base("MainMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElements()
    {
        _playButton = _screen.Q<Button>("PlayButton");
        _settingsButton = _screen.Q<Button>("SettingsButton");
        _quitButton = _screen.Q<Button>("QuitButton");

        if (_playButton == null)
            Debug.LogWarning("_playButton not found");
        if (_settingsButton == null)
            Debug.LogWarning("_settingsButton not found");
        if (_quitButton == null)
            Debug.LogWarning("_quitButton not found");
    }
    protected override void RegisterCallbacks()
    {
        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
        _quitButton.RegisterCallback<ClickEvent>(OnQuitClicked);
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        _gameManager.SwitchToGamePlayState();
    }

    void OnSettingsClicked(ClickEvent evt)
    {
        // TODO: settings menu
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