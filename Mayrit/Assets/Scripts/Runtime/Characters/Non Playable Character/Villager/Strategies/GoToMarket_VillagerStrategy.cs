using UnityEngine;

public class GoToMarket_VillagerStrategy : ANPCStrategy<Villager>
{
    Spot _marketStallSpot;

    public GoToMarket_VillagerStrategy(Villager npc)
    : base(npc) { }

    public override void Reset()
    {
        if (_npc.MarketStall != null)
            _npc.MarketStall.UnregisterClientWaiting(_npc);
        _npc.IsWaitingForAccess = false;
        _marketStallSpot = null;
    }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (!_npc.MovementController.TrySetDestinationStall(out _marketStallSpot, onlyOpen: false))
            return Node.Status.Failure;

        if (_npc.DebugMode)
            Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Start()] going to market.", _npc.GO);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        if (!ValidateMarketState())
            return Node.Status.Failure;

        if (!TryEnsureDestination(_marketStallSpot))
            return Node.Status.Failure;

        if (ShouldSwitchStallWhenNearStall())
        {
            if (_npc.MovementController.TrySetDestinationStall(out _marketStallSpot, onlyOpen: true))
                return Node.Status.Running;
            else
                return Node.Status.Failure;
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

    bool ShouldSwitchStallWhenNearStall()
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
            if (!_npc.AnimationController.IsWalking)
                ResumeMovementToSpot();

            _npc.MovementController.CheckAndHandlePlayerProximity();
            return Node.Status.Running;
        }

        // Close to stall but spot is occupied
        if (_marketStallSpot.IsOccupied())
        {
            WaitForFreeSpot();
            return Node.Status.Running;
        }

        // Close to stall and spot is free - resume movement
        ResumeMovementToSpot();
        return Node.Status.Running;
    }

    void WaitForFreeSpot()
    {
        if (!_npc.IsWaitingForAccess)
        {
            // if (_npc.DebugMode)
            //     Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] stall spot occupied, waiting", _npc.GO);

            _npc.MovementController.IsAgentStopped = true;
            _npc.AnimationController.ChangeToIdle();
            _npc.IsWaitingForAccess = true;
        }

        _npc.MovementController.RotateSmoothlyTowards(_npc.MarketStall.gameObject);
    }

    void ResumeMovementToSpot()
    {
        _npc.IsWaitingForAccess = false;
        _npc.MovementController.IsAgentStopped = false;
        _npc.AnimationController.ChangeToWalk();
    }
}
