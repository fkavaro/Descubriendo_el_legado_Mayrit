using UnityEngine;

public class GoToMarketStrategy : AStrategy
{
    readonly Market _market;
    Spot _marketStallSpot;

    public GoToMarketStrategy(INPC npc, Market market)
    : base(npc)
    {
        _market = market;
    }

    public override Node.Status Start()
    {
        _marketStallSpot = _market.GetRandomStallSpot();

        if (_marketStallSpot == null)
            return Node.Status.Failure;

        _npc.SetDestinationSpot(_marketStallSpot);

        if (_npc.IsDestination(_marketStallSpot))
            return Node.Status.Success;
        else
            return Node.Status.Failure;
    }

    public override Node.Status Update()
    {
        // Success if arrived at market
        if (_npc.HasArrivedAt(_marketStallSpot, true, false))
        {
            _npc.AnimationController.ChangeToIdle();
            return Node.Status.Success;
        }
        // Continue if not
        else
            return Node.Status.Running;
    }
}
