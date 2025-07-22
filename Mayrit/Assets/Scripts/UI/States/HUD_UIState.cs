using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class HUD_UIState : AUIState
{
    #region PUBLIC PROPERTIES
    #endregion

    #region PRIVATE PROPERTIES
    VisualElement _hud;
    Label _tooltip;
    Button _pauseButton;
    Vector2 _cursorCreenPos;
    #endregion

    #region INHERITED
    public HUD_UIState(FiniteStateMachine<UIManager> stateMachine)
    : base("HUD", stateMachine) { }

    public override void AwakeState()
    {

    }

    public override void StartState()
    {
        UIManager.Instance.UIDocument = UIManager.Instance.GetComponent<UIDocument>();
        _UI = UIManager.Instance.UIDocument;
        _hud = _UI.rootVisualElement.Q<VisualElement>("HUD");
        _tooltip = _hud.Q<Label>("Tooltip");
        _pauseButton = _hud.Q<Button>("PauseButton");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _hud.style.display = DisplayStyle.Flex; // Show HUD
        HideTooltip();
    }

    public override void UpdateState()
    {
        if (_tooltip != null &&
            _tooltip.style.display == DisplayStyle.Flex)
        {
            _cursorCreenPos = Mouse.current.position.ReadValue();

            // UI Toolkit's Y axis is from top to bottom, while screen coordinates are from bottom to top
            float tooltipX = _cursorCreenPos.x + UIManager.Instance.tooltipOffsetX;
            float tooltipY = Screen.height - _cursorCreenPos.y - UIManager.Instance.tooltipOffsetY;

            _tooltip.style.left = tooltipX;
            _tooltip.style.top = tooltipY;
        }
    }

    public override void ExitState()
    {
        _hud.style.display = DisplayStyle.None; // Hide HUD
    }

    #endregion

    #region PUBLIC METHODS
    public void PlaceTooltip(GameObject gameObject)
    {
        if (_tooltip == null)
            Debug.LogWarning("Tooltip not found");
        else
        {
            _tooltip.text = gameObject.name; // TODO: take selectableObject name component
            _tooltip.style.display = DisplayStyle.Flex; // Show tooltip}
        }
    }

    public void HideTooltip()
    {
        if (_tooltip == null)
            Debug.LogWarning("Tooltip not found");
        else
            _tooltip.style.display = DisplayStyle.None; // Hide by default
    }
    #endregion

    #region PRIVATE METHODS
    void SwitchToPauseState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance.pauseState); // Switch to pause state
    }
    #endregion
}
