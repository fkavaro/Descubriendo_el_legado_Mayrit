using UnityEngine;

public class GoToMarket_VillagerStrategy : ANPCStrategy<Villager>
{
    readonly Market _market;
    Spot _marketStallSpot;
    bool _isWaitingForAccess;

    public GoToMarket_VillagerStrategy(Villager npc, Market market)
    : base(npc)
    {
        _market = market;
    }

    public override Node.Status Start()
    {
        // Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        if (!GetOpenStallAndSetDestination())
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] could not go to any open stall in the market.", _npc.GO);

            return Node.Status.Failure;
        }

        if (_npc.DebugMode)
            Debug.Log($"[{_npc.Name}.GoToMarket_VillagerStrategy.Start()] heading to market stall spot", _npc.GO);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Guard: validate market/stall/spot state
        if (_market == null || _npc.MarketStall == null || _marketStallSpot == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] invalid market state", _npc.GO);
            return Node.Status.Failure;
        }

        // Fix destination if needed; fail if unreachable
        if (!_npc.MovementController.IsDestinationSpot(_marketStallSpot))
        {
            if (!_npc.MovementController.SetDestinationSpot(_marketStallSpot))
                return Node.Status.Failure;
        }

        // NOT CLOSE TO STALL SPOT YET
        if (!_npc.MovementController.IsCloseToSpot(_marketStallSpot))
        {
            // Keep moving
            _npc.MovementController.SetIfStopped(false);
            return Node.Status.Running;
        }

        // CLOSE TO STALL SPOT
        // If stall is closed
        if (!_npc.MarketStall._isOpen)
        {
            // Try to get another opened stall; fail if none available
            if (!GetOpenStallAndSetDestination())
            {
                if (_npc.DebugMode)
                    Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] no available stalls found", _npc.GO);
                return Node.Status.Failure;
            }
            return Node.Status.Running;
        }

        // ARRIVED AT STALL SPOT
        if (_npc.MovementController.HasArrivedAtSpot(_marketStallSpot, true))
        {
            _npc.MarketStall.UnregisterClientWaiting(_npc);
            return Node.Status.Success;
        }

        // Try get other stall if too many clients are already waiting
        if (_npc.MarketStall.TooManyClientsWaiting)
        {
            if (!GetOpenStallAndSetDestination())
            {
                if (_npc.DebugMode)
                    Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] no available stalls found when trying to avoid crowd", _npc.GO);
                return Node.Status.Failure;
            }
            return Node.Status.Running;
        }

        // NOT ARRIVED AT STALL SPOT YET, but close
        // If spot is occupied
        if (_marketStallSpot.IsOccupied())
        {
            // Wait for spot to become available
            if (!_isWaitingForAccess)
            {
                if (_npc.DebugMode)
                    Debug.LogWarning($"[{_npc.Name}.GoToMarket_VillagerStrategy.Update()] stall spot occupied, waiting", _npc.GO);

                _npc.MovementController.SetIfStopped(true);
                _npc.AnimationController.ChangeToIdle();
                _npc.MarketStall.RegisterClientWaiting(_npc);
                _isWaitingForAccess = true;
            }

            _npc.MovementController.RotateSmoothlyTowards(_npc.MarketStall.gameObject);
            return Node.Status.Running;
        }

        // CLOSE TO STALL SPOT AND SPOT IS FREE
        // Resume movement
        _isWaitingForAccess = false;
        _npc.MovementController.SetIfStopped(false);
        _npc.AnimationController.ChangeToWalk();

        return Node.Status.Running;
    }

    bool GetOpenStallAndSetDestination()
    {
        _npc.MarketStall = _market.GetRandomOpenedStall();
        if (_npc.MarketStall == null)
            return false;

        _marketStallSpot = _npc.MarketStall.GetRandomAccessSpot();
        if (_marketStallSpot == null)
            return false;

        if (!_npc.MovementController.SetDestinationSpot(_marketStallSpot))
            return false;

        _isWaitingForAccess = false;
        return true;
    }
}
