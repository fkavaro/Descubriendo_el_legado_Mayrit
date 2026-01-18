using System;
using UnityEngine;

public class GoToDestinationStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    readonly Func<Spot> _destinationResolver; // Lazy resolver to avoid stale cached spots
    Spot _destinationSpot;
    private readonly bool _fixRotation;

    public GoToDestinationStrategy(NPCtype npc, Func<Spot> destinationResolver, bool fixRotation = false)
    : base(npc)
    {
        _destinationResolver = destinationResolver;
        _fixRotation = fixRotation;
    }

    public override Node.Status Start()
    {
        // Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.GoToDestinationStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        _destinationSpot = _destinationResolver?.Invoke();

        if (_destinationSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] destination spot is null", _npc.GO);
            return Node.Status.Failure;
        }

        if (_npc.MovementController.SetDestinationSpot(_destinationSpot))
            return Node.Status.Success;

        if (_npc.DebugMode)
            Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] could not set destination", _npc.GO);
        return Node.Status.Failure;
    }

    public override Node.Status Update()
    {
        // Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.GoToDestinationStrategy.Update()] found stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        if (_destinationSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] destination spot is null", _npc.GO);
            return Node.Status.Failure;
        }

        // Fix destination if needed; fail if it cannot be reachable
        if (!_npc.MovementController.IsDestinationSpot(_destinationSpot))
        {
            if (!_npc.MovementController.SetDestinationSpot(_destinationSpot))
                return Node.Status.Failure;
        }

        // Success if arrived at destination
        return _npc.MovementController.HasArrivedAtSpot(_destinationSpot, _fixRotation)
            ? Node.Status.Success
            : Node.Status.Running;
    }
}