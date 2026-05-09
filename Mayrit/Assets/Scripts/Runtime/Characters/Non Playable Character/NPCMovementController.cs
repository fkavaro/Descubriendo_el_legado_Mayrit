using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovementController
{
    #region PROPERTIES HELPERS
    public Spot DestinationSpot => _destinationSpot;
    public Vector3 DestinationPos => _destinationPos;
    public bool IsAgentValid => _agent != null && _agent.isOnNavMesh;
    public bool IsAgentStopped
    {
        get => _agent.isStopped;
        set => SetIfAgentIsStopped(value);
    }
    #endregion

    #region PROPERTIES
    readonly INPC _npc;
    readonly NavMeshAgent _agent;
    readonly float _positionLeniency;
    readonly NavMeshQueryFilter _queryFilter;
    PlayableCharacter _player;

    Vector3 _destinationPos;
    Spot _destinationSpot;
    int _originalAvoidancePriority = -1;
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
        _agent.stoppingDistance = _npc.ArrivingDistance;
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

        _player = ServiceLocator.Instance.Get<PlayableCharacter>();
    }
    #endregion

    #region IF STOPPED METHODS
    void SetIfAgentIsStopped(bool isStopped)
    {
        if (!IsAgentValid)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.SetIfStopped] agent is not valid.", _npc.GO);
            return;
        }

        if (isStopped == _agent.isStopped)
            return;

        _agent.isStopped = isStopped;

        if (isStopped)
        {
            _originalAvoidancePriority = _agent.avoidancePriority;
            _agent.avoidancePriority = 0; // Highest priority - won't be pushed by others
        }
        else if (_originalAvoidancePriority > -1)
        {
            _agent.avoidancePriority = _originalAvoidancePriority;
            _originalAvoidancePriority = -1;
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
    public bool TrySetDestinationSpot(Spot targetSpot)
    {
        if (!IsAgentValid)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationSpot] agent is not valid.", _npc.GO);
            return false;
        }

        if (targetSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationSpot] target spot is null.", _npc.GO);
            return false;
        }

        // Already at this destination
        if (IsDestinationSpot(targetSpot))
            return true;

        // Set the position destination
        bool success = TrySetDestination(targetSpot.transform.position);
        if (success)
            _destinationSpot = targetSpot;

        return success;
    }

    /// <summary>
    /// Sets the destination to a specific world position.
    /// Handles NavMesh sampling, path calculation, and movement state updates.
    /// </summary>
    /// <returns>True if the destination was successfully set.</returns>
    public bool TrySetDestination(Vector3 targetPos)
    {
        if (!IsAgentValid)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestination] agent is not valid.", _npc.GO);
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
        if (_npc.DebugMode)
            Debug.LogWarning($"[{_npc.Name}.MovementController] could not sample a valid NavMesh position near target.", _npc.GO);

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

        if (_npc.DebugMode)
            Debug.LogWarning($"[{_npc.Name}.MovementController] could not calculate a valid path to target position.", _npc.GO);

        return false;
    }

    /// <summary>
    /// Applies movement configuration to the agent and NPC.
    /// Enables walking, sets path, and updates visual state.
    /// </summary>
    private void ApplyMovementState(NavMeshPath path)
    {
        _agent.updateRotation = true;
        _agent.SetPath(path);
        IsAgentStopped = false;

        // Update visuals
        _npc.CharacterModel.SetActive(true);
        _npc.AnimationController.ChangeToWalk();
    }

    public bool TrySetDestinationStall(out Spot stallSpot, bool onlyOpen = false)
    {
        // Fist try to get an open stall
        Stall newStall = _npc.Market.TryGetRandomStall(preferOpen: true, excludedStall: _npc.MarketStall);

        // If allowed, try to find any stall if no open stall found
        if (newStall == null && !onlyOpen)
            newStall = _npc.Market.TryGetRandomStall(preferOpen: false, excludedStall: _npc.MarketStall);

        if (newStall == null)
        {
            if (_npc.DebugMode && onlyOpen)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationStall] no open stalls available in market.", _npc.GO);
            else if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationStall] no stalls available in market.", _npc.GO);
            stallSpot = null;
            return false;
        }

        if (newStall.TooManyClientsWaiting)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationStall] selected stall has too many clients waiting.", _npc.GO);
            stallSpot = null;
            return false;
        }

        Spot newDestinationSpot = newStall.GetRandomAccessSpot();
        if (newDestinationSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationStall] selected stall has no available access spots.", _npc.GO);
            stallSpot = null;
            return false;
        }

        Stall previousStall = _npc.MarketStall;

        if (!_npc.MovementController.TrySetDestinationSpot(newDestinationSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TrySetDestinationStall] could not set destination to selected stall spot.", _npc.GO);
            stallSpot = null;
            return false;
        }

        // Prevent inflating waiting counts when switching stalls mid-routine
        if (previousStall != null && previousStall != newStall)
            previousStall.UnregisterClientWaiting(_npc);

        _npc.MarketStall = newStall;
        stallSpot = newDestinationSpot;
        _npc.MarketStall.RegisterClientWaiting(_npc);
        _npc.IsWaitingForAccess = false;
        return true;
    }
    #endregion

    #region HAS ARRIVED METHODS
    public bool HasArrivedAtDestinationSpot(Spot targetSpot, bool fixRotation = false)
    {
        // Validate agent and spot
        if (!IsAgentValid || targetSpot == null)
            return false;

        // Must be tracking this spot as destination
        if (!IsDestinationSpot(targetSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.HasArrivedAtDestinationSpot] checking arrival at untracked spot.", _npc.GO);
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

    public bool HasArrivedAtDestination()
    {
        return IsWithinDistanceToDestination(_npc.ArrivingDistance);
    }

    public bool HasArrivedAtPosition(Vector3 position)
    {
        return IsWithinDistanceToPosition(position, _npc.ArrivingDistance);
    }
    #endregion

    #region IS WITHIN DISTANCE METHODS
    private bool IsWithinDistanceToDestination(float checkingDistance)
    {
        if (!IsAgentValid || _agent.pathPending)
            return false;

        return _agent.remainingDistance <= checkingDistance;
    }

    private bool IsWithinDistanceToPosition(Vector3 position, float checkingDistance)
    {
        float effectiveDistance = checkingDistance;

        if (checkingDistance < 0f)
            effectiveDistance = _npc.NearDistance;

        float distanceToPosition = Vector3.Distance(_npc.GO.transform.position, position);
        return distanceToPosition <= effectiveDistance;
    }
    #endregion

    #region IS NEAR METHODS
    public bool IsNearDestinationSpot(Spot targetSpot)
    {
        if (!IsAgentValid || targetSpot == null)
            return false;

        // Must be tracking this spot as destination
        if (!IsDestinationSpot(targetSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.IsNearDestinationSpot] checking proximity to untracked spot.", _npc.GO);
            return false;
        }

        return IsNearDestination();
    }

    public bool IsNearDestination()
    {
        return IsWithinDistanceToDestination(_npc.NearDistance);
    }

    public bool IsNearPosition(Vector3 position)
    {
        return IsWithinDistanceToPosition(position, _npc.NearDistance);
    }

    public bool IsNearAnyWorkSpotPositionOf(Workplace worplace)
    {
        if (worplace == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.IsCloseToAnyWorkSpotOf] workplace is null.", _npc.GO);
            return false;
        }

        foreach (Spot spot in worplace.WorkSpots)
        {
            if (IsNearPosition(spot.transform.position))
                return true;
        }

        return false;
    }
    #endregion

    #region IS FAR METHODS
    public bool IsFarFromDestination()
    {
        return IsWithinDistanceToDestination(_npc.FarDistance);
    }

    public bool IsFarFromPosition(Vector3 position)
    {
        return IsWithinDistanceToPosition(position, _npc.FarDistance);
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

    public bool RotateSmoothlyTowards(GameObject GO)
    {
        if (!IsAgentValid)
            return false;

        if (GO == null)
            return false;

        Vector3 directionToOther = GO.transform.position - _agent.transform.position;
        directionToOther.y = 0f; // Keep only horizontal direction

        // Prevent zero vector error by checking magnitude
        if (directionToOther.sqrMagnitude < 0.001f)
            return true;

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

    public bool HasRotationCompleted(Quaternion targetRotation)
    {
        if (!IsAgentValid)
            return false;

        float currentYaw = _agent.transform.eulerAngles.y;
        float targetYaw = targetRotation.eulerAngles.y;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));
        return angleDifference < 2.0f;
    }
    #endregion

    #region PLACEMENT METHODS
    public void PlaceAtSpot(Spot spot, bool fixRotation = false)
    {
        if (spot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.PlaceAtSpot] spot is null.", _npc.GO);
            return;
        }

        PlaceAt(spot.transform.position);
        if (fixRotation) ForceRotation(spot.WorldDirection);
        spot.SetOccupied(true);
    }

    public void PlaceAt(Vector3 position)
    {
        // Try to sample a valid NavMesh position
        bool sampled = NavMesh.SamplePosition(position, out NavMeshHit hit, _npc.MaxSamplingDistance, NavMesh.AllAreas);
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
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.PlaceAt] exception when placing NPC: {e.Message}", _npc.GO);
        }
    }
    #endregion

    #region PLAYER PROXIMITY METHODS
    /// <summary>
    /// Checks if the player is too close and in front. If so, stops the agent and clears the current destination to trigger path recalculation.
    /// </summary>
    /// <returns>True if the player was too close and in front, and the agent was stopped.</returns>
    public bool CheckAndHandlePlayerProximity()
    {
        if (_player == null || !IsAgentValid) return false;

        float distanceToPlayer = Vector3.Distance(_npc.GO.transform.position, _player.GO.transform.position);

        if (distanceToPlayer < _npc.PlayerProximityRadius && IsPlayerInFront())
        {
            // Stop the agent
            IsAgentStopped = true;
            _npc.AnimationController.ChangeToIdle();
            return true;
        }
        else
        {
            // Resume movement if previously stopped due to player proximity
            if (IsAgentStopped)
            {
                IsAgentStopped = false;
                _npc.AnimationController.ChangeToPreviousAnimation();
            }

            return false;
        }
    }

    /// <summary>
    /// Checks if the player is in front of the NPC (within 90 degrees forward cone).
    /// </summary>
    private bool IsPlayerInFront()
    {
        Vector3 npcForward = _agent.transform.forward;
        Vector3 directionToPlayer = (_player.GO.transform.position - _agent.transform.position).normalized;

        // Dot product: 1 = directly ahead, 0 = perpendicular, -1 = behind
        // Use > 0 for a 90 degree cone in front
        float dotProduct = Vector3.Dot(npcForward, directionToPlayer);
        return dotProduct > 0f;
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
        return TrySetDestination(midPoint);
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
