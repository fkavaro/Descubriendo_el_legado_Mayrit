using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MilestoneSetting : MonoBehaviour
{
    public int MilestonePreviewIndex => _milestonePreviewIdx;

    [Range(-1, 7)]
    [SerializeField] private int _milestonePreviewIdx = -1;

#if UNITY_EDITOR
    private int _lastMilestone = -1;

    void OnValidate()
    {
        // Only work in edit mode, never during play
        if (Application.isPlaying)
            return;

        // Only update if the milestone value actually changed
        if (_milestonePreviewIdx != _lastMilestone)
        {
            _lastMilestone = _milestonePreviewIdx;

            // Defer the update to avoid SendMessage errors during OnValidate
            EditorApplication.delayCall += UpdateAllChildTrackers;
        }
    }

    /// <summary>
    /// Updates all child MilestoneTracker components to reflect the current milestone value
    /// </summary>
    private void UpdateAllChildTrackers()
    {
        // Safety check: component might have been destroyed before delayCall executes
        if (this == null) return;

        // Get all MilestoneTracker components in children (including inactive ones)
        MilestoneTracker[] trackers = GetComponentsInChildren<MilestoneTracker>(true);

        foreach (MilestoneTracker tracker in trackers)
        {
            if (tracker == null) continue;

            // Update the tracker's children
            tracker.SetChildrenActiveGivenIndex(_milestonePreviewIdx);
        }

        EnvironmentManager environmentManager = FindFirstObjectByType<EnvironmentManager>();

        if (environmentManager != null)
            environmentManager.SetCurrentTime(_milestonePreviewIdx);

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
