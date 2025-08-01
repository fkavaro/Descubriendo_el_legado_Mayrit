using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressObject : MonoBehaviour
{
    public List<ProgressManager.Milestone> milestonesActivated;

    void Awake()
    {
        ProgressManager.Instance.OnMilestoneChanged += OnMilestoneChanged;
    }

    void Start()
    {

    }

    void OnMilestoneChanged(ProgressManager.Milestone entry)
    {
        if (milestonesActivated.Contains(entry))
            SetChildrenActive(true);
        else
            SetChildrenActive(false);
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
}
