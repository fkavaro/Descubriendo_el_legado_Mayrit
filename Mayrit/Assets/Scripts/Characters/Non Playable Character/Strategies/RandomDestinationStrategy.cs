using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomDestinationStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomDestinationStrategy : RandomPatrolStrategy
{
    #region PROPERTIES
    bool _destinationIsSet = false; // Dirty flag
    #endregion

    #region CONSTRUCTOR
    // Center point is the controller transform
    public RandomDestinationStrategy(INPC npc, int samplingIterations = 30, float areaRadious = 10f)
    : base(npc, npc.GO.transform, samplingIterations, areaRadious) { }
    #endregion

    #region INHERITED METHODS
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
    #endregion
}