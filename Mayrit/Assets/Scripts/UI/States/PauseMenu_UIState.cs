using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PauseMenu_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    VisualElement _pauseMenu;
    Button _playButton;
    #endregion

    #region INHERITED
    public PauseMenu_UIState(FiniteStateMachine<UIManager> stateMachine)
    : base("PauseMenu", stateMachine) { }

    public override void AwakeState()
    {

    }

    public override void StartState()
    {
        UIManager.Instance.UIDocument = UIManager.Instance.GetComponent<UIDocument>();
        _UI = UIManager.Instance.UIDocument;
        _pauseMenu = _UI.rootVisualElement.Q<VisualElement>("PauseMenu");
        _playButton = _pauseMenu.Q<Button>("PlayButton");

        _playButton.RegisterCallback<ClickEvent>(SwitchToHUDState);

        _pauseMenu.style.display = DisplayStyle.Flex; // Show pause menu

        // Game pause state
        GameManager.Instance.fsm.SwitchState(GameManager.Instance.pauseState);
    }

    public override void UpdateState()
    {

    }

    public override void ExitState()
    {
        _pauseMenu.style.display = DisplayStyle.None; // Hide HUD
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void SwitchToHUDState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance.hudState); // Switch to HUD state
        GameManager.Instance.fsm.SwitchState(GameManager.Instance.gamePlayState);
    }
    #endregion
}
