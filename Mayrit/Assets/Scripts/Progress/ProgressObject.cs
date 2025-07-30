using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ProgressObject : MonoBehaviour
{
    public ProgressManager.Milestone enablingMilestone;
    public ProgressManager.Milestone disablingMilestone;

    void Awake()
    {
        ProgressManager.Instance.OnMilestoneChanged += OnMilestoneChanged;
    }

    void Start()
    {
        if (enablingMilestone == ProgressManager.Instance._currentMilestone.milestone)
            SetActive(true);
        else
            SetActive(false);
    }

    private void OnMilestoneChanged(ProgressManager.Milestone entry)
    {
        if (enablingMilestone == entry)
            SetActive(true);
        else if (disablingMilestone == entry)
            SetActive(false);
    }

    void SetActive(bool isActive)
    {
        //gameObject.SetActive(isActive);

        // Also all children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }
}
