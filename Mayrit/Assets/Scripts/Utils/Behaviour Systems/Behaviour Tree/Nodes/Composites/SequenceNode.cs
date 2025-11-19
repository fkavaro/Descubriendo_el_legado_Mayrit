using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SequenceNode is a composite node that executes its children in sequence.
/// Like a logical AND operation, it will return success only if all its children return success.
/// Continues to the next child if a child succeeds.
/// </summary>
public class SequenceNode : Node
{
    #region CONSTRUCTOR
    public SequenceNode(IBehaviourEntity entity, int priority = 0)
    : base(entity, "Sequence", priority) { }
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
                case Status.Failure:
                    Reset();
                    return Status.Failure;
                default: // Success
                    continue;
            }
        }

        Reset();
        return Status.Success;
    }
    #endregion
}