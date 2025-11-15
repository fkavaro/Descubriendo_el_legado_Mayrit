using UnityEngine;

public class InInteriorStrategy : ATimerStrategy
{
    readonly GameObject _model;

    public InInteriorStrategy(INPC npc, GameObject model, float min = 30, float max = 120)
    : base(npc, min, max)
    {
        _model = model;
    }

    public override Node.Status Start()
    {
        // Deactivated but not returned to pool
        _model.SetActive(false);
        _npc.Agent.enabled = false;

        if (_model.activeSelf == false && !_npc.Agent.enabled)
            return Node.Status.Success;
        else
            return Node.Status.Failure;
    }

    public override void OnTimerComplete()
    {
        // Reactivate NPC
        _model.SetActive(true);
        _npc.Agent.enabled = true;
    }
}
