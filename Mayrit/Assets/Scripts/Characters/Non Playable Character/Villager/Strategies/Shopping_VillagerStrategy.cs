using UnityEngine;

// TODO: implement with a repetition node in the behavior tree?
public class Shopping_VillagerStrategy : ATimerStrategy
{

    public Shopping_VillagerStrategy(INPC npc, float min = 30, float max = 120)
    : base(npc, min, max)
    { }

    // Start


    // Update


    // /// <summary>
    // /// Number of purchases to make during shopping.
    // /// </summary>
    // readonly int _puchasesNum;
    // int _purchasesMade;

    // Spot _stall;

    // public Shopping_VillagerStrategy(INPC npc, int minItems = 2, int maxItems = 10)
    // : base(npc, minItems, maxItems)
    // {
    //     _puchasesNum = Random.Range(minItems, maxItems);
    //     _purchasesMade = 0;
    //     _stall = null;
    // }

    // public override Node.Status Update()
    // {
    //     if (_purchasesMade < _puchasesNum)
    //     {
    //         if (_stall == null)
    //         {
    //             _stall = TownManager.Instance.GetRandomMarketStallSpot();

    //             _npc.SetDestinationSpot(_stall);

    //             if (_npc.HasArrivedAt(_stall))
    //             {
    //                 _stall = null;
    //                 _purchasesMade++;
    //             }
    //         }

    //         return Node.Status.Running;
    //     }
    //     else
    //     {
    //         // Reset for next shopping trip
    //         _purchasesMade = 0;
    //         return Node.Status.Success;
    //     }
    // }
}
