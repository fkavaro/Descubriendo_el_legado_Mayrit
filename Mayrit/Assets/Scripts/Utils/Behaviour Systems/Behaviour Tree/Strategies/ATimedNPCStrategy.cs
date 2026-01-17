using UnityEngine;

public abstract class ATimedNPCStrategy<NPCtype> : ANPCStrategy<NPCtype>
where NPCtype : INPC
{
    readonly float _strategyDuration;
    float _strategyElapsedTime;
    protected float _strategyRemainingTime => _strategyDuration - _strategyElapsedTime;

    public ATimedNPCStrategy(NPCtype npc, float min = 30, float max = 120)
    : base(npc)
    {
        _strategyDuration = Random.Range(min, max);
    }

    public override Node.Status Start()
    {
        _strategyElapsedTime = 0f;
        return Node.Status.Success;
    }

    public override Node.Status Update()
    {
        // Update elapsed time
        _strategyElapsedTime += Time.deltaTime;

        // Check if duration is completed
        if (_strategyElapsedTime >= _strategyDuration)
        {
            OnTimerComplete();
            _strategyElapsedTime = 0f;

            return Node.Status.Success;
        }
        return Node.Status.Running;
    }

    public virtual void OnTimerComplete() { }
}
