using UnityEngine;

public class Shopping_VillagerStrategy : ATimedNPCStrategy<Villager>
{
    public Shopping_VillagerStrategy(Villager npc, float min = 30, float max = 120)
    : base(npc, min, max)
    { }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (_npc.MarketStall == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[ {_npc.Name}.Shopping_VillagerStrategy.Start()] has no assigned stall to shop from. Ending shopping.", _npc.GO);

            return Node.Status.Failure;
        }

        // if (_npc.DebugMode)
        //     Debug.Log($"[{_npc.Name}.Shopping_VillagerStrategy.Start()] started shopping.", _npc.GO);

        _npc.InteractionController.Talk();
        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // No assigned stall
        if (_npc.MarketStall == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.Shopping_VillagerStrategy.Update()] has no assigned stall to shop from. Ending shopping.", _npc.GO);

            return Node.Status.Failure;
        }

        // Stall is closed
        if (!_npc.MarketStall.IsOpen)
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.Shopping_VillagerStrategy.Update()] found that stall is closed. Ending shopping.", _npc.GO);

            _npc.MarketStall = null;

            return Node.Status.Failure;
        }

        if (!_npc.AnimationController.IsIdling())
            _npc.AnimationController.ChangeToIdle();

        return base.Update();
    }

    public override void OnTimerComplete()
    {
        // Finished shopping, clear shopping stall
        _npc.MarketStall = null;
    }
}
