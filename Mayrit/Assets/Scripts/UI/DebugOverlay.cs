using UnityEngine;

public class DebugOverlay : MonoBehaviour
{
    [Header("Debug Overlay")]
    [Tooltip("Collapse the debug overlay to a small header")]
    public bool _isCollapsed = true;

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

        if (UIManager.Instance != null && UIManager.Instance.BehaviourSystem != null)
            GUILayout.Label($"UIManager state: {UIManager.Instance.BehaviourSystem.CurrentState.StateName}");
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
}
