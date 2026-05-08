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
        if (_otherNPC == null || !_otherNPC.InteractionController.IsStillTalkingWith(_npc))
        {
            if (_otherNPC == null && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] trying to talk to null NPC", _npc.GO);

            if (!_otherNPC.InteractionController.IsStillTalkingWith(_npc) && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] other NPC {_otherNPC.Name} is no longer in conversation", _npc.GO);

            _npc.InteractionController.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Failure if cannot go to middle point
        if (!_npc.MovementController.GoToMiddlePoint(_otherNPC))
        {
            _npc.InteractionController.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // if (_npc.DebugMode)
        //     Debug.Log($"[{_npc.Name}.GoToMiddlePointStrategy.Start()] moving to talk to {_otherNPC.Name} as {_npc.ConversationRole}", _npc.GO);

        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        if (_npc.MovementController.CheckAndHandlePlayerProximity())
            return Node.Status.Running;

        // Failure if other NPC is no longer in conversation
        if (!_otherNPC.InteractionController.IsStillTalkingWith(_npc))
        {
            if (!_otherNPC.InteractionController.IsStillTalkingWith(_npc) && _npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.GoToMiddlePointStrategy.Update()] other NPC {_otherNPC.Name} is no longer in conversation", _npc.GO);

            _npc.InteractionController.ConversationInterrupted();
            return Node.Status.Failure;
        }

        // Check if close enough to OTHER NPC (avoids avoidance radius collision issues)
        float distanceToOther = Vector3.Distance(_npc.GO.transform.position, _otherNPC.GO.transform.position);
        float closeEnoughDistance = _npc.AvoidanceRadius + _npc.NearDistance;

        if (distanceToOther <= closeEnoughDistance && !_npc.HasArrivedToMiddlePoint)
        {
            _npc.AnimationController.ChangeToIdle();
            _npc.MovementController.IsAgentStopped = true;
            _npc.HasArrivedToMiddlePoint = true;
        }
        else
            _npc.AnimationController.ChangeToWalk();

        // Success if both have arrived at middle point
        if (_npc.HasArrivedToMiddlePoint && _otherNPC.HasArrivedToMiddlePoint)
            return Node.Status.Success;

        return Node.Status.Running;
    }
}
