using UnityEngine;

public class UIDebugOverlay : MonoBehaviour
{
    [Header("Debug Overlay")]
    [Tooltip("Collapse the debug overlay to a small header")]
    public bool _isCollapsed = true;

    // Dependencies
    UISystem _uiSystem;
    GameManager _gameManager;
    ProgressSystem _progressSystem;
    CameraSystem _cameraSystem;
    EnvironmentManager _environmentManager;
    TownManager _townManager;
    NPCPoolManager _npcPoolManager;
    PlayableCharacter _playableCharacter;

    void OnGUI()
    {
        // Smaller box positioned at bottom-left
        const int width = 320;
        float fullHeight = Mathf.Min(235f, Screen.height - 40f);
        float height = _isCollapsed ? 28f : fullHeight;
        float x = 10f;
        float y = Screen.height - height - 10f; // 10px margin from bottom
        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);

        // Collapsed header: show minimal button that toggles expansion
        if (_isCollapsed)
        {
            if (GUILayout.Button("Debug Overlay ▶", GUILayout.Height(24)))
                _isCollapsed = false;

            GUILayout.EndArea();
            return;
        }

        // Expanded header with a collapse button on the right
        GUILayout.BeginHorizontal();
        GUILayout.Label("Debug Overlay", GUI.skin.box);
        if (GUILayout.Button("▼", GUILayout.Width(28)))
        {
            _isCollapsed = true;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            return;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        // Get dependencies from Service Locator
        _uiSystem = ServiceLocator.Instance.Get<UISystem>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _progressSystem = ServiceLocator.Instance.Get<ProgressSystem>();
        _cameraSystem = ServiceLocator.Instance.Get<CameraSystem>();
        _environmentManager = ServiceLocator.Instance.Get<EnvironmentManager>();
        _townManager = ServiceLocator.Instance.Get<TownManager>();
        _npcPoolManager = ServiceLocator.Instance.Get<NPCPoolManager>();
        _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

        // Display states of various managers
        if (_uiSystem != null && _uiSystem.BehaviourSystem != null)
            GUILayout.Label($"UISystem state: {_uiSystem.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("UISystem: <null>");

        if (_gameManager != null && _gameManager.BehaviourSystem != null)
            GUILayout.Label($"GameManager state: {_gameManager.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("GameManager: <null>");

        if (_progressSystem != null && _progressSystem.BehaviourSystem != null)
            GUILayout.Label($"ProgressSystem state: {_progressSystem.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("ProgressSystem: <null>");

        if (_cameraSystem != null && _cameraSystem.BehaviourSystem != null)
            GUILayout.Label($"CameraSystem state: {_cameraSystem.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("CameraSystem: <null>");

        if (_playableCharacter != null && _playableCharacter.BehaviourSystem != null)
            GUILayout.Label($"PlayableCharacter state: {_playableCharacter.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("PlayableCharacter: <null>");

        if (_environmentManager != null)
            GUILayout.Label($"Current time: {_environmentManager.CurrentTime:F0}h");
        else
            GUILayout.Label("TimeManager: <null>");

        if (_townManager != null)
            GUILayout.Label($"TownManager population: {_townManager._population}");
        else
            GUILayout.Label("TownManager: <null>");

        if (_npcPoolManager != null)
            GUILayout.Label($"NPCPoolManager max villagers: {_npcPoolManager._maxActiveVillagers}");
        else
            GUILayout.Label("NPCPoolManager: <null>");

        GUILayout.EndArea();
    }
}
