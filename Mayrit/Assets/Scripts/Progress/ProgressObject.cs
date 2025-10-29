using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressObject : MonoBehaviour
{
    public List<ProgressManager.Milestone> milestonesActivated;

    void Awake()
    {
        ProgressManager.Instance.OnMilestoneChanged += OnMilestoneChanged;
        SetChildrenActive(milestonesActivated.Contains(ProgressManager.Instance._currentMilestone));
    }

    void OnMilestoneChanged(ProgressManager.Milestone entry)
    {
        SetChildrenActive(milestonesActivated.Contains(entry));
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
}
