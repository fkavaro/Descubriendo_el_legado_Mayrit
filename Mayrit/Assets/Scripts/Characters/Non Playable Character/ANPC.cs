using System;
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
    public float _stoppingDistance = 0.3f;
    [Tooltip("Distance to which it's close to the destination")]
    public float _nearDistance = 2f;
    public bool _isStopped = false;

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
        _agent.stoppingDistance = _stoppingDistance;
        _agent.radius = _avoidanceRadius;

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

    public bool IsCloseToDestination(float checkingDistance = 2f, bool fixRotation = false)
    {
        return IsCloseTo(_agent.destination, checkingDistance, fixRotation);
    }

    public bool HasArrivedAtDestination(bool fixRotation = true, bool fixPosition = true)
    {
        return HasArrived(_agent.destination, fixRotation, fixPosition);
    }

    public bool HasArrived(Spot spot, bool fixRotation = true, bool fixPosition = true)
    {
        return HasArrived(spot.transform.position, fixRotation, fixPosition);
    }

    public bool HasArrived(Vector3 destination, bool fixRotation = true, bool fixPosition = true)
    {
        if (Vector3.Distance(_agent.transform.position, destination) < _stoppingDistance)
        {
            //Debug.Log($"{gameObject.name} has arrived at {destination}.");

            if (_destinationSpot != null)
            {
                _destinationSpot.SetOccupied(true);

                if (fixRotation)
                    ForceRotation(_destinationSpot.DirectionVector); // Fix rotation to the target position
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
    }

    public bool IsPathPending()
    {
        return _agent.pathPending;
    }

    public Vector3 GetDestinationPos()
    {
        return _agent.destination;
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

