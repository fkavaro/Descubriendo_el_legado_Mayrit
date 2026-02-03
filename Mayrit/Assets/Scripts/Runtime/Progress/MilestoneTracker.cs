using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ProgressObject listens to ProgressManager milestone changes and activates/deactivates
/// its child objects accordingly. This implementation is defensive for editor-time
/// operations (OnValidate, import callbacks) and avoids accessing singletons that may
/// not be initialized during editor import.
/// </summary>
public class MilestoneTracker : MonoBehaviour
{
    public bool _isActive = true;

    [Tooltip("Range of milestones where this object is active")]
    [SerializeField] protected Vector2 milestonesActivated;

    #region LIFE CYCLE
    void OnEnable()
    {
        SubscribeToRuntimeEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromRuntimeEvents();
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            SubscribeToRuntimeEvents();
#endif
    }
    #endregion

    public void SetChildrenActiveGivenIndex(int milestoneIdx)
    {
        if (milestoneIdx < 0)
        {
            SetChildrenActive(true);
            return;
        }

        int min = Mathf.Min((int)milestonesActivated.x, (int)milestonesActivated.y);
        int max = Mathf.Max((int)milestonesActivated.x, (int)milestonesActivated.y);
        SetChildrenActive(milestoneIdx >= min && milestoneIdx <= max);
    }

    #region PRIVATE METHODS
    protected virtual void SetChildrenActive(bool isActive)
    {
        if (this == null) return;

        _isActive = isActive;

        foreach (Transform child in transform)
        {
            if (child == null || child.gameObject == null) continue;
            if (child.gameObject.activeSelf != isActive)
                child.gameObject.SetActive(isActive);
        }
    }

    void SubscribeToRuntimeEvents()
    {
        ProgressManager progressManager = FindAnyObjectByType<ProgressManager>();

        if (progressManager != null)
        {
            progressManager.MilestoneChangedEvent += OnMilestoneChanged;
            progressManager.OnEditorUpdateChangedEvent += OnEditorUpdateChanged;
        }
    }

    void UnsubscribeFromRuntimeEvents()
    {
        ProgressManager progressManager = FindAnyObjectByType<ProgressManager>();

        if (progressManager != null)
        {
            progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
            progressManager.OnEditorUpdateChangedEvent -= OnEditorUpdateChanged;
        }
    }
    #endregion

    #region CALLBACK METHODS
    void OnMilestoneChanged(Milestone_DataSO milestoneMapping)
    {
        SetChildrenActiveGivenIndex(milestoneMapping.Index);
    }

    protected virtual void OnEditorUpdateChanged(bool updateInEditor)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;

        if (this == null) return;
        if (!updateInEditor)
        {
            SetChildrenActive(updateInEditor);
            return;
        }

        var progressManager = FindAnyObjectByType<ProgressManager>();
        if (progressManager == null) return;
        int milestone = progressManager.CurrentMilestoneIndex;
#endif
    }
}
#endregion

