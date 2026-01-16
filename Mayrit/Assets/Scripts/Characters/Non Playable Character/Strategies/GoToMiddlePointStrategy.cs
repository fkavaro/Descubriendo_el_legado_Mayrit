using System;
using UnityEngine;

public class GoToMiddlePointStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    INPC _otherNPC;

    public GoToMiddlePointStrategy(NPCtype npc)
    : base(npc) { }

    public override Node.Status Start()
    {
        _npc.HasArrivedToMiddlePoint = false;
        _otherNPC = _npc.CurrentConversationTarget;

        // Failure if other NPC is null or no longer in conversation
        if (_otherNPC == null || !_otherNPC.IsStillTalkingWith(_npc))
        {
            if (_otherNPC == null && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] trying to talk to null NPC", _npc.GO);

            if (!_otherNPC.IsStillTalkingWith(_npc) && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] other NPC {_otherNPC.Name} is no longer in conversation", _npc.GO);

            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if cannot go to middle point
        if (!_npc.MovementController.GoToMiddlePoint(_otherNPC))
        {
            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Failure if other NPC is no longer in conversation
        if (!_otherNPC.IsStillTalkingWith(_npc))
        {
            if (!_otherNPC.IsStillTalkingWith(_npc) && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Update()] other NPC {_otherNPC.Name} is no longer in conversation", _npc.GO);

            _npc.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Arrived at middle point (first time)
        if (_npc.MovementController.HasArrivedAtDestination() && !_npc.HasArrivedToMiddlePoint)
        {
            _npc.AnimationController.ChangeToIdle();
            _npc.MovementController.SetIfStopped(true);
            _npc.HasArrivedToMiddlePoint = true;
        }
        else
        {
            _npc.AnimationController.ChangeToWalk();
        }

        // Success if both have arrived at middle point
        if (_npc.HasArrivedToMiddlePoint && _otherNPC.HasArrivedToMiddlePoint)
            return Node.Status.Success;

        return Node.Status.Running;
    }
}
