using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovementController
{
    #region CONSTANTS
    private const float NAVMESH_SAMPLE_DISTANCE = 2f;
    #endregion

    #region PROPERTIES HELPERS
    public Spot DestinationSpot => _destinationSpot;
    public Vector3 DestinationPos => _destinationPos;
    public bool IsAgentValid => _agent != null && _agent.isOnNavMesh;
    #endregion

    #region PROPERTIES
    readonly INPC _npc;
    readonly NavMeshAgent _agent;
    readonly float _positionLeniency;
    readonly NavMeshQueryFilter _queryFilter;

    Vector3 _destinationPos;
    Spot _destinationSpot;
    #endregion

    #region CONSTRUCTOR
    public NPCMovementController(INPC npc)
    {
        _npc = npc;
        _agent = _npc.Agent;
        _destinationSpot = null;

        // Assign a randomized avoidance priority to reduce symmetric deadlocks between agents
        int priorityOffset = UnityEngine.Random.Range(-_npc.AvoidancePriorityVariance, _npc.AvoidancePriorityVariance + 1);
        _agent.avoidancePriority = Mathf.Clamp(_npc.BaseAvoidancePriority + priorityOffset, 0, 99);

        // Assign randomized walk speed variance
        float speedOffset = UnityEngine.Random.Range(-_npc.WalkSpeedVariance, _npc.WalkSpeedVariance);
        _agent.speed = Mathf.Max(0.1f, _npc.WalkSpeed + speedOffset);

        // Set default values
        _npc.RotationSpeed *= 100f;
        _agent.angularSpeed = _npc.RotationSpeed;
        _agent.stoppingDistance = _npc.StoppingDistance;
        _agent.radius = _npc.AvoidanceRadius;

        // Configure NavMesh query filter for pathfinding calculations
        _queryFilter = new() { areaMask = _agent.areaMask, agentTypeID = _agent.agentTypeID };
        _positionLeniency = _agent.radius + _agent.stoppingDistance + _agent.height;

        // Deactivate agent initially
        _agent.enabled = false;
    }

    public void Reset()
    {
        if (_destinationSpot != null)
        {
            _destinationSpot.SetOccupied(false);
            _destinationSpot = null;
        }
        _destinationPos = Vector3.zero;
    }
    #endregion

    #region IF STOPPED METHODS
    /// <summary>
    /// Checks and updates the NPC's NavMeshAgent movement state.
    /// Should be called regularly to ensure proper movement behavior.
    /// </summary>
    public void CheckBehaviourExecution()
    {
        if (!IsAgentValid)
            return;

        // Stop moving if execution is paused, otherwise respect NPC stopped state
        _agent.isStopped = _npc.IsExecutionPaused || _npc.IsStopped;
    }

    public void SetIfStopped(bool isStopped)
    {
        if (!IsAgentValid)
        {
            Debug.LogWarning($"[SetIfStopped] {_npc.Name} agent is not valid.", _npc.GO);
            return;
        }

        if (_agent.isStopped != isStopped)
        {
            _agent.isStopped = isStopped;
            _npc.IsStopped = isStopped;
        }
    }
    #endregion

    #region DESTINATION METHODS
    public bool IsDestination(Vector3 position)
    {
        // No destination set if destination spot is null and position is zero
        if (_destinationSpot == null && _destinationPos.sqrMagnitude < 0.001f)
            return false;

        return Vector3.Distance(_destinationPos, position) < 0.1f;
    }

    public bool IsDestinationSpot(Spot spot)
    {
        if (spot == null) return false;
        return _destinationSpot == spot;
    }

    /// <summary>
    /// Clears the current destination spot and marks it as unoccupied.
    /// </summary>
    private void ClearDestinationSpot()
    {
        if (_destinationSpot != null)
        {
            _destinationSpot.SetOccupied(false);
            _destinationSpot = null;
        }
    }
    #endregion

    #region SET DESTINATION METHODS
    /// <summary>
    /// Sets the destination to a specific spot.
    /// </summary>
    /// <returns>True if the destination was successfully set.</returns>
    public bool SetDestinationSpot(Spot targetSpot)
    {
        if (!IsAgentValid)
        {
            Debug.LogWarning($"[SetDestinationSpot] {_npc.Name} agent is not valid.", _npc.GO);
            return false;
        }

        if (targetSpot == null)
        {
            Debug.LogWarning($"[SetDestinationSpot] {_npc.Name} target spot is null.", _npc.GO);
            return false;
        }

        // Already at this destination
        if (IsDestinationSpot(targetSpot))
            return true;

        // Set the position destination
        bool success = SetDestination(targetSpot.transform.position);
        if (success)
            _destinationSpot = targetSpot;

        return success;
    }

    /// <summary>
    /// Sets the destination to a specific world position.
    /// Handles NavMesh sampling, path calculation, and movement state updates.
    /// </summary>
    /// <returns>True if the destination was successfully set.</returns>
    public bool SetDestination(Vector3 targetPos)
    {
        if (!IsAgentValid)
        {
            Debug.LogWarning($"[SetDestination] {_npc.Name} agent is not valid.", _npc.GO);
            return false;
        }

        // Already at destination
        if (IsDestination(targetPos))
            return true;

        // Sample and validate NavMesh position
        if (!TrySampleNavMeshPosition(ref targetPos))
            return false;

        // Calculate and validate path
        NavMeshPath path = new();
        if (!TryCalculatePath(targetPos, path))
            return false;

        // Clear previous destination spot
        ClearDestinationSpot();

        // Update destination position
        _destinationPos = targetPos;

        // Apply movement configuration
        ApplyMovementState(path);

        return true;
    }

    /// <summary>
    /// Attempts to sample a valid NavMesh position near the target.
    /// </summary>
    /// <returns>True if a valid NavMesh position was sampled or leniency is disabled.</returns>
    private bool TrySampleNavMeshPosition(ref Vector3 targetPos)
    {
        // Skip sampling if leniency is not configured
        if (_positionLeniency <= 0f)
            return true;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, _positionLeniency, _queryFilter))
        {
            targetPos = hit.position;
            return true;
        }

        Debug.LogWarning($"[SetDestination] {_npc.Name} could not sample NavMesh position near target.", _npc.GO);
        return false;
    }

    /// <summary>
    /// Attempts to calculate a valid path to the target position.
    /// </summary>
    /// <returns>True if a valid path was calculated.</returns>
    private bool TryCalculatePath(Vector3 targetPos, NavMeshPath path)
    {
        if (NavMesh.CalculatePath(_agent.transform.position, targetPos, _queryFilter, path))
            return true;

        Debug.LogWarning($"[SetDestination] {_npc.Name} could not calculate a valid path to target.", _npc.GO);
        return false;
    }

    /// <summary>
    /// Applies movement configuration to the agent and NPC.
    /// Enables walking, sets path, and updates visual state.
    /// </summary>
    private void ApplyMovementState(NavMeshPath path)
    {
        _agent.updateRotation = true;
        _agent.isStopped = false;
        _npc.IsStopped = false;
        _agent.SetPath(path);

        // Update visuals
        _npc.CharacterModel.SetActive(true);
        _npc.AnimationController.ChangeToWalk();
    }
    #endregion

    #region IS CLOSE METHODS
    /// <summary>
    /// Checks if the NPC is close to a specific destination spot.
    /// </summary>
    /// <returns>True if close to the destination spot and the spot is the current destination.</returns>
    public bool IsCloseToSpot(Spot targetSpot, float checkingDistance = -1f)
    {
        if (!IsAgentValid || targetSpot == null)
            return false;

        // Must be tracking this spot as destination
        if (!IsDestinationSpot(targetSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[IsCloseToSpot] {_npc.Name} checking distance to untracked spot.", _npc.GO);
            return false;
        }

        return IsCloseToDestination(checkingDistance);
    }

    /// <summary>
    /// Checks if the NPC is close to its current destination.
    /// </summary>
    /// <returns>True if close to the destination and path is calculated.</returns>
    public bool IsCloseToDestination(float checkingDistance = -1f)
    {
        if (!IsAgentValid || _agent.pathPending)
            return false;

        float effectiveDistance = checkingDistance;

        if (checkingDistance < 0f)
            effectiveDistance = _npc.NearDistance;

        return _agent.remainingDistance <= effectiveDistance;
    }

    public bool IsCloseToAnyWorkSpotOf(Workplace worplace)
    {
        if (worplace == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[IsCloseToAnyWorkSpotOf] {_npc.Name} workplace is null.", _npc.GO);
            return false;
        }

        foreach (Spot spot in worplace.WorkSpots)
        {
            if (IsCloseToPosition(spot.transform.position))
                return true;
        }

        if (_npc.DebugMode)
            Debug.LogWarning($"[IsCloseToAnyWorkSpotOf] {_npc.Name} not close to any work spot of workplace.", _npc.GO);

        return false;
    }

    public bool IsCloseToPosition(Vector3 position, float checkingDistance = -1f)
    {
        float effectiveDistance = checkingDistance;

        if (checkingDistance < 0f)
            effectiveDistance = _npc.NearDistance;

        float distanceToPosition = Vector3.Distance(_npc.GO.transform.position, position);
        return distanceToPosition <= effectiveDistance;
    }
    #endregion

    #region HAS ARRIVED METHODS
    /// <summary>
    /// Checks if the NPC has arrived at a specific destination spot.
    /// Marks the spot as occupied and optionally fixes rotation upon arrival.
    /// </summary>
    /// <returns>True if arrived at the spot and (if fixRotation) rotation is completed.</returns>
    public bool HasArrivedAtSpot(Spot targetSpot, bool fixRotation = false)
    {
        // Validate agent and spot
        if (!IsAgentValid || targetSpot == null)
            return false;

        // Must be tracking this spot as destination
        if (!IsDestinationSpot(targetSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[HasArrivedAtSpot] {_npc.Name} checking arrival at untracked spot.", _npc.GO);
            return false;
        }

        // Check if arrived at destination
        if (!HasArrivedAtDestination())
            return false;

        // Handle rotation if required
        if (!fixRotation || RotateSmoothlyTowards(targetSpot.WorldDirection))
        {
            // Arrived - mark spot as occupied
            targetSpot.SetOccupied(true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the NPC has reached its current destination.
    /// </summary>
    /// <returns>True if the agent has reached its destination within stopping distance.</returns>
    public bool HasArrivedAtDestination()
    {
        if (!IsAgentValid || _agent.pathPending)
            return false;

        return _agent.remainingDistance <= _agent.stoppingDistance;
    }
    #endregion

    #region ROTATION METHODS
    public void ForceRotation(Quaternion targetRotation)
    {
        if (!IsAgentValid)
            return;

        _agent.updateRotation = false;
        _agent.transform.rotation = targetRotation;
    }

    public bool HasRotationCompleted(Quaternion targetRotation)
    {
        if (!IsAgentValid)
            return false;

        float currentYaw = _agent.transform.eulerAngles.y;
        float targetYaw = targetRotation.eulerAngles.y;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));
        return angleDifference < 2.0f;
    }

    public bool RotateSmoothlyTowards(GameObject GO)
    {
        if (!IsAgentValid)
            return false;

        if (GO == null)
            return false;

        Vector3 directionToOther = GO.transform.position - _agent.transform.position;
        directionToOther.y = 0f; // Keep only horizontal direction

        directionToOther.Normalize();
        return RotateSmoothlyTowards(Quaternion.LookRotation(directionToOther));
    }

    public bool RotateSmoothlyTowards(Quaternion targetRotation)
    {
        if (!IsAgentValid)
            return false;

        if (HasRotationCompleted(targetRotation))
            return true;

        _agent.updateRotation = false;

        // Rotate only on Y-axis (XZ plane)
        float currentYaw = _agent.transform.eulerAngles.y;
        float targetYaw = targetRotation.eulerAngles.y;
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, _npc.RotationSpeed * Time.deltaTime);

        Vector3 eulerAngles = _agent.transform.eulerAngles;
        eulerAngles.y = newYaw;
        _agent.transform.eulerAngles = eulerAngles;

        return false;
    }
    #endregion

    #region PLACEMENT METHODS
    public void PlaceAtSpot(Spot spot)
    {
        if (spot == null)
        {
            Debug.LogWarning($"[PlaceAtSpot] {_npc.Name} spot is null.", _npc.GO);
            return;
        }

        PlaceAt(spot.transform.position);
        spot.SetOccupied(true);
    }

    public void PlaceAt(Vector3 position)
    {
        // Try to sample a valid NavMesh position
        bool sampled = NavMesh.SamplePosition(position, out NavMeshHit hit, NAVMESH_SAMPLE_DISTANCE, NavMesh.AllAreas);
        Vector3 placementPos = sampled ? hit.position : position;

        // Place transform
        _npc.GO.transform.position = placementPos;

        // Enable agent and warp if on NavMesh
        try
        {
            if (!_npc.Agent.enabled)
                _npc.Agent.enabled = true;

            if (sampled && _npc.Agent.isOnNavMesh)
                _npc.Agent.Warp(placementPos);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[PlaceAt] {_npc.Name} failed to enable/warp agent: {e.Message}", _npc.GO);
        }
    }
    #endregion

    #region GO TO MIDDLE POINT METHODS
    /// <summary>
    /// Commands the NPC to move to the middle point between itself and another NPC, adjusted for separation.
    /// </summary>
    /// <returns>The destination position that was set.</returns>
    public bool GoToMiddlePoint(INPC otherNPC)
    {
        if (!IsAgentValid)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePoint] agent is not valid.", _npc.GO);
            return false;
        }

        if (otherNPC == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePoint] other NPC is null.", _npc.GO);
            return false;
        }

        Vector3 midPoint = FindMidpointTo(otherNPC);
        return SetDestination(midPoint);
    }

    /// <summary>
    /// Finds the best reachable midpoint destination between two NPCs.
    /// </summary>
    public Vector3 FindMidpointTo(INPC otherNPC)
    {
        Vector3 posA = _npc.GO.transform.position;
        Vector3 posB = otherNPC.GO.transform.position;
        Vector3 centerPoint = (posA + posB) * 0.5f;

        // Try center point on NavMesh
        if (NavMesh.SamplePosition(centerPoint, out NavMeshHit hit, _npc.MaxSamplingDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback: move slightly toward other NPC from current position
        Vector3 directionToOther = (posB - posA).normalized;
        Vector3 stepToward = posA + directionToOther * Mathf.Min(1f, Vector3.Distance(posA, posB) * 0.3f);

        if (NavMesh.SamplePosition(stepToward, out hit, _npc.MaxSamplingDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Final fallback: stay at current position
        return _agent.transform.position;
    }
    #endregion
}
