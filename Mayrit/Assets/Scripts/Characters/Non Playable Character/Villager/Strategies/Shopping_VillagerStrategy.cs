using UnityEngine;

public class Shopping_VillagerStrategy : ATimedNPCStrategy<Villager>
{
    public Shopping_VillagerStrategy(Villager npc, float min = 30, float max = 120)
    : base(npc, min, max)
    { }

    public override Node.Status Start()
    {
        if (_npc.MarketStall == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[Shopping_VillagerStrategy.Start()] {_npc.Name} has no assigned stall to shop from. Ending shopping.");

            return Node.Status.Failure;
        }

        //Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.Shopping_VillagerStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        _npc.InteractionController.Talk();
        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // No assigned stall
        if (_npc.MarketStall == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[Shopping_VillagerStrategy.Update()] {_npc.Name} has no assigned stall to shop from. Ending shopping.");

            return Node.Status.Failure;
        }

        //Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.Shopping_VillagerStrategy.Update()] found stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        // Stall is closed
        if (!_npc.MarketStall._isOpen)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[Shopping_VillagerStrategy.Update()] {_npc.Name} found that stall {_npc.MarketStall.name} is closed. Ending shopping.");

            _npc.MarketStall = null;

            return Node.Status.Failure;
        }

        return base.Update();
    }

    public override void OnTimerComplete()
    {
        // Finished shopping, clear shopping stall
        _npc.MarketStall = null;
    }
}
