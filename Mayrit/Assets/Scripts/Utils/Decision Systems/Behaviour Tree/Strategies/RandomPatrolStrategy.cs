using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomPatrolStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomPatrolStrategy<TController> : AStrategy<TController>
where TController : ANPC<TController>
{
    protected readonly Transform _centerPoint;
    protected readonly int _samplingIterations;
    protected readonly float _areaRadious;

    public RandomPatrolStrategy(TController controller, Transform centerPoint, int samplingIterations = 30, float areaRadious = 10f)
    : base(controller)
    {
        _centerPoint = centerPoint;
        _samplingIterations = samplingIterations;
        _areaRadious = areaRadious;
    }

    public override Node<TController>.Status Update()
    {
        if (_controller.HasArrivedAtDestination())
        {
            // Random destination is reachable
            if (_controller.CalculateRandomDestination(_samplingIterations, _areaRadious, _centerPoint, out Vector3 randomDestination))
                _controller.SetDestination(randomDestination);
            // It's not
            else
                return Node<TController>.Status.Failure;
        }
        return Node<TController>.Status.Running;
    }
}