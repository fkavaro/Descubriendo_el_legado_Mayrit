using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionStrategy<TController> : AStrategy<TController>
where TController : ABehaviourController<TController>
{
    readonly Action _action;

    public ActionStrategy(TController controller, Action action) : base(controller)
    {
        _action = action;
    }

    public override Node<TController>.Status Update()
    {
        _action();
        return Node<TController>.Status.Success;
    }
}