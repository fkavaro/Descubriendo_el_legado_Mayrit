using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomDestinationStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomDestinationStrategy<TController> : RandomPatrolStrategy<TController>
where TController : ANPC<TController>
{
    bool _destinationIsSet = false; // Dirty flag

    // Center point is the controller transform
    public RandomDestinationStrategy(TController controller, int samplingIterations = 30, float areaRadious = 10f)
    : base(controller, controller.transform, samplingIterations, areaRadious) { }


    public override Node<TController>.Status Update()
    {
        // Destination not yet set
        if (!_destinationIsSet)
        {
            // Random destination is reachable, calculated from controller position
            if (_controller.CalculateRandomDestination(_samplingIterations, _areaRadious, _centerPoint, out Vector3 randomDestination))
            {
                _controller.SetDestination(randomDestination);
                _destinationIsSet = true;
            }
            // It's not
            else
                return Node<TController>.Status.Failure;
        }

        // Is close to destination
        if (_controller.IsCloseToDestination(1f))
        {
            if (_controller.debugMode) Debug.Log(_controller.name + " arrived at random destination");
            return Node<TController>.Status.Success;
        }
        else // Hasn't arrived
        {
            // Reduce energy
            _controller.ReduceEnergy(Time.deltaTime);

            return Node<TController>.Status.Running;
        }
    }

    public override void Reset()
    {
        _destinationIsSet = false;
    }
}