using System;
using UnityEngine;

public class ExitHome_VillagerStrategy : ANPCStrategy<Villager>
{
    readonly Villager _villager;
    readonly Spot _homeSpot;

    public ExitHome_VillagerStrategy(Villager villager, Spot homeSpot)
    : base(villager)
    {
        _villager = villager;
        _homeSpot = homeSpot;
    }

    public override Node.Status Start()
    {
        _npc.MovementController.PlaceAtSpot(_homeSpot, true);
        _villager.SetCharacterAndAgentActive(true);

        if (_villager.CharacterModel.activeSelf == true && _villager.MovementController.IsNearPosition(_homeSpot.transform.position))
        {
            Debug.Log($"[{_villager.Name}.ExitHome_VillagerStrategy.Start()] successfully exited home", _villager.GO);
            return Node.Status.Success;
        }
        else
        {
            Debug.LogWarning($"[{_villager.Name}.ExitHome_VillagerStrategy.Start()] failed to exit home", _villager.GO);
            return Node.Status.Failure;
        }
    }
}
