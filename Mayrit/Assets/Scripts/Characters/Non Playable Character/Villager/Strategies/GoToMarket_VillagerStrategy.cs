using UnityEngine;

public class GoToMarket_VillagerStrategy : ANPCStrategy<Villager>
{
    readonly Market _market;
    Stall _marketStall;
    Spot _marketStallSpot;
    bool _alreadyWaiting = false;

    public GoToMarket_VillagerStrategy(Villager npc, Market market)
    : base(npc)
    {
        _market = market;
    }

    public override Node.Status Start()
    {
        // Clean up any stale conversation state
        if (_npc.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.GoToMarket_VillagerStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.ConversationInterrupted();
        }

        GetStallAndSetDestination();

        if (_marketStallSpot == null)
        {
            if (_npc.DebugMode)
                Debug.Log($"[GoToMarket_VillagerStrategy.Update()] {_npc.Name} could not find an available stall spot in the market.", _npc.GO);

            return Node.Status.Failure;
        }

        if (_npc.MovementController.IsDestinationSpot(_marketStallSpot))
            return Node.Status.Success;
        else
            return Node.Status.Failure;
    }

    public override Node.Status Update()
    {
        if (_marketStallSpot == null)
        {
            if (_npc.DebugMode)
                Debug.Log($"[GoToMarket_VillagerStrategy.Update()] {_npc.Name} could not find an available stall spot in the market.", _npc.GO);

            return Node.Status.Failure;
        }

        // Fix destination if needed
        if (!_npc.MovementController.IsDestinationSpot(_marketStallSpot))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[GoToMarket_VillagerStrategy.Update()] {_npc.Name} fixing destination", _npc.GO);

            _npc.MovementController.SetDestinationSpot(_marketStallSpot);
        }

        // Is close to destination stall spot
        if (_npc.MovementController.IsCloseToSpot(_marketStallSpot, 1f))
        {
            if (!_market.IsOpen())
            {
                if (_npc.DebugMode)
                    Debug.Log($"[GoToMarket_VillagerStrategy.Update()] {_npc.Name} found that no stalls are open in the market.", _npc.GO);

                return Node.Status.Failure;
            }

            // Is open
            if (_marketStall.IsWorkplaceOpen)
            {
                // Is occupied
                if (_marketStallSpot.IsOccupied())
                {
                    if (!_alreadyWaiting)
                    {
                        // Stop and idle
                        if (_npc.DebugMode)
                            Debug.Log($"[GoToMarket_VillagerStrategy.Update()] {_npc.Name} is near market stall spot but it's occupied. Stopping.", _npc.GO);
                        _alreadyWaiting = true;
                    }

                    _npc.MovementController.SetIfStopped(true);
                    _npc.AnimationController.ChangeToIdle();

                    // Look towards stall
                    Vector3 lookAtPosition = _marketStall.transform.position;
                    lookAtPosition.y = _npc.GO.transform.position.y;
                    _npc.GO.transform.LookAt(lookAtPosition);
                }
                // Is not occupied
                else
                {
                    _alreadyWaiting = false;

                    _npc.MovementController.SetIfStopped(false);
                    _npc.AnimationController.ChangeToWalk();

                    // Has arrived
                    if (_npc.MovementController.HasArrivedAtSpot(_marketStallSpot, true))
                    {
                        _npc.MarketStall = _marketStall;

                        return Node.Status.Success;
                    }
                }
            }
            // Is closed
            else
            {
                // Go to another stall
                GetStallAndSetDestination();
            }
        }
        else
        {
            _npc.MovementController.SetIfStopped(false);
        }

        return Node.Status.Running;
    }

    void GetStallAndSetDestination()
    {
        _marketStall = _market.GetRandomStall();
        _marketStallSpot = _marketStall.GetRandomAccessSpot();
        _alreadyWaiting = false;
        _npc.MovementController.SetDestinationSpot(_marketStallSpot);
    }
}
