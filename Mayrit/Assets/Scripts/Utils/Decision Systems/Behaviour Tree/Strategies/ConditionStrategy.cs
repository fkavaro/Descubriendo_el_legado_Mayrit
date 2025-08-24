using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionStrategy<TController> : AStrategy<TController>
where TController : MonoBehaviour
{
    readonly Func<bool> _predicate;

    public ConditionStrategy(ANPC<TController> controller, Func<bool> predicate)
    : base(controller)
    {
        _predicate = predicate;
    }

    public override Node<TController>.Status Update()
    {
        return _predicate() ? Node<TController>.Status.Success : Node<TController>.Status.Failure;
    }
}
