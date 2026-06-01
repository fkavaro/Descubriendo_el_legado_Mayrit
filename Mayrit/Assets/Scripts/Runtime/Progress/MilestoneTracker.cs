using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MilestoneTracker : MonoBehaviour
{
    public bool _isActive = true;

    [Tooltip("Range of milestones where this object is active")]
    [SerializeField] protected List<Vector2> milestonesActivated;

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
    #endregion
}


