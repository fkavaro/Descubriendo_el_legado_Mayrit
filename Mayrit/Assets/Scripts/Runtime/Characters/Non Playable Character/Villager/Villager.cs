using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    Spot _sanctuaryEntrance;
    Spot _workplaceEntrance;
    Spot _homeEntrance;

    SequenceNode _routineSequence;
    SequenceNode _enterHomeSequence;
    SuccederNode _shoppingSucceeder;

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
        _routineSequence = new(this);

        // Exit home
        ExitHome_VillagerStrategy exitHomeStrategy = new(this, _homeEntrance);
        LeafNode exitHomeLeaf = new(this, "Exiting home", exitHomeStrategy);
        _routineSequence.AddChild(exitHomeLeaf);

        // Pray at sanctuary
        GoToDestinationStrategy<Villager> goToSanctuaryStrategy = new(this, _sanctuaryEntrance);
        InInteriorStrategy<Villager> prayingStrategy = new(this, _sanctuaryEntrance);
        SequenceNode prayingSequence = new(this, "Praying sequence");
        LeafNode goToSanctuaryLeaf = new(this, "Going to sanctuary", goToSanctuaryStrategy);
        LeafNode prayLeaf = new(this, "Praying", prayingStrategy);
        prayingSequence.AddChild(goToSanctuaryLeaf);
        prayingSequence.AddChild(prayLeaf);
        _routineSequence.AddChild(prayingSequence);

        // Work
        if (_workplaceEntrance != null)
        {
            GoToDestinationStrategy<Villager> goToWorkStrategy = new(this, _workplaceEntrance, true);
            Working_VillagerStrategy workingStrategy = new(this, _workplaceEntrance, 60, 180);

            SequenceNode workingSequence = new(this, "Working sequence");
            LeafNode goToWorkLeaf = new(this, "Going to work", goToWorkStrategy);
            LeafNode workLeaf = new(this, "Working", workingStrategy);
            workingSequence.AddChild(goToWorkLeaf);
            workingSequence.AddChild(workLeaf);

            _routineSequence.AddChild(workingSequence);
        }

        // Shop at market
        if (_market != null)
        {
            GoToMarket_VillagerStrategy goToMarketStrategy = new(this);
            Shopping_VillagerStrategy shoppingStrategy = new(this, 15, 45);

            _shoppingSucceeder = new(this, "Shopping sequence");
            SequenceNode shoppingSequence = new(this);
            LeafNode goToMarketStallLeaf = new(this, "Going to market", goToMarketStrategy);
            LeafNode shopLeaf = new(this, "Shopping", shoppingStrategy);
            shoppingSequence.AddChild(goToMarketStallLeaf);
            shoppingSequence.AddChild(shopLeaf);

            int randomRepetitions = UnityEngine.Random.Range(2, 5);
            RepetitionNode shoppingRepetition = new(this, randomRepetitions, shoppingSequence);

            // So that in case of failure (e.g., market closed), routine continues
            _shoppingSucceeder.AddChild(shoppingRepetition);

            _routineSequence.AddChild(_shoppingSucceeder);
        }

        // Finally, go home
        GoToDestinationStrategy<Villager> goToHomeStrategy = new(this, _homeEntrance, true);
        EnterHome_VillagerStrategy enterHomeStrategy = new(this);
        _enterHomeSequence = new(this, "Going home sequence");
        LeafNode goHomeLeaf = new(this, "Going home", goToHomeStrategy);
        LeafNode enterHomeLeaf = new(this, "Resting", enterHomeStrategy);
        _enterHomeSequence.AddChild(goHomeLeaf);
        _enterHomeSequence.AddChild(enterHomeLeaf);
        _routineSequence.AddChild(_enterHomeSequence);

        // Behaviour sequence
        SelectorNode behaviourSelector = new(this);
        behaviourSelector.AddChild(_conversationCooldownNode); // First: higher priority
        behaviourSelector.AddChild(_routineSequence);

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

    public void SetRandomInitialRoutineNode()
    {
        Node initialNode;
        do
        {
            initialNode = _routineSequence.SetRandomCurrentChild();
        } while (initialNode == _enterHomeSequence); // So that not all villagers start at home, which would look weird

        // If shopping initially, go from home
        if (initialNode == _shoppingSucceeder)
        {
            // Place at home
            MovementController.PlaceAtSpot(_homeEntrance, true);
            SetCharacterAndAgentActive(true);
        }
        // If initial node has more than 1 child, start with the second (action) instead of the first (going to destination)
        else if (initialNode._children.Count > 1)
            initialNode.SetCurrentChild(1); // So that it starts in the action, not in the going to destination part

        if (DebugMode)
            Debug.Log($"[{name}] Random routine start: {initialNode._nodeName}", this);
    }

    public void ReturnToPool()
    {
        Reset();

        NPCPoolManager poolManager = ServiceLocator.Instance.Get<NPCPoolManager>();
        poolManager.ReturnVillagerToPool(this);
    }

    public void Reset()
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

        _behaviourSystem = null;
    }
    #endregion
}
