using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SelectorNode is a composite node that that executes its children randomly.
/// Like a logical OR operation, it will return success when a child return success.
/// </summary>
public class RanndomSelectorNode : PrioritySelectorNode
{
    public RanndomSelectorNode(IBehaviourControllable controllable, int priority = 0)
    : base(controllable, priority) { }

    protected override List<Node> SortChildren()
    {
        return children.Shuffle().ToList();
    }
}