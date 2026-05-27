using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MilestoneTracker : MonoBehaviour
{
    public bool _isActive = true;

    [Tooltip("Range of milestones where this object is active")]
    [SerializeField] protected List<Vector2> milestonesActivated;

    // TODO: remove eventually
    //     #region LIFE CYCLE
    //     void OnEnable()
    //     {
    //         SubscribeToRuntimeEvents();
    //     }

    //     void OnDisable()
    //     {
    //         UnsubscribeFromRuntimeEvents();
    //     }

    //     void OnValidate()
    //     {
    // #if UNITY_EDITOR
    //         if (!Application.isPlaying)
    //             SubscribeToRuntimeEvents();
    // #endif
    //     }
    //     #endregion

    public virtual void SetChildrenActiveGivenIndex(int milestoneIdx)
    {
        if (milestoneIdx < 0)
        {
            SetChildrenActive(true);
            return;
        }

        bool inAnyRange = false;

        foreach (Vector2 milestoneRange in milestonesActivated)
        {
            if (milestoneIdx >= milestoneRange.x && milestoneIdx <= milestoneRange.y)
            {
                inAnyRange = true;
                break;
            }
        }

        SetChildrenActive(inAnyRange);
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

    // TODO: remove eventually  
    // void SubscribeToRuntimeEvents()
    // {
    //     ProgressManager progressManager = FindAnyObjectByType<ProgressManager>();

    //     if (progressManager != null)
    //     {
    //         progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    //         progressManager.OnEditorUpdateChangedEvent += OnEditorUpdateChanged;
    //     }
    // }

    // void UnsubscribeFromRuntimeEvents()
    // {
    //     ProgressManager progressManager = FindAnyObjectByType<ProgressManager>();

    //     if (progressManager != null)
    //     {
    //         progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    //         progressManager.OnEditorUpdateChangedEvent -= OnEditorUpdateChanged;
    //     }
    // }
    #endregion

    #region CALLBACK METHODS
    // void OnMilestoneChanged(Milestone_DataSO milestoneMapping)
    // {
    //     SetChildrenActiveGivenIndex(milestoneMapping.Index);
    // }

    //     protected virtual void OnEditorUpdateChanged(bool updateInEditor)
    //     {
    // #if UNITY_EDITOR
    //         if (Application.isPlaying)
    //             return;

    //         if (this == null) return;
    //         if (!updateInEditor)
    //         {
    //             SetChildrenActive(updateInEditor);
    //             return;
    //         }

    //         var progressManager = FindAnyObjectByType<ProgressManager>();
    //         if (progressManager == null) return;
    //         int milestone = progressManager.CurrentMilestoneIndex;
    // #endif
    //     }
    #endregion
}


