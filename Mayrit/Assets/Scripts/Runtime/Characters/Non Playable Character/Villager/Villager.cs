using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    Spot _sanctuaryEntrance;
    Spot _workplaceEntrance;
    Spot _homeEntrance;

    #region BEHAVIOUR SYSTEM DEFINITION
    public override BehaviourTree DefineBehaviourSystem()
    {
        // Conversation sequence
        ConditionStrategy notInAccessZoneStrategy = new(() => !InAccessZone);
        ConditionStrategy isFollowingConversationStrategy = new(() => _interactionController.IsFollowingConversation());
        GoToMiddlePointStrategy<Villager> goToMiddlePointStrategyForFollower = new(this);
        GoToMiddlePointStrategy<Villager> goToMiddlePointStrategyForInitiator = new(this);
        ConversationFollowerStrategy<Villager> followConversationStrategy = new(this);
        ConditionStrategy canSomeoneNearbyTalkStrategy = new(() => _interactionController.CanInitiateConversationWithSomeoneNearby<Villager>());
        ConversationInitiatorStrategy<Villager> initiateConversationStrategy = new(this);

        _conversationCooldownNode = new(this, _conversationCooldown);
        SequenceNode conversationSequence = new(this, "Conversation Sequence");
        SelectorNode roleSelector = new(this);
        SequenceNode followConversationSequence = new(this);
        SequenceNode initiateConversationSequence = new(this);

        LeafNode notInAccessZoneLeaf = new(this, "Can talk?", notInAccessZoneStrategy);
        LeafNode isBeingTalkedToLeaf = new(this, "Is following conversation?", isFollowingConversationStrategy);
        LeafNode goToMiddlePointAsFollowerLeaf = new(this, "Going to middle point", goToMiddlePointStrategyForFollower);
        LeafNode goToMiddlePointAsInitiatorLeaf = new(this, "Going to middle point", goToMiddlePointStrategyForInitiator);
        LeafNode followConversationLeaf = new(this, "Talking", followConversationStrategy);
        LeafNode isOtherNearbyLeaf = new(this, "Can someone nearby talk?", canSomeoneNearbyTalkStrategy);
        LeafNode initiateConversationLeaf = new(this, "Talking", initiateConversationStrategy);

        _conversationCooldownNode.AddChild(conversationSequence);
        conversationSequence.AddChild(notInAccessZoneLeaf);
        conversationSequence.AddChild(roleSelector);
        roleSelector.AddChild(followConversationSequence);
        followConversationSequence.AddChild(isBeingTalkedToLeaf);
        followConversationSequence.AddChild(goToMiddlePointAsFollowerLeaf);
        followConversationSequence.AddChild(followConversationLeaf);
        roleSelector.AddChild(initiateConversationSequence);
        initiateConversationSequence.AddChild(isOtherNearbyLeaf);
        initiateConversationSequence.AddChild(goToMiddlePointAsInitiatorLeaf);
        initiateConversationSequence.AddChild(initiateConversationLeaf);

        // Routine sequence
        SequenceNode routineSequence = new(this);

        // Exit home
        ExitHome_VillagerStrategy exitHomeStrategy = new(this, _homeEntrance);
        LeafNode exitHomeLeaf = new(this, "Exiting home", exitHomeStrategy);
        routineSequence.AddChild(exitHomeLeaf);

        // Pray at sanctuary
        GoToDestinationStrategy<Villager> goToSanctuaryStrategy = new(this, _sanctuaryEntrance);
        InInteriorStrategy<Villager> prayingStrategy = new(this, _sanctuaryEntrance);
        SequenceNode prayingSequence = new(this, "Praying sequence");
        LeafNode goToSanctuaryLeaf = new(this, "Going to sanctuary", goToSanctuaryStrategy);
        LeafNode prayLeaf = new(this, "Praying", prayingStrategy);
        prayingSequence.AddChild(goToSanctuaryLeaf);
        prayingSequence.AddChild(prayLeaf);
        routineSequence.AddChild(prayingSequence);

        if (_workplaceEntrance != null)
        {
            GoToDestinationStrategy<Villager> goToWorkStrategy = new(this, _workplaceEntrance, true);
            Working_VillagerStrategy workingStrategy = new(this, _workplaceEntrance, 60, 180);

            SequenceNode workingSequence = new(this, "Working sequence");
            LeafNode goToWorkLeaf = new(this, "Going to work", goToWorkStrategy);
            LeafNode workLeaf = new(this, "Working", workingStrategy);
            workingSequence.AddChild(goToWorkLeaf);
            workingSequence.AddChild(workLeaf);

            routineSequence.AddChild(workingSequence);
        }

        SuccederNode shoppingSucceeder = new(this, "Shopping sequence");
        if (_market != null)
        {
            GoToMarket_VillagerStrategy goToMarketStrategy = new(this);
            Shopping_VillagerStrategy shoppingStrategy = new(this, 15, 45);

            SequenceNode shoppingSequence = new(this);
            LeafNode goToMarketStallLeaf = new(this, "Going to market", goToMarketStrategy);
            LeafNode shopLeaf = new(this, "Shopping", shoppingStrategy);
            shoppingSequence.AddChild(goToMarketStallLeaf);
            shoppingSequence.AddChild(shopLeaf);

            int randomRepetitions = UnityEngine.Random.Range(2, 5);
            RepetitionNode shoppingRepetition = new(this, randomRepetitions, shoppingSequence);

            // So that in case of failure (e.g., market closed), routine continues
            shoppingSucceeder.AddChild(shoppingRepetition);

            routineSequence.AddChild(shoppingSucceeder);
        }

        // Random starting node in routine sequence to add variability among villagers
        Node initialRoutineNode = routineSequence.GetCurrentRandomChild();

        // If shopping initially, go from home
        if (initialRoutineNode == shoppingSucceeder)
        {
            // Place at home
            MovementController.PlaceAtSpot(_homeEntrance, true);
            SetCharacterAndAgentActive(true);
            Debug.Log($"[{name}] Initial node: {initialRoutineNode._nodeName}", this);
        }
        // If initial node has more than 1 child, start with the second (action) instead of the first (going to destination)
        else if (initialRoutineNode._children.Count > 1)
        {
            initialRoutineNode.SetCurrentChild(1); // So that it starts in the action, not in the going to destination part
            Debug.Log($"[{name}] Initial node: {initialRoutineNode._nodeName} (skipping going to destination)", this);
        }

        if (_homeEntrance != null)
        {
            GoToDestinationStrategy<Villager> goToHomeStrategy = new(this, _homeEntrance, true);
            EnterHome_VillagerStrategy enterHomeStrategy = new(this);

            SequenceNode enterHomeSequence = new(this, "Going home sequence");
            LeafNode goHomeLeaf = new(this, "Going home", goToHomeStrategy);
            LeafNode enterHomeLeaf = new(this, "Resting", enterHomeStrategy);
            enterHomeSequence.AddChild(goHomeLeaf);
            enterHomeSequence.AddChild(enterHomeLeaf);

            routineSequence.AddChild(enterHomeSequence);
        }

        // Behaviour sequence
        SelectorNode behaviourSelector = new(this);
        behaviourSelector.AddChild(_conversationCooldownNode); // First: higher priority
        behaviourSelector.AddChild(routineSequence);

        InfiniteLoopNode infiniteLoop = new(this, behaviourSelector);
        BehaviourTree villagerBT = new(this, infiniteLoop);

        return villagerBT;
    }
    #endregion

    #region PUBLIC METHODS
    public void AssignHome(House home)
    {
        if (home == null)
        {
            if (DebugMode)
                Debug.LogWarning("Trying to assign null Home to " + name);
            return;
        }

        _home = home;
        home.AddNewAssigned(this);
        _homeEntrance = _home.GetRandomAccessSpot();

    }

    public void AssignWorkplace(Workplace workPlace)
    {
        if (workPlace == null)
        {
            if (DebugMode)
                Debug.LogWarning("Trying to assign null Workplace to " + name);
            return;
        }

        _workplace = workPlace;
        workPlace.AddNewAssigned(this);
        _workplaceEntrance = _workplace.GetRandomWorkingSpot();
    }

    public void AssignSanctuary(Sanctuary sanctuary)
    {
        if (sanctuary == null)
        {
            if (DebugMode)
                Debug.LogWarning("Trying to assign null Sanctuary to " + name);
            return;
        }

        _sanctuary = sanctuary;
        _sanctuaryEntrance = _sanctuary.GetRandomAccessSpot();
    }

    public void AssignMarket(Market randomMarket)
    {
        if (randomMarket == null)
        {
            if (DebugMode)
                Debug.LogWarning("Trying to assign null Market to " + name);
            return;
        }

        _market = randomMarket;
    }

    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        SetCharacterAndAgentActive(false);

        if (_home != null)
            _home.RemoveAssigned(this);

        if (_workplace != null)
            _workplace.RemoveAssigned(this);

        _home = null;
        _homeEntrance = null;
        _workplace = null;
        _workplaceEntrance = null;
        _sanctuary = null;
        _sanctuaryEntrance = null;
        _market = null;
        _marketStall = null;

        NPCPoolManager poolManager = ServiceLocator.Instance.Get<NPCPoolManager>();
        poolManager.ReturnVillagerToPool(this);
    }
    #endregion
}
