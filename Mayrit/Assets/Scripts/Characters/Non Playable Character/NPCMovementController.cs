using System;
using System.Data.Common;
using UnityEngine;
using UnityEngine.AI;

public class NPCMovementController
{
    #region PROPERTIES
    readonly INPC _npc;
    readonly NavMeshAgent _agent;
    readonly float _positionLeniency = 0f; // Allowable leniency when setting destination
    readonly NavMeshQueryFilter _queryFilter;

    Vector3 _destinationPos;
    Spot _destinationSpot;

    float ArrivedHorizontalDistance => _npc.ArrivedDistance.x;
    float ArrivedVerticalDistance => _npc.ArrivedDistance.y;
    float NearHorizontalDistance => _npc.NearDistance.x;
    float NearVerticalDistance => _npc.NearDistance.y;
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
        _agent.stoppingDistance = ArrivedHorizontalDistance;
        _agent.radius = _npc.AvoidanceRadius;

        // Configure NavMesh query filter for pathfinding calculations
        _queryFilter = new() { areaMask = _agent.areaMask, agentTypeID = _agent.agentTypeID };
        _positionLeniency = _agent.radius + _agent.stoppingDistance + _agent.height;

        // Deactivate agent initially
        _agent.enabled = false;
    }
    #endregion

    #region IF STOPPED METHODS
    /// <summary>
    /// Checks and updates the NPC's NavMeshAgent movement state.
    /// Should be called regularly to ensure proper movement behavior.
    /// </summary>
    public void CheckBehaviourExecution()
    {
        if (!_agent.isOnNavMesh)
            return;

        // Stop moving if execution is paused
        if (_npc.IsExecutionPaused)
            _agent.isStopped = true;
        else
            _agent.isStopped = _npc.IsStopped;
    }

    public void SetIfStopped(bool isStopped)
    {
        if (_agent == null)
        {
            Debug.LogError(_npc.Name + ", IsStopped(): NavMeshAgent is null.");
            return;
        }

        if (!_agent.isOnNavMesh)
        {
            Debug.LogError(_npc.Name + ", IsStopped(): NavMeshAgent is not on a NavMesh.");
            return;
        }

        _npc.IsStopped = isStopped;
    }
    #endregion

    #region DESTINATION METHODS
    public Vector3 GetDestinationPos()
    {
        return _destinationPos;
    }

    public Spot GetDestinationSpot()
    {
        return _destinationSpot;
    }

    public bool IsDestination(Vector3 position)
    {
        // Difference is minimal
        return Vector3.Distance(_destinationPos, position) < 0.1f;
    }

    public bool IsDestination(Spot spot)
    {
        if (spot == null) return false;
        return _destinationSpot == spot;
    }

    public bool IsDestinationSpotOccupied()
    {
        if (_destinationSpot == null)
            return false;
        else
            return _destinationSpot.IsOccupied();
    }
    #endregion

    #region SET DESTINATION METHODS
    /// <summary>
    /// Sets the target destination for the NPC's NavMeshAgent.
    /// Manually calculates the path to the target position and assigns it to the agent.
    /// If a position leniency is defined, it attempts to sample a valid NavMesh position near the target.
    /// If the NPC is already at the destination, it does not change the animation state.
    /// SetDestination() is not used because it causes jittering in the movement (in Unity 6)
    /// </summary>
    public void SetDestination(Vector3 targetPosition)
    {
        if (IsDestination(targetPosition)) return;

        NavMeshPath path = new();

        if (_positionLeniency != 0f)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, _positionLeniency, _queryFilter))
                targetPosition = hit.position; // Adjust destination to sampled position
            else
            {
                Debug.LogWarning(_npc.Name + ": SetDestination() - Could not sample position near target destination within leniency.");
                return;
            }
        }

        bool canSetPath = NavMesh.CalculatePath(_agent.transform.position, targetPosition, _queryFilter, path);

        if (!canSetPath)
        {
            Debug.LogWarning(_npc.Name + ": SetDestination() - Could not calculate path to target destination.");
            return;
        }

        if (_destinationSpot != null)
        {
            _destinationSpot.SetOccupied(false); // Leave free current target spot
            _destinationSpot = null; // Reset the target spot
        }

        _destinationPos = targetPosition;
        _agent.updateRotation = true;
        _agent.SetPath(path);

        if (HasArrivedAtDestination()) return;
        else _npc.AnimationController.ChangeToWalk();
    }

    public void SetDestinationSpot(Spot destinationSpot)
    {
        if (_destinationSpot == destinationSpot) return;

        SetDestination(destinationSpot.transform.position); // Set the target position for the NavMeshAgent

        _destinationSpot = destinationSpot;
    }
    #endregion

    #region IS CLOSE METHODS
    public bool IsCloseTo(Spot spot, float horizontalDistance = 2f, float verticalDistance = 3.5f)
    {
        return IsCloseTo(spot.transform.position, horizontalDistance, verticalDistance);
    }

    public bool IsCloseToDestination(float horizontalDistance = 2f, float verticalDistance = 3.5f)
    {
        return IsCloseTo(_destinationPos, horizontalDistance, verticalDistance);
    }

    public bool IsCloseTo(Vector3 destination, float horizontalDistance = 2f, float verticalDistance = 3.5f)
    {
        Vector3 agentPos = _agent.transform.position;
        Vector3 destPos = destination;

        // Horizontal distance on XZ plane
        Vector2 agentXZ = new(agentPos.x, agentPos.z);
        Vector2 destXZ = new(destPos.x, destPos.z);
        float horizontalDist = Vector2.Distance(agentXZ, destXZ);

        // Vertical distance on Y axis
        float verticalDist = Mathf.Abs(agentPos.y - destPos.y);

        // Take max values of provided distances and npc's near distances
        horizontalDistance = Mathf.Max(horizontalDistance, NearHorizontalDistance);
        verticalDistance = Mathf.Max(verticalDistance, NearVerticalDistance);

        // Check if within stopping distances
        if (horizontalDist < horizontalDistance && verticalDist < verticalDistance)
            return true;
        else
            return false;
    }
    #endregion

    #region HAS ARRIVED METHODS
    public bool HasArrivedAtDestination(bool fixRotation = false, bool fixPosition = false)
    {
        return HasArrivedAt(_destinationPos, fixRotation, fixPosition);
    }

    public bool HasArrivedAt(Spot spot, bool fixRotation = false, bool fixPosition = false)
    {
        return HasArrivedAt(spot.transform.position, fixRotation, fixPosition);
    }

    public bool HasArrivedAt(Vector3 destination, bool fixRotation = false, bool fixPosition = false)
    {
        Vector3 agentPos = _agent.transform.position;

        // Horizontal distance on XZ plane
        Vector2 agentXZ = new(agentPos.x, agentPos.z);
        Vector2 destXZ = new(destination.x, destination.z);
        float horizontalDist = Vector2.Distance(agentXZ, destXZ);

        // Vertical distance on Y axis
        float verticalDist = Mathf.Abs(agentPos.y - destination.y);

        // Check if within stopping distances
        if (horizontalDist < ArrivedHorizontalDistance && verticalDist < ArrivedVerticalDistance)
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
    #endregion

    #region FORCE ROTATION METHODS
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
    #endregion

    #region PLACEMENT METHODS
    public void PlaceAtSpot(Spot spot)
    {
        if (spot == null)
        {
            Debug.LogError("PlaceAt(): destinationSpot is null.");
            return;
        }

        PlaceAt(spot.transform.position);

        // Set spot as occupied
        spot.SetOccupied(true);
    }

    public void PlaceAt(Vector3 position)
    {
        Vector3 placementPos;
        bool sampled = false;

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            placementPos = hit.position;
            sampled = true;
        }
        else
        {
            placementPos = position;
        }

        // Place transform first
        _npc.GO.transform.position = placementPos;

        // Now enable the agent and warp it to the sampled NavMesh position if available
        try
        {
            _npc.Agent.enabled = true;
            if (sampled)
            {
                try { _npc.Agent.Warp(placementPos); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
        catch (Exception e) { Debug.LogException(e); }

        // Check if it's in a destination spot and set occupancy
        if (_destinationSpot != null && Vector3.Distance(_destinationSpot.transform.position, placementPos) <= 0.1f)
            _destinationSpot.SetOccupied(true);
    }


    public void PlaceAtDestination()
    {
        PlaceAt(_destinationPos);
    }
    #endregion

    #region OTHER METHODS
    public bool CanReachPosition(Vector3 targetPos, out Vector3 reachablePos)
    {
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hitLocation, _npc.MaxSamplingDistance, NavMesh.AllAreas))
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

    public bool IsPathPending()
    {
        return _agent.pathPending;
    }

    public Vector3 GoToMiddlePoint(INPC otherNPC)
    {
        // Compute midpoint and safe target positions offset so NPCs don't overlap
        Vector3 posA = _npc.GO.transform.position;
        Vector3 posB = otherNPC.GO.transform.position;
        Vector3 midPoint = (posA + posB) * 0.5f;

        Vector3 direction = posB - posA;
        if (direction.sqrMagnitude < 0.0001f)
            direction = _npc.GO.transform.forward;
        direction.Normalize();

        // Determine a comfortable separation using avoidance radii
        float separation = Mathf.Max(0.5f, _npc.AvoidanceRadius + otherNPC.AvoidanceRadius);
        float half = separation * 0.5f;

        Vector3 correctedMidPoint = midPoint - direction * half;

        // Prefer a reachable point on the NavMesh. Try corrected midpoint first,
        // then raw midpoint, then sample along the segment between the two NPCs.
        Vector3 destination = correctedMidPoint;

        if (CanReachPosition(correctedMidPoint, out Vector3 reachablePos))
        {
            destination = reachablePos;
        }
        else if (CanReachPosition(midPoint, out reachablePos))
        {
            destination = reachablePos;
        }
        else
        {
            bool found = false;
            // Sample several points along the line between the two NPCs to find a reachable spot
            for (int i = 1; i <= 9; i++)
            {
                float t = i / 10f;
                Vector3 sample = Vector3.Lerp(posA, posB, t);
                if (CanReachPosition(sample, out reachablePos))
                {
                    destination = reachablePos;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // As a last resort, remain in place (avoid commanding unreachable destination)
                destination = _agent != null ? _agent.transform.position : correctedMidPoint;
            }
        }

        // Command movement to the chosen reachable destination
        SetDestination(destination);

        return destination;
    }
    #endregion
}
