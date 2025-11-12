using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ProgressObject listens to ProgressManager milestone changes and activates/deactivates
/// its child objects accordingly. This implementation is defensive for editor-time
/// operations (OnValidate, import callbacks) and avoids accessing singletons that may
/// not be initialized during editor import.
/// </summary>
public class ProgressObject : MonoBehaviour
{
    public List<ProgressManager.Milestone> milestonesActivated;

    void OnEnable()
    {
        // Subscribe to runtime events when enabled. Use scene lookup to avoid forcing
        // singleton creation during editor import or teardown.
        var pm = FindAnyObjectByType<ProgressManager>();
        if (pm != null)
        {
            pm.OnMilestoneChanged += OnMilestoneChanged;
            pm.OnEditorUpdateChanged += OnEditorUpdateChanged;
        }

        // If playing, initialize immediately to the current milestone.
        if (Application.isPlaying && pm != null)
            SetChildrenActive(milestonesActivated != null && milestonesActivated.Contains(pm._currentMilestone));
    }

    void OnDisable()
    {
        var pm = FindAnyObjectByType<ProgressManager>();
        if (pm != null)
        {
            pm.OnMilestoneChanged -= OnMilestoneChanged;
            pm.OnEditorUpdateChanged -= OnEditorUpdateChanged;
        }
    }

    void OnValidate()
    {
        // Editor-time: avoid calling SetActive directly inside OnValidate. Schedule a
        // delayed call so runtime APIs are safe. Also ensure we are subscribed so
        // delayed invocations from ProgressManager reach this object.
#if UNITY_EDITOR
        var pm = FindAnyObjectByType<ProgressManager>();
        if (pm == null) return;

        // Ensure subscription in editor so delayed invocations are received.
        pm.OnMilestoneChanged -= OnMilestoneChanged;
        pm.OnEditorUpdateChanged -= OnEditorUpdateChanged;

        var milestone = pm._currentMilestone;

        if (!Application.isPlaying)
        {
            pm.OnMilestoneChanged += OnMilestoneChanged;
            pm.OnEditorUpdateChanged += OnEditorUpdateChanged;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                SetChildrenActive(milestonesActivated != null && milestonesActivated.Contains(milestone));
            };
        }
#endif
    }

    void OnMilestoneChanged(ProgressManager.Milestone entry)
    {
        // Defensive: the object may have been destroyed between the delayed call being
        // scheduled and now (editor callbacks). Unity will throw when accessing members
        // of a destroyed object, so check for null here.
        if (this == null) return;
        SetChildrenActive(milestonesActivated != null && milestonesActivated.Contains(entry));
    }

    private void OnEditorUpdateChanged(bool updateInEditor)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (this == null) return;
            if (!updateInEditor)
                SetChildrenActive(true);
            else
            {
                var pm = FindAnyObjectByType<ProgressManager>();
                if (pm == null) return;
                var milestone = pm._currentMilestone;
                SetChildrenActive(milestonesActivated != null && milestonesActivated.Contains(milestone));
            }
        }
#endif
    }

    void SetChildrenActive(bool isActive)
    {
        // Defensive: guard against the Unity object being destroyed (editor delayed calls)
        if (this == null) return;

        foreach (Transform child in transform)
        {
            if (child == null || child.gameObject == null) continue;
            if (child.gameObject.activeSelf != isActive)
                child.gameObject.SetActive(isActive);
        }
    }
}
