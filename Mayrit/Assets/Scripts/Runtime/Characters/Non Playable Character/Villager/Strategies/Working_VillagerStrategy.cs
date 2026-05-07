using System;
using UnityEngine;

public class Working_VillagerStrategy : ATimedNPCStrategy<Villager>
{
    readonly Spot _workplaceSpot;

    bool _waitingForClientSpotClear;

    public Working_VillagerStrategy(Villager villager, Spot workplaceSpot, float min = 30, float max = 120)
    : base(villager, min, max)
    {
        _workplaceSpot = workplaceSpot;
    }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (_workplaceSpot == null)
            Debug.LogWarning($"[{_npc.Name}.Working_VillagerStrategy.Start()] has null workplace", _npc.GO);
        else
            _npc.MovementController.PlaceAtSpot(_workplaceSpot, true);

        if (_npc.Workplace.IsInterior)
            // Deactivate model and agent
            _npc.SetCharacterAndAgentActive(false);
        else
        {
            _npc.AnimationController.ChangeToIdle();
            _npc.MovementController.IsAgentStopped = true;
            _npc.SetCharacterAndAgentActive(true);
        }

        _npc.Workplace.IsOpen = true;
        _waitingForClientSpotClear = false;

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // If waiting for client spot to clear
        if (_waitingForClientSpotClear)
        {
            // Success if client left
            if (TryFinishWorkIfClientSpotFree())
            {
                if (_npc.DebugMode)
                    Debug.Log($"[{_npc.Name}.Working_VillagerStrategy.Update()] client spot cleared, finishing work", _npc.GO);
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }

        // Normal timer-based update
        Node.Status timerResult = base.Update();

        // Prevent early completion if waiting for client spot to clear
        if (timerResult == Node.Status.Success && _waitingForClientSpotClear)
            return Node.Status.Running;

        return timerResult;
    }

    public override void OnTimerComplete()
    {
        // Wait for client to leave if working at an stall
        if (_npc.Workplace is Stall stall && stall.IsThereAnyClient())
        {
            // if (_npc.DebugMode)
            //     Debug.LogWarning($"[{_npc.Name}.OnTimerComplete()] waiting for client to leave before finishing work", _npc.GO);
            _waitingForClientSpotClear = true;
            return;
        }

        FinishWork();
    }

    bool TryFinishWorkIfClientSpotFree()
    {
        if (_npc.Workplace is not Stall stall)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.TryFinishWorkIfClientSpotFree()] workplace is not a stall", _npc.GO);
            return false;
        }


        if (stall.IsThereAnyClient())
            return false;

        FinishWork();
        _waitingForClientSpotClear = false;
        return true;
    }

    void FinishWork()
    {
        if (!_npc.CharacterModel.activeSelf)
            // Reactivate model and agent
            _npc.SetCharacterAndAgentActive(true);

        _npc.Workplace.IsOpen = false;
    }
}
