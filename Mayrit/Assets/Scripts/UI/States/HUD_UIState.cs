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
    Button _pauseButton, _closeHeritagePanelButton, _discoverHeritageButton, _playerButton;
    VisualElement _heritagePanel, _eventPanel;
    Vector2 _cursorScreenPos;
    #endregion

    #region INHERITED
    public HUD_UIState(FiniteStateMachine<UIManager> stateMachine)
    : base("HUD", stateMachine) { }

    public override void AwakeState()
    {
        _UIDocument = UIManager.Instance._UIDocument;
        _screen = _UIDocument.rootVisualElement.Q<VisualElement>("HUD");

        _tooltip = _screen.Q<Label>("Tooltip");
        _pauseButton = _screen.Q<Button>("PauseButton");
        _heritagePanel = _screen.Q<VisualElement>("HeritagePanel");
        _heritagePanelName = _heritagePanel.Q<Label>("Name");
        _heritagePanelDescription = _heritagePanel.Q<Label>("Description");
        _closeHeritagePanelButton = _heritagePanel.Q<Button>("CloseButton");
        _discoverHeritageButton = _screen.Q<Button>("HeritageButton");
        _eventPanel = _screen.Q<VisualElement>("EventPanel");
        _playerButton = _screen.Q<Button>("PlayerButton");

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
        if (_playerButton == null)
            Debug.LogWarning("_playerButton button not found");

        _pauseButton.RegisterCallback<ClickEvent>(SwitchToPauseState);
        _closeHeritagePanelButton.RegisterCallback<ClickEvent>(CloseHeritagePanel);
        //_discoverHeritageButton.RegisterCallback<ClickEvent>(CloseHeritagePanel);
        _playerButton.RegisterCallback<ClickEvent>(PlayPlayer);
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
            _tooltip.style.left = _cursorScreenPos.x + UIManager.Instance._tooltipOffset.x; ;
            _tooltip.style.top = Screen.height - _cursorScreenPos.y + UIManager.Instance._tooltipOffset.y;
        }

        UpdatePlayerButton();
    }

    private void UpdatePlayerButton()
    {
        if (_playerButton == null) return;

        // Get the player's world position and an offset above the head
        Vector3 playerWorldPos = GameObject.FindGameObjectWithTag("Player").transform.position + Vector3.up * 2.0f;

        // Convert world position to screen position
        Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(playerWorldPos);

        // Convert screen position to UI Toolkit coordinates
        // UI Toolkit's y=0 is at the top, but ScreenPoint's y=0 is at the bottom, so invert y
        _playerButton.style.left = playerScreenPos.x + UIManager.Instance._playerButtonOffset.x;
        _playerButton.style.top = Screen.height - playerScreenPos.y + UIManager.Instance._playerButtonOffset.y;

        // Hide the button if the player is off-screen
        // _playerButton.style.display = (playerScreenPos.z > 0 &&
        //                                 playerScreenPos.x >= 0 && playerScreenPos.x <= Screen.width &&
        //                                 playerScreenPos.y >= 0 && playerScreenPos.y <= Screen.height)
        //                                 ? DisplayStyle.Flex
        //                                 : DisplayStyle.None;
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
        _stateMachine.SwitchState(UIManager.Instance._pauseState); // Switch to pause state
    }

    void CloseHeritagePanel(ClickEvent evt)
    {
        // - Cursor will be over UI
        if (_heritagePanel == null) return;

        _heritagePanel.style.display = DisplayStyle.None;
    }

    void PlayPlayer(ClickEvent evt)
    {
        if (_playerButton == null) return;

        _playerButton.style.display = DisplayStyle.None;
        CameraManager.Instance.PlayPlayer();
    }
    #endregion
}
