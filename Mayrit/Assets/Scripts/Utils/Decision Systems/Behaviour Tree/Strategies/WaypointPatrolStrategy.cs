using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// WaypointPatrolStrategy is a strategy for patrolling between a list of points using a NavMeshAgent.
/// </summary>
public class WaypointPatrolStrategy<TController> : AStrategy<TController>
where TController : ANPC<TController>
{
    readonly List<Transform> _patrolPoints;
    int _currentPatrolPointIndex;
    bool _isPathCalculated;

    public WaypointPatrolStrategy(TController controller, List<Transform> patrolPoints) : base(controller)
    {
        _patrolPoints = patrolPoints;
    }

    public override Node<TController>.Status Update()
    {
        if (_currentPatrolPointIndex >= _patrolPoints.Count)
            return Node<TController>.Status.Success;

        var target = _patrolPoints[_currentPatrolPointIndex];
        _controller.SetDestination(target.position);
        _controller.transform.LookAt(target);

        if (_isPathCalculated && _controller.HasArrivedAtDestination())
        {
            _currentPatrolPointIndex++;
            _isPathCalculated = false;
        }

        if (_controller.IsPathPending())
        {
            _isPathCalculated = true;
        }

        return Node<TController>.Status.Running;
    }

    public override void Reset()
    {
        _currentPatrolPointIndex = 0;
    }
}
