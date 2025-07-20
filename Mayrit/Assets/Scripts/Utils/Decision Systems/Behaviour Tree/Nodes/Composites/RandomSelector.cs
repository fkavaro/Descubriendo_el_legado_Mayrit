using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SelectorNode is a composite node that that executes its children randomly.
/// Like a logical OR operation, it will return success when a child return success.
/// </summary>
public class RanndomSelectorNode<TController> : PrioritySelectorNode<TController>
where TController : ABehaviourController<TController>
{
    public RanndomSelectorNode(TController controller, int priority = 0)
    : base(controller, priority) { }

    protected override List<Node<TController>> SortChildren()
    {
        return children.Shuffle().ToList();
    }
}