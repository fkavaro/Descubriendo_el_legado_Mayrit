using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfiniteLoopNode : Node
{
    #region CONSTRUCTORs
    private readonly Node _child;
    #endregion

    #region CONSTRUCTORS
    public InfiniteLoopNode(IBehaviourEntity entity)
    : base(entity, "InfiniteLoop") { }

    public InfiniteLoopNode(IBehaviourEntity entity, Node child)
    : base(entity, "InfiniteLoop")
    {
        AddChild(child);
        _child = child;
    }
    #endregion

    #region INHERITED METHODS
    public override Status UpdateNode()
    {
        if (_child == null)
            return Status.Failure;

        _child.UpdateNode();

        return Status.Running;
    }
    #endregion
}