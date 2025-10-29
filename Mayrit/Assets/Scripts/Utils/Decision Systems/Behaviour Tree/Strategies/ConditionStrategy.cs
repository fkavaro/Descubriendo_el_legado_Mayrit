using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionStrategy : AStrategy
{
    readonly Func<bool> _predicate;

    public ConditionStrategy(ANPC controller, LeafNode leafNode, Func<bool> predicate)
    : base(controller, leafNode)
    {
        _predicate = predicate;
    }

    public override Node.Status Update()
    {
        return _predicate() ? Node.Status.Success : Node.Status.Failure;
    }
}
