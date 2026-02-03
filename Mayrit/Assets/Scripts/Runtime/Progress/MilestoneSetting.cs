using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// MilestoneSetting is an editor-only helper script that allows you to preview
/// how objects will appear at different milestone values while designing your level.
/// It updates all child MilestoneTracker components to show/hide their children
/// based on the selected milestone value (0-7).
/// </summary>
public class MilestoneSetting : MonoBehaviour
{
    [Range(-1, 7)]
    [SerializeField] private int _milestonePreview = -1;

#if UNITY_EDITOR
    private int _lastMilestone = -1;

    void OnValidate()
    {
        // Only work in edit mode, never during play
        if (Application.isPlaying)
            return;

        // Only update if the milestone value actually changed
        if (_milestonePreview != _lastMilestone)
        {
            _lastMilestone = _milestonePreview;
            UpdateAllChildTrackers();
        }
    }

    /// <summary>
    /// Updates all child MilestoneTracker components to reflect the current milestone value
    /// </summary>
    private void UpdateAllChildTrackers()
    {
        // Get all MilestoneTracker components in children (including inactive ones)
        MilestoneTracker[] trackers = GetComponentsInChildren<MilestoneTracker>(true);

        foreach (MilestoneTracker tracker in trackers)
        {
            if (tracker == null) continue;

            // Update the tracker's children
            tracker.SetChildrenActiveGivenIndex(_milestonePreview);
        }

        // Mark the scene as dirty so changes are saved
        EditorUtility.SetDirty(gameObject);
    }

    // Add a context menu option to manually refresh if needed
    [ContextMenu("Refresh Milestone Preview")]
    private void RefreshPreview()
    {
        _lastMilestone = -1; // Force refresh
        UpdateAllChildTrackers();
    }
#endif
}
