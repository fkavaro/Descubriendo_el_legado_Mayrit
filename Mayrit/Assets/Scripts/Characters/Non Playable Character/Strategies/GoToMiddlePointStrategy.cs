using System;
using UnityEngine;

public class GoToMiddlePointStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    private const float MIDPOINT_RECALC_DISTANCE = 1f; // Recalculate if NPCs drift apart
    private const float UPDATE_INTERVAL = 0.5f; // Check every 0.5 seconds
    private const float MAX_MIDPOINT_DISTANCE_FACTOR = 1.5f; // Fail if midpoint ends too far from partner
    private const float MIN_SEPARATION = 0.5f; // Minimum separation distance
    private const float SEPARATION_BUFFER = 0.2f; // Extra buffer distance

    INPC _otherNPC;
    Vector3 _lastMiddlePoint;
    float _timeSinceLastUpdate;
    bool _isMoving;

    public GoToMiddlePointStrategy(NPCtype npc)
    : base(npc) { }

    public override Node.Status Start()
    {
        _otherNPC = _npc.CurrentConversationTarget;

        if (_otherNPC == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] trying to talk to null NPC", _npc.GO);
            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        _lastMiddlePoint = _npc.MovementController.GoToMiddlePoint(_otherNPC);

        if (_lastMiddlePoint == Vector3.zero || IsMiddlePointTooFar(_lastMiddlePoint))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] could not calculate a valid middle point to {_otherNPC.Name}", _npc.GO);

            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        _isMoving = true;
        _timeSinceLastUpdate = 0f;
        _npc.HasArrivedToMiddlePoint = false;

        if (_npc.DebugMode)
            Debug.Log($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] moving to talk to {_otherNPC.Name} as {_npc.ConversationRole}", _npc.GO);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        // Update timer for periodic recalculation
        _timeSinceLastUpdate += Time.deltaTime;

        // Check if arrived at destination
        bool hasArrived = _npc.MovementController.HasArrivedAtDestination();

        if (hasArrived)
        {
            // Handle arrival - set idle animation and stop movement once
            if (_isMoving)
            {
                _npc.AnimationController.ChangeToIdle();
                _npc.MovementController.SetIfStopped(true);
                _isMoving = false;
            }

            _npc.HasArrivedToMiddlePoint = true;
        }
        else
        {
            // Handle movement - set walk animation once
            if (!_isMoving)
            {
                _npc.AnimationController.ChangeToWalk();
                _isMoving = true;
            }

            // Periodically recalculate middle point if NPCs have drifted apart
            // Only recalculate while moving (not when both are ready)
            if (_timeSinceLastUpdate >= UPDATE_INTERVAL)
            {
                Vector3 newMiddlePoint = _npc.MovementController.GoToMiddlePoint(_otherNPC);

                if (IsMiddlePointTooFar(newMiddlePoint))
                {
                    if (_npc.DebugMode)
                        Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Update()] midpoint too far from {_otherNPC.Name}; aborting conversation.", _npc.GO);

                    return Node.Status.Failure;
                }

                // If middle point changed significantly, update destination
                if (Vector3.Distance(_lastMiddlePoint, newMiddlePoint) > MIDPOINT_RECALC_DISTANCE)
                {
                    _lastMiddlePoint = newMiddlePoint;
                    if (_npc.DebugMode)
                        Debug.Log($"[{_npc.Name}.GoToMiddlePointStrategy.Update()] recalculating middle point, distance drifted.", _npc.GO);
                }

                _timeSinceLastUpdate = 0f;
            }
        }

        // Success if both are ready to talk
        if (_npc.HasArrivedToMiddlePoint && _otherNPC.HasArrivedToMiddlePoint)
            return Node.Status.Success;

        return Node.Status.Running;
    }

    bool IsOtherStillInConversation()
    {
        if (_otherNPC.IsStillInConversationWith(_npc))
            return true;

        _npc.ConversationInterrupted();
        return false;
    }

    bool IsMiddlePointTooFar(Vector3 candidate)
    {
        float desiredSeparation = Mathf.Max(
            MIN_SEPARATION,
            _npc.AvoidanceRadius + _otherNPC.AvoidanceRadius + _npc.StoppingDistance + _otherNPC.StoppingDistance + SEPARATION_BUFFER);

        float distanceToOther = Vector3.Distance(candidate, _otherNPC.GO.transform.position);

        if (distanceToOther > desiredSeparation * MAX_MIDPOINT_DISTANCE_FACTOR)
        {
            _npc.ConversationInterrupted();
            return true;
        }

        return false;
    }
}
