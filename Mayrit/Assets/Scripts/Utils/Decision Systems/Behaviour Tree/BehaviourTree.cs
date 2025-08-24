using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree(IBehaviourControllable controllable, string name = "BehaviourTree")
    : base(controllable, name) { }

    public BehaviourTree(IBehaviourControllable controllable, Node child, string name = "BehaviourTree")
    : base(controllable, name)
    {
        AddChild(child);
    }

    public override Status UpdateNode()
    {
        while (_currentChildId < children.Count)
        {
            var status = children[_currentChildId].UpdateNode();

            if (status != Status.Success)
                return status;

            _currentChildId++;
        }
        return Status.Success;
    }
}