using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [Header("Debug Overlay")]
    [Tooltip("Collapse the debug overlay to a small header")]
    public bool _isCollapsed = true;

    // Dependencies
    UIManager _uiManager;
    GameManager _gameManager;
    ProgressManager _progressManager;
    CameraManager _cameraManager;
    TimeManager _timeManager;
    TownManager _townManager;
    NPCPoolManager _npcPoolManager;


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
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _timeManager = ServiceLocator.Instance.Get<TimeManager>();
        _townManager = ServiceLocator.Instance.Get<TownManager>();
        _npcPoolManager = ServiceLocator.Instance.Get<NPCPoolManager>();

        // Display states of various managers
        if (_uiManager != null && _uiManager.BehaviourSystem != null)
            GUILayout.Label($"UIManager state: {_uiManager.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("UIManager: <null>");

        if (_gameManager != null && _gameManager.BehaviourSystem != null)
            GUILayout.Label($"GameManager state: {_gameManager.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("GameManager: <null>");

        if (_progressManager != null && _progressManager.BehaviourSystem != null)
            GUILayout.Label($"ProgressManager state: {_progressManager.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("ProgressManager: <null>");

        if (_cameraManager != null && _cameraManager.BehaviourSystem != null)
            GUILayout.Label($"CameraManager state: {_cameraManager.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("CameraManager: <null>");

        if (_gameManager != null && _gameManager.PlayableCharacter != null)
            GUILayout.Label($"PlayableCharacter state: {_gameManager.PlayableCharacter.BehaviourSystem.CurrentState.StateName}");
        else
            GUILayout.Label("PlayableCharacter: <null>");

        if (_timeManager != null)
            GUILayout.Label($"TimeManager current time: {_timeManager.CurrentTime:F0}h");
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
