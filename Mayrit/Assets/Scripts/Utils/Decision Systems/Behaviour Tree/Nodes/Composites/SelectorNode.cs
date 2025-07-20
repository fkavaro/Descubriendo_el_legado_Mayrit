using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SelectorNode is a composite node that that executes its children in sequence.
/// Like a logical OR operation, it will return success when a child return success.
/// </summary>
public class SelectorNode<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    public SelectorNode(TController controller, int priority = 0)
    : base(controller, "Selector", priority) { }

    public override Status UpdateNode()
    {
        // Execute every child
        if (_currentChildId < children.Count)
        {
            switch (children[_currentChildId].UpdateNode())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    Reset();
                    return Status.Success;
                default: // Failure
                    _currentChildId++; // Next one
                    // Success if it was the last, if not continue
                    return _currentChildId == children.Count ? Status.Success : Status.Running;
            }
        }
        Reset();
        return Status.Failure;
    }
}