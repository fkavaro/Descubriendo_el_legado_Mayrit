using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class MainMenu_UIState : AUIState
{
    #region PUBLIC PROPERTIES

    #endregion

    #region PRIVATE PROPERTIES
    Button _playButton, _mainMenuButton, _quitButton;
    #endregion

    public MainMenu_UIState(UIDocument uiDocument)
    : base("MainMenu", uiDocument) { }

    #region INHERITED PROPERTIES
    public override void StartState()
    {
        _screen = _UIDocument.rootVisualElement;//.Q<VisualElement>("MainMenu");
        _playButton = _screen.Q<Button>("PlayButton");
        _mainMenuButton = _screen.Q<Button>("SettingsButton");
        _quitButton = _screen.Q<Button>("QuitButton");

        _playButton.RegisterCallback<ClickEvent>(SwitchToGamePlayState);
        _mainMenuButton.RegisterCallback<ClickEvent>(SwitchToSettingsState);
        _quitButton.RegisterCallback<ClickEvent>(QuitGame);
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void SwitchToGamePlayState(ClickEvent evt)
    {
        GameManager.Instance.BehaviourSystem.SwitchState(GameManager.Instance._gamePlayState);
    }

    void SwitchToSettingsState(ClickEvent evt)
    {
        // TODO: settings menu
    }

    void QuitGame(ClickEvent evt)
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For convenience in the editor
#endif
    }
    #endregion
}