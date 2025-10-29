using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomPatrolStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomPatrolStrategy : AStrategy
{
    protected readonly Transform _centerPoint;
    protected readonly int _samplingIterations;
    protected readonly float _areaRadious;

    public RandomPatrolStrategy(ANPC controller, LeafNode leadNode, Transform centerPoint, int samplingIterations = 30, float areaRadious = 10f)
    : base(controller, leadNode)
    {
        _centerPoint = centerPoint;
        _samplingIterations = samplingIterations;
        _areaRadious = areaRadious;
    }

    public override Node.Status Update()
    {
        if (_npc.HasArrivedAtDestination())
        {
            // Random destination is reachable
            if (_npc.CalculateRandomDestination(_samplingIterations, _areaRadious, _centerPoint, out Vector3 randomDestination))
                _npc.SetDestination(randomDestination);
            // It's not
            else
                return Node.Status.Failure;
        }
        return Node.Status.Running;
    }
}