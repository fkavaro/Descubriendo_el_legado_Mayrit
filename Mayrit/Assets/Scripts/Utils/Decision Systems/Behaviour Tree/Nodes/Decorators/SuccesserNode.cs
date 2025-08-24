using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SuccederNode : Node
{
    public SuccederNode(IBehaviourControllable controllable, int priority = 0)
    : base(controllable, "Successer", priority) { }

    public override Status UpdateNode()
    {
        switch (children[0].UpdateNode())
        {
            case Status.Running:
                return Status.Running;
            default: // Always return success
                return Status.Success;
        }
    }
}