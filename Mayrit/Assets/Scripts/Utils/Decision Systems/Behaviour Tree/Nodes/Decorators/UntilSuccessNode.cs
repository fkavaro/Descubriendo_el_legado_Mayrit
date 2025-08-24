using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// UntilSuccessNode is a node that continues running its only child as long as it doesn't return success.
/// </summary>
public class UntilSuccessNode : Node
{
    private readonly Node _child; // Make sure we have a reference to the child

    public UntilSuccessNode(IBehaviourControllable controllable, Node child, int priority = 0)
    : base(controllable, "UntilSuccess", priority)
    {
        AddChild(child); // Use the AddChild method to set the child
        _child = children[0]; // Store a direct reference for easier access
    }


    public override Status UpdateNode()
    {
        if (_child.UpdateNode() == Status.Success)
        {
            Reset();
            return Status.Success;
        }
        return Status.Running;
    }
}