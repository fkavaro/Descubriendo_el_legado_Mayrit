using System;
using UnityEngine;

public class GoToDestinationStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    readonly Spot _destinationSpot;
    private readonly bool _fixRotation;

    public GoToDestinationStrategy(NPCtype npc, Spot destinationSpot, bool fixRotation = false)
    : base(npc)
    {
        _destinationSpot = destinationSpot;
        _fixRotation = fixRotation;
    }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (_destinationSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] destination spot is null", _npc.GO);
            return Node.Status.Failure;
        }

        if (!_npc.MovementController.TrySetDestinationSpot(_destinationSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] could not set destination", _npc.GO);
            return Node.Status.Failure;
        }

        _npc.SetCharacterAndAgentActive(true);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        if (_npc.MovementController.CheckAndHandlePlayerProximity())
            return Node.Status.Running;

        if (!TryEnsureDestination(_destinationSpot))
            return Node.Status.Failure;

        if (!_npc.AnimationController.IsWalking)
            _npc.AnimationController.ChangeToWalk();

        // Success if arrived at destination
        return _npc.MovementController.HasArrivedAtDestinationSpot(_destinationSpot, _fixRotation)
            ? Node.Status.Success
            : Node.Status.Running;
    }
}