using UnityEngine;

public class ConversationFollowerStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    INPC _otherNPC;
    bool _otherFinishedTalking;

    public ConversationFollowerStrategy(NPCtype npc)
    : base(npc) { }

    public override Node.Status Start()
    {
        _otherNPC = _npc.CurrentConversationTarget;

        if (_otherNPC == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.ConversationFollowerStrategy.Start()] is being talked to by null NPC", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        // Subscribe to conversation end event
        _otherNPC.InteractionController.ConversationEndedEvent += OnConversationEnded;
        _otherFinishedTalking = false;

        _npc.InteractionController.Talk();

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Success if other finished talking
        if (_otherFinishedTalking)
        {
            _otherNPC.InteractionController.ConversationEndedEvent -= OnConversationEnded;
            _npc.InteractionController.ConversationSucceeded();
            return Node.Status.Success;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
        {
            _otherNPC.InteractionController.ConversationEndedEvent -= OnConversationEnded;
            return Node.Status.Failure;
        }

        // Keep facing other NPC (XZ plane only)
        if (_otherNPC?.GO != null)
            _npc.MovementController.RotateSmoothlyTowards(_otherNPC.GO);

        _npc.ConversationDuration = _otherNPC.ConversationDuration;

        if (!_npc.AnimationController.IsIdling)
            _npc.AnimationController.ChangeToIdle();

        return Node.Status.Running;
    }

    bool IsOtherStillInConversation()
    {
        if (_otherNPC.InteractionController.IsStillTalkingWith(_npc))
            return true;

        _npc.InteractionController.ConversationInterrupted();
        return false;
    }

    void OnConversationEnded()
    {
        _otherFinishedTalking = true;
    }
}
