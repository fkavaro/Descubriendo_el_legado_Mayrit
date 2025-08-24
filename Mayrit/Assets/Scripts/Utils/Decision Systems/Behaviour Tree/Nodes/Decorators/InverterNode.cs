using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// InverterNode is a logical node that inverts its only child status.
/// Like a logical NOR operation, it will return success when the child returns failure.
/// </summary>
public class InverterNode : Node
{
    public InverterNode(IBehaviourControllable controllable, int priority = 0)
    : base(controllable, "Inverter", priority) { }

    public override Status UpdateNode()
    {
        switch (children[0].UpdateNode())
        {
            case Status.Success:
                return Status.Failure;
            case Status.Failure:
                return Status.Success;
            default:
                return Status.Running;
        }
    }
}