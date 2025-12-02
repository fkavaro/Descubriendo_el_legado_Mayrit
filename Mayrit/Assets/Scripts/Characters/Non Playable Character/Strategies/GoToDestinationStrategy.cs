using UnityEngine;

public class GoToDestinationStrategy : AStrategy
{
    readonly Spot _destinationSpot;
    private readonly bool _fixRotation, _fixPosition;

    public GoToDestinationStrategy(INPC npc, Spot destinationSpot, bool fixRotation = false, bool fixPosition = false)
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
            _npc.IsInStreet = true;
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
        // Failure if not going to destination anymore
        if (!_npc.MovementController.IsDestination(_destinationSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToDestinationStrategy.Update()] {_npc.Name} fixing destination");
            _npc.MovementController.SetDestinationSpot(_destinationSpot);
        }

        if (_npc.MovementController.IsCloseTo(_destinationSpot))
            _npc.IsInStreet = false;

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