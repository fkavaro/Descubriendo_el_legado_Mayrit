using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SelectorNode is a composite node that that executes its children in sequence.
/// Like a logical OR operation, it will return success when a child return success.
/// Continues to the next child if a child fails.
/// </summary>
public class SelectorNode : Node
{
    #region CONSTRUCTOR
    public SelectorNode(IBehaviourEntity entity, int priority = 0)
    : base(entity, "Selector", priority) { }
    #endregion

    #region INHERITED METHODS
    public override Status UpdateNode()
    {
        foreach (var child in _children)
        {
            switch (child.UpdateNode())
            {
                case Status.Running:
                    return Status.Running;
                case Status.Success:
                    Reset();
                    return Status.Success;
                default: // Failure
                    continue;
            }
        }

        Reset();
        return Status.Failure;
    }
    #endregion
}