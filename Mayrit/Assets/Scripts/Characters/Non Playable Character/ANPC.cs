using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for NPC (Non-Playable Character).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public abstract class ANPC<T> : ABehaviourEntity<T>, INPC
where T : ABehaviourSystem
{
    #region EDITOR PROPERTIES
    [Header("Movement settings")]
    [Tooltip("Walking speed of the agent")]
    public float _walkSpeed = 2f;
    [Tooltip("Sprinting speed of the agent")]
    public float _sprintSpeed = 3f;
    [Tooltip("Rotation speed of the agent")]
    public float _rotationSpeed = 3f;
    [Tooltip("Distance to which the agent will avoid other agents"), Range(0.5f, 2f)]
    public float _avoidanceRadius = 0.7f;
    [Tooltip("Max distance from the random point to a point on the navmesh, for target position sampling")]
    public float _maxSamplingDistance = 1f;
    [Tooltip("Distance to which it's considered as arrived at destination")]
    public float _horizontalStoppingDistance = 0.3f;
    [Tooltip("Vertical margin (Y axis) to consider arrival")]
    public float _verticalStoppingDistance = 1.5f;
    [Tooltip("Distance to which it's close to the destination")]
    public float _nearDistance = 2f;
    public bool _isStopped = false;

    [Header("Avoidance")]
    [Tooltip("Base avoidance priority (0 = most important, 99 = least)")]
    public int _baseAvoidancePriority = 50;
    [Tooltip("Random +/- variance applied to base avoidance priority")]
    public int _avoidancePriorityVariance = 10;

    [Header("Energy Properties")]
    [Tooltip("Energy value"), Range(0, 100)]
    public float _energy = 100;

    [Header("Animation")]
    public Animator _animator;
    #endregion

    #region INTERNAL PROPERTIES
    NavMeshAgent _agent;
    public AnimationController _animationController;
    Spot _destinationSpot = null;
    public NavMeshAgent Agent => _agent;
    public AnimationController AnimationController => _animationController;
    #endregion

    #region MONOBEHAVIOUR
    protected override void Awake()
    {
        base.Awake();

        _animationController = new(this, this, _animator);

        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _walkSpeed;
        _agent.angularSpeed = _rotationSpeed * 100f;
        _agent.stoppingDistance = _horizontalStoppingDistance;
        _agent.radius = _avoidanceRadius;

        // Assign a randomized avoidance priority to reduce symmetric deadlocks between agents
        int offset = UnityEngine.Random.Range(-_avoidancePriorityVariance, _avoidancePriorityVariance + 1);
        _agent.avoidancePriority = Mathf.Clamp(_baseAvoidancePriority + offset, 0, 99);

        // Deactivate agent initially
        _agent.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!_agent.isOnNavMesh)
            return;

        // Stop moving if execution is paused
        if (IsExecutionPaused)
            _agent.isStopped = true;
        else
        {
            if (_isStopped)
                _agent.isStopped = true;
            else
                _agent.isStopped = false;
        }

        if (_agent.speed != _walkSpeed)
            _agent.speed = _walkSpeed;
    }
    #endregion

    #region PUBLIC METHODS
    public void SetDestination(Vector3 destinationPos)
    {
        if (_agent.destination == destinationPos) return;

        if (_destinationSpot != null)
        {
            _destinationSpot.SetOccupied(false); // Leave free current target spot
            _destinationSpot = null; // Reset the target spot
        }

        _agent.updateRotation = true;
        _agent.SetDestination(destinationPos);

        if (HasArrivedAtDestination()) return;
        else _animationController.ChangeToWalk();
    }

    public void SetDestinationSpot(Spot destinationSpot)
    {
        if (_destinationSpot == destinationSpot) return;

        SetDestination(destinationSpot.transform.position); // Set the target position for the NavMeshAgent

        _destinationSpot = destinationSpot;
    }

    public bool DestinationSpotIsOccupied()
    {
        if (_destinationSpot == null)
            return false;
        else
            return _destinationSpot.IsOccupied();
    }

    public bool IsCloseTo(Vector3 destination, float checkingDistance = 2f, bool fixRotation = false)
    {
        if (checkingDistance <= _nearDistance)
            checkingDistance = _nearDistance;

        if (Vector3.Distance(_agent.transform.position, destination) < checkingDistance)
        {
            if (fixRotation)
                _agent.transform.LookAt(destination);

            return true;
        }
        else
            return false;
    }

    public bool IsCloseTo(Spot spot, float checkingDistance = 2f, bool lookAtDestination = false)
    {
        return IsCloseTo(spot.transform.position, checkingDistance, lookAtDestination);
    }

    public bool IsCloseToDestination(float checkingDistance = 2f, bool fixRotation = false)
    {
        return IsCloseTo(_agent.destination, checkingDistance, fixRotation);
    }

    public bool HasArrivedAtDestination(bool fixRotation = false, bool fixPosition = false)
    {
        return HasArrivedAt(_agent.destination, fixRotation, fixPosition);
    }

    public bool HasArrivedAt(Spot spot, bool fixRotation = false, bool fixPosition = false)
    {
        return HasArrivedAt(spot.transform.position, fixRotation, fixPosition);
    }

    public bool HasArrivedAt(Vector3 destination, bool fixRotation = false, bool fixPosition = false)
    {
        // Compare horizontal (XZ) distance and allow a bigger vertical margin.
        Vector3 agentPos = _agent.transform.position;
        Vector3 destPos = destination;

        // Horizontal distance on XZ plane
        Vector2 agentXZ = new(agentPos.x, agentPos.z);
        Vector2 destXZ = new(destPos.x, destPos.z);
        float horizontalDist = Vector2.Distance(agentXZ, destXZ);

        // Vertical distance on Y axis
        float verticalDist = Mathf.Abs(agentPos.y - destPos.y);

        // Check if within stopping distances
        if (horizontalDist < _horizontalStoppingDistance && verticalDist < _verticalStoppingDistance)
        {
            if (_destinationSpot != null)
            {
                _destinationSpot.SetOccupied(true);

                if (fixRotation)
                    ForceRotation(_destinationSpot.WorldDirection);
                if (fixPosition)
                    _agent.transform.position = _destinationSpot.transform.position;
            }

            return true;
        }
        else return false;
    }

    public void ForceRotation(Vector3 lookDirection)
    {
        if (_agent.isOnNavMesh)
            _agent.updateRotation = false; // Disable automatic rotation

        _agent.transform.rotation = Quaternion.Euler(lookDirection);
    }
    public void ForceRotation(Quaternion rotation)
    {
        if (_agent.isOnNavMesh)
            _agent.updateRotation = false; // Disable automatic rotation

        _agent.transform.rotation = rotation;
    }

    public bool CanReachPosition(Vector3 targetPos, out Vector3 reachablePos)
    {
        NavMeshHit hitLocation;

        if (NavMesh.SamplePosition(targetPos, out hitLocation, _maxSamplingDistance, NavMesh.AllAreas))
        {
            reachablePos = hitLocation.position;
            return true;
        }
        else
        {
            reachablePos = Vector3.zero;
            return false;
        }
    }

    public bool CalculateRandomDestination(int samplingIterations, float areaRadious, Transform centerPoint, out Vector3 destination)
    {
        // Repeat until a random position in the navmesh is found
        for (int i = 0; i < samplingIterations; i++)
        {
            // Random point inside a circular area
            Vector3 randomPoint = centerPoint.position + UnityEngine.Random.insideUnitSphere * areaRadious;

            // Try to find a position in the navmesh area sampled from the random position
            if (CanReachPosition(randomPoint, out destination))
                return true;
        }

        // Hasn't found any reachable point in the navmesh
        destination = Vector3.zero;
        return false;
    }

    public void SetIfStopped(bool isStopped)
    {
        if (!_agent.isOnNavMesh)
        {
            Debug.LogError("IsStopped(): NavMeshAgent is not on a NavMesh.");
            return;
        }

        _agent.isStopped = isStopped;

        // Change animation accordingly
        if (isStopped)
            _animationController.ChangeToIdle();
        else
            _animationController.ChangeToWalk();
    }

    public bool IsPathPending()
    {
        return _agent.pathPending;
    }

    public Vector3 GetDestinationPos()
    {
        return _agent.destination;
    }

    public Spot GetDestinationSpot()
    {
        return _destinationSpot;
    }

    public bool IsDestination(Vector3 position)
    {
        return _agent.destination == position;
    }

    public bool IsDestination(Spot spot)
    {
        if (spot == null) return false;
        return _destinationSpot == spot;
    }

    public void PlaceAt(Spot spot)
    {
        if (spot == null)
        {
            Debug.LogError("PlaceAt(): destinationSpot is null.");
            return;
        }

        PlaceAt(spot.transform.position);

        // Set spot as occupied
        spot.SetOccupied(true);
        _destinationSpot = spot;
    }

    public void PlaceAt(Vector3 position)
    {
        // Place at position
        _agent.transform.position = position;

        // Leave free current target spot
        if (_destinationSpot != null)
        {
            _destinationSpot.SetOccupied(false);
            _destinationSpot = null;
        }
    }

    public void PlaceAtDestination()
    {
        PlaceAt(_agent.destination);
    }
    #endregion

    #region ENERGY METHODS
    public void ReduceEnergy(float amount)
    {
        if (_energy > 0)
            _energy -= amount;
    }

    public void IncreaseEnergy(float amount)
    {
        if (_energy < 100)
            _energy += amount;

        if (_energy > 100)
            _energy = 100;
    }

    public bool IsEnergyLow()
    {
        if (_energy <= 0)
        {
            _energy = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsEnergyAtMax()
    {
        return _energy >= 100;
    }
    #endregion
}

