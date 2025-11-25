using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Manages the user interface states and data. Singleton.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : ASingletonBehaviourEntity<UIManager, StackFiniteStateMachine<AUIState>>
{
    #region EDITOR PROPERTIES
    [Header("User Interface Document")]
    public UIDocument _UIDocument;

    [Header("Tooltip Settings")]
    public Vector2 _tooltipOffset = new(-30, -30);
    #endregion

    #region INTERNAL PROPERTIES
    StackFiniteStateMachine<AUIState> _sfsm;
    public MainMenu_UIState _mainMenuState;
    public SpectatorHUD_UIState _spectatorHUDState;
    public PlayerHUD_UIState _playerHUDState;
    public PauseMenu_UIState _pauseState;
    public HeritageMenu_UIState _heritageState;
    #endregion

    #region INHERITED
    public override StackFiniteStateMachine<AUIState> InitializeBehaviourSystem()
    {
        _sfsm = new(this);

        UIDocument uiDocument = GetComponent<UIDocument>();

        // States initialization
        _mainMenuState = new(uiDocument);
        _spectatorHUDState = new(uiDocument);
        _playerHUDState = new(uiDocument);
        _pauseState = new(uiDocument);
        _heritageState = new(uiDocument);

        // Set initial state based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameScene")
            _sfsm.SetInitialState(_spectatorHUDState);
        else
            _sfsm.SetInitialState(_mainMenuState);

        return _sfsm;
    }
    #endregion

    #region DEBUG OVERLAY
    [Header("Debug Overlay")]
    [Tooltip("Show debug overlay (toggle from the inspector at runtime)")]
    public bool _showDebugOverlay = true;
    [Tooltip("Collapse the debug overlay to a small header")]
    public bool _debugCollapsed = false;

    void OnGUI()
    {
        if (!_showDebugOverlay) return;

        // Smaller box positioned at bottom-left
        const int width = 320;
        float fullHeight = Mathf.Min(235f, Screen.height - 40f);
        float height = _debugCollapsed ? 28f : fullHeight;
        float x = 10f;
        float y = Screen.height - height - 10f; // 10px margin from bottom
        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);

        // Collapsed header: show minimal button that toggles expansion
        if (_debugCollapsed)
        {
            if (GUILayout.Button("Debug Overlay ▶", GUILayout.Height(24)))
                _debugCollapsed = false;

            GUILayout.EndArea();
            return;
        }

        // Expanded header with a collapse button on the right
        GUILayout.BeginHorizontal();
        GUILayout.Label("Debug Overlay", GUI.skin.box);
        if (GUILayout.Button("▼", GUILayout.Width(28)))
        {
            _debugCollapsed = true;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            return;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        if (_sfsm != null)
            GUILayout.Label($"UIManager state: {_sfsm.CurrentState.StateName}");
        else
            GUILayout.Label("UIManager: <null>");

        if (GameManager.Instance != null && GameManager.Instance.BehaviourSystem != null)
            GUILayout.Label($"GameManager state: {GameManager.Instance.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("GameManager: <null>");

        if (ProgressManager.Instance != null && ProgressManager.Instance.BehaviourSystem != null)
            GUILayout.Label($"ProgressManager state: {ProgressManager.Instance.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("ProgressManager: <null>");

        if (CameraManager.Instance != null && CameraManager.Instance.BehaviourSystem != null)
            GUILayout.Label($"CameraManager state: {CameraManager.Instance.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("CameraManager: <null>");

        if (GameManager.Instance != null && GameManager.Instance.PlayableCharacter != null)
            GUILayout.Label($"PlayableCharacter state: {GameManager.Instance.PlayableCharacter.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("PlayableCharacter: <null>");

        if (TimeManager.Instance != null)
            GUILayout.Label($"TimeManager current time: {TimeManager.Instance._currentTime:F0}h");
        else
            GUILayout.Label("TimeManager: <null>");

        // Town Manager (use ExistingInstance to avoid creating objects during scene teardown)
        var town = TownManager.Instance;
        if (town != null)
            GUILayout.Label($"TownManager population: {town._population}");
        else
            GUILayout.Label("TownManager: <null>");

        if (NPCPoolManager.Instance != null)
            GUILayout.Label($"NPCPoolManager max villagers: {NPCPoolManager.Instance._maxActiveVillagers}");
        else
            GUILayout.Label("NPCPoolManager: <null>");

        GUILayout.EndArea();
    }
    #endregion
}
