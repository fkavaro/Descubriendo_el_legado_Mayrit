using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// BehaviourTree 
/// </summary>
public class BehaviourTree<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    public BehaviourTree(TController controller, string name = "BehaviourTree")
    : base(controller, name) { }

    public BehaviourTree(TController controller, Node<TController> child, string name = "BehaviourTree")
    : base(controller, name)
    {
        AddChild(child);
    }

    // protected override void DebugDecision()
    // {
    //     if (_currentChildId < children.Count)
    //         controller.stateText.text = children[_currentChildId].name;
    //     else
    //         controller.stateText.text = "None";
    // }

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