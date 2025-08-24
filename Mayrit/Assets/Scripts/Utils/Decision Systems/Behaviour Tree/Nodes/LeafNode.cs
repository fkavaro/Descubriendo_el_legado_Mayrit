using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// LeafNode is a node that has no children and executes an specific action.
/// </summary>
public class LeafNode : Node
{
    readonly AStrategy _strategy;

    public LeafNode(IBehaviourControllable controllable, string name, AStrategy strategy, int priority = 0)
    : base(controllable, name, priority)
    {
        _strategy = strategy;
    }

    protected override void DebugDecision()
    {
        //controller.nodeText.text = name;
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