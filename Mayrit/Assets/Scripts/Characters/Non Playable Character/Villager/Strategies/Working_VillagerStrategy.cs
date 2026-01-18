using UnityEngine;

public class Working_VillagerStrategy : ATimedNPCStrategy<Villager>
{
    readonly Workplace _workplace;

    public Working_VillagerStrategy(Villager villager, Workplace workplace, float min = 30, float max = 120)
    : base(villager, min, max)
    {
        _workplace = workplace;
    }

    // Start
    public override Node.Status Start()
    {
        if (_workplace == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.Working_VillagerStrategy.Start()] has null workplace", _npc.GO);
            return Node.Status.Failure;
        }

        if (!_npc.MovementController.IsCloseToAnyAccessOf(_workplace))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.Working_VillagerStrategy.Start()] not in workplace", _npc.GO);
            return Node.Status.Failure;
        }

        _npc.AnimationController.ChangeToIdle();
        _workplace.IsWorkplaceOpen = true;

        return Node.Status.Success;
    }

    public override void OnTimerComplete()
    {
        _workplace.IsWorkplaceOpen = false;
    }
}
