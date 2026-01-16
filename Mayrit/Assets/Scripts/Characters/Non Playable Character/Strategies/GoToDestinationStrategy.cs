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
        // Clean up any stale conversation state
        if (_npc.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.GoToDestinationStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.ConversationInterrupted();
        }

        if (_npc.MovementController.SetDestinationSpot(_destinationSpot))
            return Node.Status.Success;
        else
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToDestinationStrategy.Start()] could not set destination", _npc.GO);
            return Node.Status.Failure;
        }
    }

    public override Node.Status Update()
    {
        // Fix destination if needed
        if (!_npc.MovementController.IsDestinationSpot(_destinationSpot))
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.GoToDestinationStrategy.Update()] fixing destination", _npc.GO);

            _npc.MovementController.SetDestinationSpot(_destinationSpot);
        }

        // Success if arrived at destination
        if (_npc.MovementController.HasArrivedAtSpot(_destinationSpot, _fixRotation))
            return Node.Status.Success;
        // Continue if not
        else
            return Node.Status.Running;
    }
}