using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovementController
{
    #region CONSTANTS
    private const float POSITION_TOLERANCE = 0.1f;
    private const float POSITION_ZERO_THRESHOLD = 0.0001f;
    private const float MIN_SEPARATION = 0.5f;
    private const float DIRECTION_ZERO_THRESHOLD = 0.0001f;
    private const float NAVMESH_SAMPLE_DISTANCE = 2f;
    private const float ROTATION_COMPLETION_ANGLE = 0.5f;
    private const float DEFAULT_CLOSE_DISTANCE = 2f;
    private const int MIDDLE_POINT_SAMPLES = 9;
    private const float SEPARATION_BUFFER = 0.2f;
    private const float MAX_MIDPOINT_DISTANCE_FACTOR = 1.5f; // Fail if midpoint ends too far from partner
    #endregion

    #region PROPERTIES HELPERS
    public Spot DestinationSpot => _destinationSpot;
    public Vector3 DestinationPos => _destinationPos;
    public bool IsAgentValid => _agent != null && _agent.isOnNavMesh;
    public bool HasDestination => _destinationSpot != null || _destinationPos.sqrMagnitude >= POSITION_ZERO_THRESHOLD;
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
        _agent.angularSpeed = _npc.RotationSpeed * 100f;
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
        if (_destinationSpot == null && _destinationPos.sqrMagnitude < POSITION_ZERO_THRESHOLD)
            return false;

        float tolerance = Mathf.Max(POSITION_TOLERANCE, _positionLeniency);
        return Vector3.Distance(_destinationPos, position) < tolerance;
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
    public bool IsCloseToSpot(Spot targetSpot, float checkingDistance = DEFAULT_CLOSE_DISTANCE)
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
    public bool IsCloseToDestination(float checkingDistance = DEFAULT_CLOSE_DISTANCE)
    {
        if (!IsAgentValid || _agent.pathPending)
            return false;

        float effectiveDistance = GetEffectiveCheckingDistance(checkingDistance);
        return _agent.remainingDistance <= effectiveDistance;
    }

    /// <summary>
    /// Calculates the effective checking distance using the larger of provided distance or NPC's near distance.
    /// </summary>
    private float GetEffectiveCheckingDistance(float checkingDistance)
    {
        return Mathf.Max(checkingDistance, _npc.NearDistance);
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

        // Arrived - mark spot as occupied
        targetSpot.SetOccupied(true);

        // Handle rotation if required
        return !fixRotation || HandleRotationAtArrival(targetSpot.WorldDirection);
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

    /// <summary>
    /// Handles rotation at arrival point.
    /// Applies smooth rotation and returns true only when rotation is complete.
    /// </summary>
    /// <returns>True if rotation is already completed, false if still rotating.</returns>
    private bool HandleRotationAtArrival(Quaternion targetDirection)
    {
        if (HasRotationCompleted(targetDirection))
            return true;

        SmoothRotation(targetDirection);
        return false;
    }
    #endregion

    #region ROTATION METHODS
    public void ForceRotation(Quaternion rotation)
    {
        if (!IsAgentValid)
            return;

        _agent.updateRotation = false;
        _agent.transform.rotation = rotation;
    }

    public bool HasRotationCompleted(Quaternion targetRotation)
    {
        if (!IsAgentValid)
            return false;

        float currentYaw = _agent.transform.eulerAngles.y;
        float targetYaw = targetRotation.eulerAngles.y;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));
        return angleDifference < ROTATION_COMPLETION_ANGLE;
    }

    public void SmoothRotation(Quaternion direction)
    {
        if (!IsAgentValid)
            return;

        _agent.updateRotation = false;

        // Rotate only on Y-axis (XZ plane)
        float currentYaw = _agent.transform.eulerAngles.y;
        float targetYaw = direction.eulerAngles.y;
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, _npc.RotationSpeed * Time.deltaTime);

        Vector3 eulerAngles = _agent.transform.eulerAngles;
        eulerAngles.y = newYaw;
        _agent.transform.eulerAngles = eulerAngles;
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

    #region GO TO MIDDLE POINT
    /// <summary>
    /// Commands the NPC to move to the middle point between itself and another NPC, adjusted for separation.
    /// </summary>
    /// <returns>The destination position that was set.</returns>
    public bool GoToMiddlePoint(INPC otherNPC)
    {
        if (!IsAgentValid)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToMiddlePoint] {_npc.Name} agent is not valid.", _npc.GO);
            return false;
        }

        if (otherNPC == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToMiddlePoint] {_npc.Name} other NPC is null.", _npc.GO);
            return false;
        }

        Vector3 midPoint = FindMidpointTo(otherNPC);

        // Failure if middle point too far
        if (IsMiddlePointTooFar(midPoint, otherNPC))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToMiddlePoint] {_npc.Name} could not find valid middle point to {otherNPC.Name}.", _npc.GO);

            return false;
        }

        SetDestination(midPoint);
        return true;
    }

    /// <summary>
    /// Finds the best reachable midpoint destination between two NPCs.
    /// Prioritizes positions that balance distance and collision avoidance.
    /// </summary>
    public Vector3 FindMidpointTo(INPC otherNPC)
    {
        Vector3 posA = _npc.GO.transform.position;
        Vector3 posB = otherNPC.GO.transform.position;
        Vector3 directVector = posB - posA;

        // Handle case where NPCs are at same position
        if (directVector.sqrMagnitude < DIRECTION_ZERO_THRESHOLD)
            directVector = _npc.GO.transform.forward;

        directVector.Normalize();
        float distanceBetweenNPCs = Vector3.Distance(posA, posB);

        // Calculate desired separation
        float desiredSeparation = Mathf.Max(
            MIN_SEPARATION,
            _npc.AvoidanceRadius + otherNPC.AvoidanceRadius + _npc.StoppingDistance + otherNPC.StoppingDistance + SEPARATION_BUFFER);

        // Primary strategy: Move toward center, but back off by half separation
        Vector3 centerPoint = (posA + posB) * 0.5f;
        Vector3 correctedMidPoint = centerPoint - directVector * (desiredSeparation * 0.5f);
        if (CanReachPosition(correctedMidPoint, otherNPC, out Vector3 reachablePos))
            return reachablePos;

        // Secondary: Try pure midpoint
        if (CanReachPosition(centerPoint, otherNPC, out reachablePos))
            return reachablePos;

        // Tertiary: Sample along the line between NPCs for alternative positions
        int sampleCount = Mathf.Min(MIDDLE_POINT_SAMPLES, Mathf.CeilToInt(distanceBetweenNPCs / 0.5f));
        for (int i = 1; i <= sampleCount; i++)
        {
            float t = i / (float)(sampleCount + 1);
            Vector3 sample = Vector3.Lerp(posA, posB, t);

            if (CanReachPosition(sample, otherNPC, out reachablePos))
                return reachablePos;
        }

        // Quaternary: Try offset perpendicular to the line between NPCs
        Vector3 perpendicular = new(-directVector.z, directVector.y, directVector.x);
        Vector3 offsetPoint = centerPoint + perpendicular * (desiredSeparation * 0.5f);
        if (CanReachPosition(offsetPoint, otherNPC, out reachablePos))
            return reachablePos;

        // Final fallback: Current position if nothing else works
        return _agent.transform.position;
    }

    /// <summary>
    /// Checks if a position is reachable on the NavMesh and maintains proper separation.
    /// </summary>
    /// <returns>True if the position is reachable and valid for conversation placement.</returns>
    private bool CanReachPosition(Vector3 targetPos, INPC otherNPC, out Vector3 reachablePos)
    {
        // Sample position on NavMesh
        if (!NavMesh.SamplePosition(targetPos, out NavMeshHit hitLocation, _npc.MaxSamplingDistance, NavMesh.AllAreas))
        {
            reachablePos = _agent.transform.position;
            return false;
        }

        Vector3 sampledPosition = hitLocation.position;
        Vector3 otherNPCPos = otherNPC.GO.transform.position;

        // Minimum distance to other NPC to prevent overlap
        float minDistanceToOther = otherNPC.AvoidanceRadius + _npc.AvoidanceRadius + SEPARATION_BUFFER;
        float distanceToOther = Vector3.Distance(sampledPosition, otherNPCPos);

        // Reject if too close to other NPC
        if (distanceToOther < minDistanceToOther)
        {
            reachablePos = _agent.transform.position;
            return false;
        }

        // Verify there's a valid path to this position
        if (!HasValidPathTo(sampledPosition))
        {
            reachablePos = _agent.transform.position;
            return false;
        }

        reachablePos = sampledPosition;
        return true;
    }

    /// <summary>
    /// Checks if a valid path exists to the target position.
    /// </summary>
    private bool HasValidPathTo(Vector3 targetPos)
    {
        NavMeshPath testPath = new();
        if (!NavMesh.CalculatePath(_agent.transform.position, targetPos, _queryFilter, testPath))
            return false;

        // Path found, but check if it's complete (not partial)
        return testPath.status == NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Checks if a middle point position is valid and not too far from the other NPC.
    /// </summary>
    /// <returns>True if the middle point is too far and should be rejected.</returns>
    private bool IsMiddlePointTooFar(Vector3 candidate, INPC otherNPC)
    {
        if (otherNPC == null)
            return true;

        float desiredSeparation = Mathf.Max(
            MIN_SEPARATION,
            _npc.AvoidanceRadius + otherNPC.AvoidanceRadius + _npc.StoppingDistance + otherNPC.StoppingDistance + SEPARATION_BUFFER);

        float distanceToOther = Vector3.Distance(candidate, otherNPC.GO.transform.position);
        float maxAllowedDistance = desiredSeparation * MAX_MIDPOINT_DISTANCE_FACTOR;

        return distanceToOther > maxAllowedDistance;
    }
    #endregion
}
