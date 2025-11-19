using UnityEngine;

public class InteractStrategy : ATimedStrategy
{
    INPC _otherNPC;

    public InteractStrategy(INPC npc, int min = 30, int max = 60)
    : base(npc, min, max) { }

    public override Node.Status Start()
    {
        _otherNPC = _npc.CurrentInteractionTarget;

        // Null or handshake is refused
        if (_otherNPC == null || !_otherNPC.TryAcceptInteraction(_npc))
            return Node.Status.Failure;

        // Handshake accepted
        Debug.Log($"{_npc.Name} is interacting with {_otherNPC.Name}");

        _npc.StartInteraction();

        return Node.Status.Success;
    }

    public override void OnTimerComplete()
    {
        // End interaction on both participants
        try
        {
            _npc.EndInteraction();
            _otherNPC.EndInteraction();
        }
        catch { }
        finally
        {
            _otherNPC = null;
        }
    }
}

