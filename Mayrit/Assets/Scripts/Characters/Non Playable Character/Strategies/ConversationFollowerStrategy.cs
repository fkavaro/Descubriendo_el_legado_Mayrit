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
            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
            return Node.Status.Failure;

        // Subscribe to conversation end event
        _otherNPC.ConversationEndedEvent += OnConversationEnded;
        _otherFinishedTalking = false;

        _npc.Talk();

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Success if other finished talking
        if (_otherFinishedTalking)
        {
            _otherNPC.ConversationEndedEvent -= OnConversationEnded;
            _npc.ConversationSucceeded();
            return Node.Status.Success;
        }

        // Failure if other NPC is no longer in conversation
        if (!IsOtherStillInConversation())
        {
            _otherNPC.ConversationEndedEvent -= OnConversationEnded;
            return Node.Status.Failure;
        }

        // Keep facing other NPC (XZ plane only)
        Vector3 targetPosition = _otherNPC.GO.transform.position;
        targetPosition.y = _npc.GO.transform.position.y;
        _npc.GO.transform.LookAt(targetPosition);

        _npc.ConversationDuration = _otherNPC.ConversationDuration;
        return Node.Status.Running;
    }

    bool IsOtherStillInConversation()
    {
        if (_otherNPC.IsStillTalkingWith(_npc))
            return true;

        _npc.ConversationInterrupted();
        return false;
    }

    void OnConversationEnded()
    {
        _otherFinishedTalking = true;
    }
}
