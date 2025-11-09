using UnityEngine;

public class DeactivateModelStrategy : AStrategy
{
    readonly GameObject _model;
    readonly float _duration;
    float _elapsedTime = 0f;

    public DeactivateModelStrategy(INPC npc, GameObject model, float duration = 30)
    : base(npc)
    {
        _duration = duration;
        _elapsedTime = 0f;
        _model = model;
    }

    public override Node.Status Update()
    {
        // Deactivated but not returned to pool
        _model.SetActive(false);
        _npc.Agent.enabled = false;

        // Update elapsed time
        _elapsedTime += Time.deltaTime;

        // Check if praying duration is completed
        if (_elapsedTime >= _duration)
        {
            // Reactivate NPC GameObject
            _model.SetActive(true);
            _npc.Agent.enabled = true;
            _elapsedTime = 0f;

            return Node.Status.Success;
        }
        return Node.Status.Running;
    }
}