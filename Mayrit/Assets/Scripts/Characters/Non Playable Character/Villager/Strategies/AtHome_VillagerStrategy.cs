using UnityEngine;

public class AtHome_VillagerStrategy : ANPCStrategy<Villager>
{
    readonly Villager _villager;
    readonly NPCPoolManager _npcPoolManager;

    public AtHome_VillagerStrategy(Villager villager)
    : base(villager)
    {
        _villager = villager;

        // Get dependency from Service Locator
        _npcPoolManager = ServiceLocator.Instance.Get<NPCPoolManager>();

        if (_npcPoolManager == null)
            Debug.LogError("AtHome_VillagerStrategy: NPCPoolManager not found in ServiceLocator!");
    }

    public override Node.Status Start()
    {
        _npcPoolManager.ReturnVillagerToPool(_villager);

        if (_villager.gameObject.activeSelf == false)
            return Node.Status.Success;
        else
            return Node.Status.Failure;
    }

    public override Node.Status Update()
    {
        return Node.Status.Success;
    }
}
