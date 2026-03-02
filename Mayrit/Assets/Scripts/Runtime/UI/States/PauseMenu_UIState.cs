using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PauseMenu_UIState : AUIState
{
    #region  PROPERTIES
    Button _playButton,
        _mainMenuButton,
        _settingsButton,
        _quitButton;

    CameraManager _cameraManager;
    #endregion

    #region CONSTRUCTOR
    public PauseMenu_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("PauseMenu", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _playButton = GetButtonAndRegisterCallback("PlayButton", OnPlayClicked);
        _mainMenuButton = GetButtonAndRegisterCallback("MainMenuButton", OnMainMenuClicked);
        _settingsButton = GetButtonAndRegisterCallback("SettingsButton", OnSettingsClicked);
        _quitButton = GetButtonAndRegisterCallback("QuitButton", OnQuitClicked);
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        if (_cameraManager == null)
            _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
    }

    protected override void SubscribeToServicesEventsOnStart()
    {
        base.SubscribeToServicesEventsOnStart();

        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
    }

    public override void StartState()
    {
        base.StartState();
        _gameManager.SwitchToPauseState();
        _gameManager.InputActions.UI.Enable();
        _gameManager.InputActions.UI.Pause.performed += OnPauseKeyPressed;
    }

    public override void ExitState()
    {
        //base.ExitState(); Obly after main menu loaded
        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.UI.Pause.performed -= OnPauseKeyPressed;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        base.ExitState();
        _gameManager.SwitchToGamePlayState();
        _soundManager.PlayButtonClickSFX();

        if (_cameraManager.IsInSpectatorState || _cameraManager.IsInOrbitalState)
            _uiManager.SwitchToSpectatorHUDState();
        else // Third Person or POI cameras
            _uiManager.SwitchToPlayerHUDState();
    }

    void OnPauseKeyPressed(InputAction.CallbackContext context)
    {
        OnPlayClicked(null);
    }

    void OnMainMenuClicked(ClickEvent evt)
    {
        _gameManager.SwitchToMainMenuState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnSettingsClicked(ClickEvent evt)
    {
        base.ExitState();
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

    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        if (name == SceneDatabase.SceneName.MainMenuScene)
            base.ExitState();
    }
    #endregion
}
