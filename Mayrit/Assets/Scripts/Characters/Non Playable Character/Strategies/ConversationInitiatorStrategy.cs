using UnityEngine;

public class ConversationInitiatorStrategy<NPCtype> : ATimedNPCStrategy<NPCtype>
where NPCtype : INPC
{
    INPC _otherNPC;

    public ConversationInitiatorStrategy(NPCtype npc, int min = 30, int max = 60)
    : base(npc, min, max) { }

    public override Node.Status Start()
    {
        _otherNPC = _npc.CurrentConversationTarget;

        if (_otherNPC == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.ConversationInitiatorStrategy.Start()] trying to talk to null NPC", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        _npc.InteractionController.Talk();

        if (_npc.DebugMode)
            Debug.Log($"[{_npc.Name}.ConversationInitiatorStrategy.Start()] initiating conversation with {_otherNPC.Name}", _npc.GO);

        return base.Start();
    }

    public override Node.Status Update()
    {
        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        // Keep facing other NPC (XZ plane only)
        _npc.MovementController.RotateSmoothlyTowards(_otherNPC.GO);

        // Continue timing
        _npc.ConversationDuration = _strategyRemainingTime;
        return base.Update();
    }

    bool IsOtherStillInConversation()
    {
        if (_otherNPC.InteractionController.IsStillTalkingWith(_npc))
            return true;

        _npc.InteractionController.ConversationInterrupted();
        return false;
    }

    public override void OnTimerComplete()
    {
        _npc.InteractionController.EndConversationAsInitiator();
    }
}