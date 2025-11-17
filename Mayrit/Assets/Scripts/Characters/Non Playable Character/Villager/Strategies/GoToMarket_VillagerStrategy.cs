using UnityEngine;

public class GoToMarket_VillagerStrategy : AStrategy
{
    readonly Market _market;
    Stall _marketStall;
    Spot _marketStallSpot;

    public GoToMarket_VillagerStrategy(INPC npc, Market market)
    : base(npc)
    {
        _market = market;
    }

    public override Node.Status Start()
    {
        GetStallAndSetDestination();

        if (_marketStallSpot == null)
        {
            Debug.LogWarning($"{_npc.Name} could not find an available stall spot in the market.");
            return Node.Status.Failure;
        }

        if (_npc.IsDestination(_marketStallSpot))
            return Node.Status.Success;
        else
            return Node.Status.Failure;
    }

    public override Node.Status Update()
    {
        if (_marketStallSpot == null)
        {
            Debug.LogWarning($"{_npc.Name} could not find an available stall spot in the market.");
            return Node.Status.Failure;
        }

        // Is close to destination stall spot
        if (_npc.IsCloseTo(_marketStallSpot))
        {
            if (!_market.IsOpen())
            {
                Debug.LogWarning($"{_npc.Name} found that no stalls are open in the market.");
                return Node.Status.Failure;
            }

            // Is open
            if (_marketStall.IsOpen())
            {
                // Is occupied
                if (_marketStallSpot.IsOccupied())
                {
                    // Stop and idle
                    Debug.Log($"{_npc.Name} is near market stall spot but it's occupied. Stopping.");
                    _npc.SetIfStopped(true);
                    _npc.AnimationController.ChangeToIdle();
                }
                // Is not occupied
                else
                {
                    _npc.SetIfStopped(false);

                    // Has arrived
                    if (_npc.HasArrivedAt(_marketStallSpot, true, false))
                    {
                        _npc.AnimationController.ChangeToTalk();
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
        // else
        // {
        //     _npc.SetIfStopped(false);
        // }

        return Node.Status.Running;
    }

    void GetStallAndSetDestination()
    {
        _marketStall = _market.GetRandomStall();
        _marketStallSpot = _marketStall.GetRandomAccessSpot();
        _npc.SetDestinationSpot(_marketStallSpot);
    }
}
