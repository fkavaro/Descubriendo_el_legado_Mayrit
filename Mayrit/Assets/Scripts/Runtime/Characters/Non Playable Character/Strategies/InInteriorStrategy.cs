using System;
using UnityEngine;

public class InInteriorStrategy<NPCtype> : ATimedNPCStrategy<NPCtype>
where NPCtype : INPC
{
    readonly Spot _interiorSpot;

    public InInteriorStrategy(NPCtype npc, Spot interiorSpot, float min = 30, float max = 120)
    : base(npc, min, max)
    {
        _interiorSpot = interiorSpot;
    }

    public override Node.Status Start()
    {
        CleanupStaleConversation();

        if (_interiorSpot == null)
            Debug.LogWarning($"[{_npc.Name}.InInteriorStrategy.Start()] interior spot is null", _npc.GO);
        else
            _npc.MovementController.PlaceAtSpot(_interiorSpot, true);

        _npc.SetCharacterAndAgentActive(false);

        if (_npc.CharacterModel.activeSelf == false && !_npc.Agent.enabled)
            return Node.Status.Success;
        else
        {
            Debug.LogWarning($"[{_npc.Name}.InInteriorStrategy.Start()] failed to go to interior", _npc.GO);
            return Node.Status.Failure;
        }
    }

    public override void OnTimerComplete()
    {
        // Reactivate model and agent
        _npc.SetCharacterAndAgentActive(true);
    }
}
