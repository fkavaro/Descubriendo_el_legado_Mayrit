using UnityEngine;

public class GoToMarket_VillagerStrategy : ANPCStrategy<Villager>
{
    Spot _marketStallSpot;

    public GoToMarket_VillagerStrategy(Villager npc)
    : base(npc) { }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (!_npc.MovementController.TrySetDestinationStall(out _marketStallSpot, onlyOpen: false))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Start()] can't go to market.", _npc.GO);
            return Node.Status.Failure;
        }

        // if (_npc.DebugMode)
        //     Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Start()] going to market stall.", _npc.GO);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        if (!ValidateMarketState())
            return Node.Status.Failure;

        if (!TryEnsureDestination(_marketStallSpot))
            return Node.Status.Failure;

        if (ShouldSwitchStallWhenNear())
        {
            // No open stall found: failure
            if (!_npc.MovementController.TrySetDestinationStall(out _marketStallSpot, onlyOpen: true))
            {
                if (_npc.DebugMode)
                    Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] no stalls available. Ending...", _npc.GO);
                return Node.Status.Failure;
            }
            return Node.Status.Running;
        }

        if (HasArrivedAtStall())
        {
            _npc.MarketStall.UnregisterClientWaiting(_npc);
            return Node.Status.Success;
        }

        return HandleApproachingStall();
    }

    bool ValidateMarketState()
    {
        if (_npc.Market == null || _npc.MarketStall == null || _marketStallSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] invalid market state", _npc.GO);
            return false;
        }
        return true;
    }

    bool ShouldSwitchStallWhenNear()
    {
        if (!_npc.MovementController.IsFarFromPosition(_marketStallSpot.transform.position))
            return false;

        return !_npc.MarketStall.IsOpen || _npc.MarketStall.TooManyClientsWaiting;
    }

    bool HasArrivedAtStall()
    {
        return _npc.MovementController.HasArrivedAtDestinationSpot(_marketStallSpot, true);
    }

    Node.Status HandleApproachingStall()
    {
        // Still far from stall
        if (!_npc.MovementController.IsNearDestinationSpot(_marketStallSpot))
        {
            _npc.MovementController.SetIfStopped(false);
            return Node.Status.Running;
        }

        // Close to stall but spot is occupied
        if (_marketStallSpot.IsOccupied())
        {
            HandleWaitingForSpot();
            return Node.Status.Running;
        }

        // Close to stall and spot is free - resume movement
        ResumeMovementToSpot();
        return Node.Status.Running;
    }

    void HandleWaitingForSpot()
    {
        if (!_npc.IsWaitingForAccess)
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] stall spot occupied, waiting", _npc.GO);

            _npc.MovementController.SetIfStopped(true);
            _npc.AnimationController.ChangeToIdle();
            _npc.IsWaitingForAccess = true;
        }

        _npc.MovementController.RotateSmoothlyTowards(_npc.MarketStall.gameObject);
    }

    void ResumeMovementToSpot()
    {
        _npc.IsWaitingForAccess = false;
        _npc.MovementController.SetIfStopped(false);
        _npc.AnimationController.ChangeToWalk();
    }
}
