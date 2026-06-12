using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LeafNode is a node that has no children and executes an specific action.
/// </summary>
public class LeafNode : Node
{
    #region PROPERTIES
    readonly IStrategy _strategy;
    bool _started;
    #endregion

    #region CONSTRUCTOR
    public LeafNode(IBehaviourEntity entity, string name, IStrategy strategy, int priority = 0)
    : base(entity, name, priority)
    {
        _strategy = strategy;
    }
    #endregion

    #region INHERITED METHODS
    public override Status UpdateNode()
    {
        // Ensure Start() runs at the beginning of this node's execution
        if (!_started)
        {
            Status startStatus;

            try
            {
                startStatus = _strategy.Start();
            }
            catch (Exception)
            {
                startStatus = Status.Failure;
            }

            // If Start hasn't completed successfully yet, 
            // propagate its status (Running/Failure).
            if (startStatus != Status.Success)
                return startStatus;

            _started = true;
        }

        // Only call Update once Start() returned Success
        return _strategy.Update();
    }

    public override void Reset()
    {
        _strategy.Reset();
        _started = false;
    }

    public override void DebugDecision()
    {
        _behaviourEntity.CurrentAction = _nodeName;
    }
    #endregion
}