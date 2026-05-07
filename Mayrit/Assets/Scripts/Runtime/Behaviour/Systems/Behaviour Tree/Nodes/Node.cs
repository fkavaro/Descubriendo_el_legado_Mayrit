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
        Running,
        Success,
        Failure
    }

    #region PROPERTIES
    public readonly string _nodeName;
    public readonly int _priority;

    public readonly List<Node> _children = new();
    protected int _currentChildIdx;
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
    /// Resets current node index and all its children to their initial state
    /// </summary>
    public override void Reset()
    {
        _currentChildIdx = 0;

        foreach (Node child in _children)
            child.Reset();
    }

    /// <summary>
    /// Debugs the current node of the behaviour tree.
    /// </summary>
    public override void DebugDecision()
    {
        if (_currentChildIdx < _children.Count)
            _children[_currentChildIdx].DebugDecision();
    }
    #endregion

    #region LIFE CYCLE
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

    public void SetCurrentChild(int childIdx)
    {
        if (childIdx >= 0 && childIdx < _children.Count)
            _currentChildIdx = childIdx;
        else
            Debug.LogWarning("[" + _nodeName + "] Trying to set current child index to " + childIdx + " but it is out of bounds.");
    }

    public void SetRandomCurrentChild()
    {
        if (_children.Count > 0)
            _currentChildIdx = UnityEngine.Random.Range(0, _children.Count);
    }

    public Node GetCurrentRandomChild()
    {
        if (_children.Count <= 0) return null;

        SetRandomCurrentChild();
        return _children[_currentChildIdx];
    }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual Status UpdateNode()
    {
        return _children[_currentChildIdx].UpdateNode();
    }
    #endregion
}
