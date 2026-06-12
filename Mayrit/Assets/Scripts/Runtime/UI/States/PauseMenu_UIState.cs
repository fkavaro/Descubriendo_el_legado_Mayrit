using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenu_UIState : AUIState
{
    #region  PROPERTIES
    Button _resumeButton,
        _mainMenuButton,
        _settingsButton,
        _creditsButton,
        _quitButton;

    public event Action ResumeGameClickedEvent;
    public event Action MainMenuClickedEvent;
    public event Action SettingsClickedEvent;
    public event Action CreditsClickedEvent;
    public event Action QuitClickedEvent;
    #endregion

    #region CONSTRUCTOR
    public PauseMenu_UIState(UISystem uiSystem, UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base(uiSystem, "PauseMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _resumeButton = GetButtonAndRegisterCallback("ResumeGameButton", OnResumeGameClicked);
        _mainMenuButton = GetButtonAndRegisterCallback("MainMenuButton", OnMainMenuClicked);
        _settingsButton = GetButtonAndRegisterCallback("SettingsButton", OnSettingsClicked);
        _creditsButton = GetButtonAndRegisterCallback("CreditsButton", OnCreditsClicked);
        _quitButton = GetButtonAndRegisterCallback("QuitButton", OnQuitClicked);
    }
    protected override void SubscribeToServicesEventsOnStart()
    {
        base.SubscribeToServicesEventsOnStart();

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
    }
    #endregion

    #region CALLBACK METHODS
    void OnResumeGameClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        ResumeGameClickedEvent?.Invoke();
    }

    void OnMainMenuClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        MainMenuClickedEvent?.Invoke();
    }

    void OnSettingsClicked(ClickEvent evt)
    {
        base.ExitState();
        _soundSystem.PlayButtonClickSFX();
        SettingsClickedEvent?.Invoke();
    }

    void OnCreditsClicked(ClickEvent evt)
    {
        base.ExitState();
        _soundSystem.PlayButtonClickSFX();
        CreditsClickedEvent?.Invoke();
    }

    void OnQuitClicked(ClickEvent evt)
    {
        _soundSystem.PlayButtonClickSFX();
        QuitClickedEvent?.Invoke();
    }

    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name == SceneDatabase.SceneName.MainMenuScene)
            base.ExitState();
    }
    #endregion
}
