using UnityEngine;

public class GoToDestinationStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    readonly Spot _destinationSpot;
    private readonly bool _fixRotation, _fixPosition;

    public GoToDestinationStrategy(NPCtype npc, Spot destinationSpot, bool fixRotation = false, bool fixPosition = false)
    : base(npc)
    {
        _destinationSpot = destinationSpot;
        _fixRotation = fixRotation;
        _fixPosition = fixPosition;
    }

    public override Node.Status Start()
    {
        // Set initial destination
        _npc.MovementController.SetDestinationSpot(_destinationSpot);

        if (_npc.MovementController.IsDestination(_destinationSpot))
        {
            if (_npc.CurrentConversationTarget != null || _npc.ConversationRole != INPC.RoleInConversation.None)
            {
                if (_npc.DebugMode)
                    Debug.Log($"[GoToDestinationStrategy.Start()] {_npc.Name} is going to {_destinationSpot.name}. Ending conversation with {_npc.CurrentConversationTarget.Name}.");

                _npc.EndConversation();
            }

            return Node.Status.Success;
        }
        else
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToDestinationStrategy.Start()] {_npc.Name} could not set destination");
            return Node.Status.Failure;
        }
    }

    public override Node.Status Update()
    {
        // Fix destination if needed
        if (!_npc.MovementController.IsDestination(_destinationSpot))
        {
            if (_npc.DebugMode)
                Debug.Log($"[GoToDestinationStrategy.Update()] {_npc.Name} fixing destination");

            _npc.MovementController.SetDestinationSpot(_destinationSpot);
        }

        // Success if arrived at destination
        if (_npc.MovementController.HasArrivedAt(_destinationSpot, _fixRotation, _fixPosition))
        {
            _npc.AnimationController.ChangeToIdle();
            return Node.Status.Success;
        }
        // Continue if not
        else
            return Node.Status.Running;
    }
}