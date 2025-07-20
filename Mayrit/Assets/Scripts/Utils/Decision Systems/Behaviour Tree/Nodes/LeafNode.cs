using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LeafNode is a node that has no children and executes an specific action.
/// </summary>
public class LeafNode<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    readonly AStrategy<TController> _strategy;

    public LeafNode(TController controller, string name, AStrategy<TController> strategy, int priority = 0)
    : base(controller, name, priority)
    {
        _strategy = strategy;
    }

    protected override void DebugDecision()
    {
        controller.nodeText.text = name;
    }

    public override Status UpdateNode()
    {
        return _strategy.Update();
    }

    public override void Reset()
    {
        _strategy.Reset();
    }
}