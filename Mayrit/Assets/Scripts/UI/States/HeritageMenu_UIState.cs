using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HeritageMenu_UIState : AUIState
{
    #region PROPERTIES
    Button _playButton;
    #endregion

    #region CONSTRUCTOR
    public HeritageMenu_UIState(UIDocument uiDocument)
    : base("HeritageMenu", uiDocument) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _playButton = _screen.Q<Button>("PlayButton");

        if (_playButton == null)
            Debug.LogWarning("_playButton not found");
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _playButton.RegisterCallback<ClickEvent>(OnPlayClicked);
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
        base.ExitState();

        _gameManager.InputActions.UI.Disable();
        _gameManager.InputActions.UI.Pause.performed -= OnPauseKeyPressed;
    }
    #endregion

    #region CALLBACK METHODS
    void OnPlayClicked(ClickEvent evt)
    {
        _uiManager.BehaviourSystem.SwitchToPreviousStateInStack(); // Player or spectator HUD
        _gameManager.SwitchToGamePlayState();
        _soundManager.PlayButtonClickSFX();
    }

    void OnPauseKeyPressed(InputAction.CallbackContext context)
    {
        OnPlayClicked(null);
    }
    #endregion
}
