using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for all nodes in the behaviour tree.
/// </summary>
public class Node : ADecisionSystem
{
    public enum Status
    {
        Running, // In progress
        Success,
        Failure
    }

    public readonly string name;
    public readonly int priority;

    public readonly List<Node> children = new();
    protected int _currentChildId;
    public Status status;

    public Node(IBehaviourControllable controllable, string name = "Node", int priority = 0)
    : base(controllable)
    {
        this.name = name;
        this.priority = priority;
    }

    #region INHERITED METHODS
    protected override void DebugDecision()
    {
        if (_currentChildId < children.Count)
            children[_currentChildId].DebugDecision();
        //else
        //controller.nodeText.text = "";
    }

    public override void Update()
    {
        DebugDecision();
        if (!_controllable.IsExecutionPaused)
            status = UpdateNode();
    }

    public override void Reset()
    {
        _currentChildId = 0;
        foreach (var child in children)
        {
            child.Reset();
        }
    }
    #endregion

    #region PUBLIC	METHODS
    public void AddChild(Node child)
    {
        children.Add(child);
    }

    public virtual Status UpdateNode()
    {
        return children[_currentChildId].UpdateNode();
    }
    #endregion
}
