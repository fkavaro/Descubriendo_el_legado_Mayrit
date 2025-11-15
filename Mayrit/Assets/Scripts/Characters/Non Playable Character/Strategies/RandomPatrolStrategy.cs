using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// RandomPatrolStrategy is a strategy for moving constantly between random points using a NavMeshAgent.
/// </summary>
public class RandomPatrolStrategy : AStrategy
{
    #region PROPERTIES
    protected readonly Transform _centerPoint;
    protected readonly int _samplingIterations;
    protected readonly float _areaRadious;
    #endregion

    #region CONSTRUCTOR
    public RandomPatrolStrategy(INPC npc, Transform centerPoint, int samplingIterations = 30, float areaRadious = 10f)
    : base(npc)
    {
        _centerPoint = centerPoint;
        _samplingIterations = samplingIterations;
        _areaRadious = areaRadious;
    }
    #endregion

    #region INHERITED METHODS
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
    #endregion
}