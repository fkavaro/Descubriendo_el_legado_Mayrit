using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PrioritySelectorNode is a composite node that that executes its children in descencing priority.
/// Like a logical OR operation, it will return success when a child return success.
/// </summary>
public class PrioritySelectorNode : SelectorNode
{
    List<Node> sortedChildren;
    List<Node> SortedChildren => sortedChildren ??= SortChildren();

    public PrioritySelectorNode(IBehaviourControllable controllable, int priority = 0)
    : base(controllable, priority) { }

    public override Status UpdateNode()
    {
        foreach (var child in SortedChildren)
        {
            switch (child.UpdateNode())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    return Status.Success;
                default:
                    continue;
            }
        }
        return Status.Failure;
    }


    public override void Reset()
    {
        base.Reset();
        sortedChildren = null;
    }

    protected virtual List<Node> SortChildren()
    {
        return children.OrderByDescending(child => child.priority).ToList();
    }
}