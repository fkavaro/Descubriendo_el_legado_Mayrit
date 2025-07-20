using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SuccederNode<TController> : Node<TController>
where TController : ABehaviourController<TController>
{
    public SuccederNode(TController controller, int priority = 0)
    : base(controller, "Successer", priority) { }

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