using UnityEngine;

public class AtHome_VillagerStrategy : AStrategy
{
    readonly Villager _villager;
    public AtHome_VillagerStrategy(INPC npc, Villager villager)
    : base(npc)
    {
        _villager = villager;
    }

    public override Node.Status Update()
    {
        NPCPoolManager.Instance.ReturnVillagerToPool(_villager);
        return Node.Status.Success;
    }
}
