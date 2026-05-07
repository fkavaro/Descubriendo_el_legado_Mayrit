using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SuccederNode : Node
{
    #region CONSTRUCTOR
    public SuccederNode(IBehaviourEntity entity, string nodeName = "Succeder", int priority = 0)
    : base(entity, nodeName, priority) { }
    #endregion

    #region INHERITED METHODS
    public override Status UpdateNode()
    {
        switch (_children[0].UpdateNode())
        {
            case Status.Running:
                return Status.Running;
            default: // Always return success
                return Status.Success;
        }
    }
    #endregion
}