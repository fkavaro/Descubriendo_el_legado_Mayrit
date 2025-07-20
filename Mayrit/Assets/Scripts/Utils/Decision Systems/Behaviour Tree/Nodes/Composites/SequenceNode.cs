using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SequenceNode is a composite node that executes its children in sequence.
/// Like a logical AND operation, it will return success only if all its children return success.
/// </summary>
public class SequenceNode<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    public SequenceNode(TController controller, int priority = 0)
    : base(controller, "Sequence", priority) { }

    public override Status UpdateNode()
    {
        // Execute every child
        if (_currentChildId < children.Count)
        {
            switch (children[_currentChildId].UpdateNode())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Failure:
                    Reset();
                    return Status.Failure;
                default: // Success
                    _currentChildId++; // Next one
                    // Success if it was the last, if not continue
                    return _currentChildId == children.Count ? Status.Success : Status.Running;
            }
        }
        Reset();
        return Status.Success;
    }
}