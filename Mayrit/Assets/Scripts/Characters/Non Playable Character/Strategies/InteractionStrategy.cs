using UnityEngine;

public class InteractionStrategy : ATimedStrategy
{
    INPC _otherNPC;

    public InteractionStrategy(INPC npc, int min = 30, int max = 60)
    : base(npc, min, max) { }

    public override Node.Status Start()
    {
        _otherNPC = (INPC)_npc.CurrentInteractionTarget;

        // Null or handshake is refused
        if (_otherNPC == null || !_otherNPC.TryAcceptInteraction(_npc))
            return Node.Status.Failure;
        // Handshake accepted
        else
            return Node.Status.Success;
    }

    public override void OnTimerComplete()
    {
        // End interaction on both participants
        _npc.EndInteraction();
        _otherNPC.EndInteraction();
        _otherNPC = null;

        Debug.Log($"{_npc.Name} has ended the interaction.");
    }
}