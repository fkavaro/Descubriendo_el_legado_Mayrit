using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomDestinationStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomDestinationStrategy : RandomPatrolStrategy
{
    bool _destinationIsSet = false; // Dirty flag

    // Center point is the controller transform
    public RandomDestinationStrategy(ANPC controller, LeafNode leafNode, int samplingIterations = 30, float areaRadious = 10f)
    : base(controller, leafNode, controller._agent.transform, samplingIterations, areaRadious) { }


    public override Node.Status Update()
    {
        // Destination not yet set
        if (!_destinationIsSet)
        {
            // Random destination is reachable, calculated from controller position
            if (_npc.CalculateRandomDestination(_samplingIterations, _areaRadious, _centerPoint, out Vector3 randomDestination))
            {
                _npc.SetDestination(randomDestination);
                _destinationIsSet = true;
            }
            // It's not
            else
                return Node.Status.Failure;
        }

        // Is close to destination
        if (_npc.IsCloseToDestination(1f))
        {
            if (_leafNode._debugMode) Debug.Log(_npc.Name + " arrived at random destination");
            return Node.Status.Success;
        }
        else // Hasn't arrived
        {
            // Reduce energy
            _npc.ReduceEnergy(Time.deltaTime);

            return Node.Status.Running;
        }
    }

    public override void Reset()
    {
        _destinationIsSet = false;
    }
}