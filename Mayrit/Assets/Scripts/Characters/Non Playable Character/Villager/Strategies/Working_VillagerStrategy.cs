using UnityEngine;

public class Working_VillagerStrategy : ATimedNPCStrategy<Villager>
{
    readonly Workplace _workplace;

    public Working_VillagerStrategy(Villager villager, Workplace workplace, float min = 30, float max = 120)
    : base(villager, min, max)
    {
        _workplace = workplace;
    }

    public override Node.Status Start()
    {
        //Clean up any stale conversation state
        if (_npc.InteractionController.IsTalking())
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"{_npc.Name}.Working_VillagerStrategy.Start()] starting routine with stale conversation state - cleaning up", _npc.GO);
            _npc.InteractionController.ConversationInterrupted();
        }

        if (_workplace == null)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.Working_VillagerStrategy.Start()] has null workplace", _npc.GO);
            return Node.Status.Failure;
        }

        if (!_npc.MovementController.IsCloseToAnyWorkSpotOf(_workplace))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.Working_VillagerStrategy.Start()] not in workplace", _npc.GO);
            return Node.Status.Failure;
        }

        if (_workplace.IsInterior)
            // Deactivate model and agent
            _npc.SetCharacterAndAgentActive(false);
        else
        {
            _npc.AnimationController.ChangeToIdle();
            _npc.MovementController.SetIfStopped(true);
        }

        _workplace._isOpen = true;

        // if (_npc.DebugMode)
        //     Debug.Log($"{_npc.Name} started working", _npc.GO);

        return Node.Status.Success;
    }

    public override void OnTimerComplete()
    {
        if (!_npc.CharacterModel.activeSelf)
            // Reactivate model and agent
            _npc.SetCharacterAndAgentActive(true);

        _workplace._isOpen = false;
    }
}
