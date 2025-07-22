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
    Label _tooltip, _heritagePanelName, _heritagePanelDescription;
    Button _pauseButton, _closeHeritagePanelButton, _discoverHeritageButton;
    VisualElement _heritagePanel, _eventPanel;
    Vector2 _cursorScreenPos;
    #endregion

    #region INHERITED
    public HUD_UIState(FiniteStateMachine<UIManager> stateMachine)
    : base("HUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance.UIDocument;
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("HUD");

        _tooltip = _screen.Q<Label>("Tooltip");
        _pauseButton = _screen.Q<Button>("PauseButton");
        _heritagePanel = _screen.Q<VisualElement>("HeritagePanel");
        _heritagePanelName = _heritagePanel.Q<Label>("Name");
        _heritagePanelDescription = _heritagePanel.Q<Label>("Description");
        _closeHeritagePanelButton = _heritagePanel.Q<Button>("CloseButton");
        _discoverHeritageButton = _screen.Q<Button>("HeritageButton");
        _eventPanel = _screen.Q<VisualElement>("EventPanel");

        if (_tooltip == null)
            Debug.LogWarning("_tooltip not found");
        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_heritagePanel == null)
            Debug.LogWarning("_heritagePanel not found");
        if (_heritagePanelName == null)
            Debug.LogWarning("_heritagePanelName not found");
        if (_heritagePanelDescription == null)
            Debug.LogWarning("_heritagePanelDescription not found");
        if (_closeHeritagePanelButton == null)
            Debug.LogWarning("_closeHeritagePanelButton button not found");
        if (_discoverHeritageButton == null)
            Debug.LogWarning("_discoverHeritageButton button not found");
        if (_eventPanel == null)
            Debug.LogWarning("_eventPanel button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _closeHeritagePanelButton.RegisterCallback<ClickEvent>(CloseHeritagePanel);
        //_discoverHeritageButton.RegisterCallback<ClickEvent>(CloseHeritagePanel);
    }

    public override void StartState()
    {
        _screen.style.display = DisplayStyle.Flex; // Show HUD
        HideTooltip();
    }

    public override void UpdateState()
    {
        _cursorScreenPos = Mouse.current.position.ReadValue();

        if (_tooltip != null &&
            _tooltip.style.display == DisplayStyle.Flex)
        {
            // UI Toolkit's Y axis is from top to bottom, while screen coordinates are from bottom to top
            float tooltipX = _cursorScreenPos.x + UIManager.Instance.tooltipOffsetX;
            float tooltipY = Screen.height - _cursorScreenPos.y - UIManager.Instance.tooltipOffsetY;

            _tooltip.style.left = tooltipX;
            _tooltip.style.top = tooltipY;
        }
    }

    public override void ExitState()
    {
        _screen.style.display = DisplayStyle.None; // Hide HUD
    }

    #endregion

    #region PUBLIC METHODS
    public void ShowTooltip(SelectableObject objectHovered)
    {
        if (_tooltip == null) return;

        // Overwrite and show tooltip
        _tooltip.text = objectHovered._information.Name;
        _tooltip.style.display = DisplayStyle.Flex;
    }

    public void HideTooltip()
    {
        if (_tooltip == null) return;

        _tooltip.style.display = DisplayStyle.None;
    }

    public void ShowHeritagePanel(SelectableObject objectSelected)
    {
        if (IsCursorOverUI(_cursorScreenPos)) return;
        if (_heritagePanel == null) return;
        if (_discoverHeritageButton == null) return;
        if (_eventPanel == null) return;

        // Overwrite panel information
        _heritagePanelName.text = objectSelected._information.Name;
        _heritagePanelDescription.text = objectSelected._information.Description;

        // Show heritage panel and hide overlapping UI elements
        _heritagePanel.style.display = DisplayStyle.Flex;
        _discoverHeritageButton.style.display = DisplayStyle.None;
        _eventPanel.style.display = DisplayStyle.None;
    }

    public void HideHeritagePanel()
    {
        if (IsCursorOverUI(_cursorScreenPos)) return;
        if (_heritagePanel == null) return;
        if (_discoverHeritageButton == null) return;
        if (_eventPanel == null) return;

        _heritagePanel.style.display = DisplayStyle.None;
        _discoverHeritageButton.style.display = DisplayStyle.Flex;
        _eventPanel.style.display = DisplayStyle.Flex;
    }
    #endregion

    #region PRIVATE METHODS
    void SwitchToPauseState(ClickEvent evt)
    {
        _stateMachine.SwitchState(UIManager.Instance.pauseState); // Switch to pause state
    }

    void CloseHeritagePanel(ClickEvent evt)
    {
        // Cursor will be over UI
        if (_heritagePanel == null) return;

        _heritagePanel.style.display = DisplayStyle.None;
    }
    #endregion
}
