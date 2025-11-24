using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for all nodes in the behaviour tree.
/// </summary>
public class Node : ABehaviourSystem
{
    public enum Status
    {
        Running, // In progress
        Success,
        Failure
    }

    #region PROPERTIES
    public readonly string _nodeName;
    public readonly int _priority;

    public readonly List<Node> _children = new();
    protected int _currentChildId;
    public Status _status;
    #endregion

    #region CONSTRUCTOR
    public Node(IBehaviourEntity entity, string nodeName = "Node", int priority = 0)
    : base(entity)
    {
        _nodeName = nodeName;
        _priority = priority;
    }
    #endregion

    #region INHERITED METHODS  
    /// <summary>
    /// Resets all children nodes.
    /// </summary>
    public override void Reset()
    {
        _currentChildId = 0;
        foreach (var child in _children)
        {
            child.Reset();
        }
    }

    /// <summary>
    /// Debugs the current node of the behaviour tree.
    /// </summary>
    public override void DebugDecision()
    {
        if (_currentChildId < _children.Count)
            _children[_currentChildId].DebugDecision();
    }
    #endregion

    #region MONOBEHAVIOUR
    public override void Update()
    {

        if (!_behaviourEntity.IsExecutionPaused)
        {
            _status = UpdateNode();

            if (_status == Status.Running)
            {
                DebugDecision();
            }
        }
    }
    #endregion

    #region PUBLIC	METHODS
    public void AddChild(Node child)
    {
        _children.Add(child);
    }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual Status UpdateNode()
    {
        return _children[_currentChildId].UpdateNode();
    }
    #endregion
}
