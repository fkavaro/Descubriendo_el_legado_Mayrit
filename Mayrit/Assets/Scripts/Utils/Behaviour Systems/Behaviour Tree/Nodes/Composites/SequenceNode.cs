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
    public SequenceNode(IBehaviourEntity entity, string name = "Sequence", int priority = 0)
    : base(entity, name, priority) { }
    #endregion

    #region INHERITED METHODS
    public override Status UpdateNode()
    {
        while (_currentChildIdx < _children.Count)
        {
            Status status = _children[_currentChildIdx].UpdateNode();

            if (status == Status.Running)
                return Status.Running;

            if (status == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            // status == Success -> advance to next child
            _currentChildIdx++;
        }

        // All children succeeded
        Reset();
        return Status.Success;
    }
    #endregion
}