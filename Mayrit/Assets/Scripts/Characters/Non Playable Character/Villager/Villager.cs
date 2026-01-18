using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    #region PROPERTIES HELPERS
    public Stall MarketStall
    {
        get => _marketStall;
        set => _marketStall = value;
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Villager")]
    [SerializeField] protected House _home;
    [SerializeField] protected Workplace _workplace;
    [SerializeField] protected Sanctuary _sanctuary;
    [SerializeField] protected Market _market;
    [SerializeField] protected Stall _marketStall;
    #endregion

    #region BEHAVIOUR SYSTEM DEFINITION
    public override BehaviourTree DefineBehaviourSystemOnAwake()
    {
        // Lazy spot getters
        Spot getSanctuaryEntrance() => _sanctuary != null ? _sanctuary.GetRandomAccessSpot() : null;
        Spot getWorkplaceEntrance() => _workplace != null ? _workplace.GetRandomWorkingSpot() : null;
        Spot getHomeEntrance() => _home != null ? _home.GetRandomAccessSpot() : null;

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

        if (getSanctuaryEntrance() != null)
        {
            GoToDestinationStrategy<Villager> goToSanctuaryStrategy = new(this, getSanctuaryEntrance, true);
            InInteriorStrategy<Villager> prayingStrategy = new(this);

            SequenceNode prayingSequence = new(this);
            LeafNode goToSanctuaryLeaf = new(this, "Going to sanctuary", goToSanctuaryStrategy);
            LeafNode prayLeaf = new(this, "Praying", prayingStrategy);
            prayingSequence.AddChild(goToSanctuaryLeaf);
            prayingSequence.AddChild(prayLeaf);

            routineSequence.AddChild(prayingSequence);
        }

        if (getWorkplaceEntrance() != null)
        {
            GoToDestinationStrategy<Villager> goToWorkStrategy = new(this, getWorkplaceEntrance, true);
            Working_VillagerStrategy workingStrategy = new(this, _workplace, 60, 180);

            SequenceNode workingSequence = new(this);
            LeafNode goToWorkLeaf = new(this, "Going to work", goToWorkStrategy);
            LeafNode workLeaf = new(this, "Working", workingStrategy);
            workingSequence.AddChild(goToWorkLeaf);
            workingSequence.AddChild(workLeaf);

            routineSequence.AddChild(workingSequence);
        }

        if (_market != null)
        {
            GoToMarket_VillagerStrategy goToMarketStrategy = new(this, _market);
            Shopping_VillagerStrategy shoppingStrategy = new(this, 15, 45);

            SequenceNode shoppingSequence = new(this);
            LeafNode goToMarketStallLeaf = new(this, "Going to market", goToMarketStrategy);
            LeafNode shopLeaf = new(this, "Shopping", shoppingStrategy);
            shoppingSequence.AddChild(goToMarketStallLeaf);
            shoppingSequence.AddChild(shopLeaf);

            int randomRepetitions = UnityEngine.Random.Range(2, 5);
            RepetitionNode shoppingRepetition = new(this, randomRepetitions, shoppingSequence);

            // So that in case of failure (e.g., market closed), routine continues
            SuccederNode shoppingSucceeder = new(this);
            shoppingSucceeder.AddChild(shoppingRepetition);

            routineSequence.AddChild(shoppingSucceeder);
        }

        if (getHomeEntrance() != null)
        {
            GoToDestinationStrategy<Villager> goToHomeStrategy = new(this, getHomeEntrance, true);
            AtHome_VillagerStrategy atHomeStrategy = new(this);

            SequenceNode atHomeSequence = new(this);
            LeafNode goHomeLeaf = new(this, "Going home", goToHomeStrategy);
            LeafNode restLeaf = new(this, "Resting", atHomeStrategy);
            atHomeSequence.AddChild(goHomeLeaf);
            atHomeSequence.AddChild(restLeaf);

            routineSequence.AddChild(atHomeSequence);
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
        if (home != null)
        {
            _home = home;

            // Add to its residents
            home.AddNewAssigned(this);
        }
        else
        {
            Debug.LogWarning("Trying to assign null Home to " + name);
        }
    }

    public void AssignWorkplace(Workplace workPlace)
    {
        if (workPlace != null)
        {
            _workplace = workPlace;

            // Add to its employees
            workPlace.AddNewAssigned(this);
        }
    }

    public void AssignSanctuary(Sanctuary sanctuary)
    {
        if (sanctuary != null)
            _sanctuary = sanctuary;
    }

    public void AssignMarket(Market randomMarket)
    {
        if (randomMarket != null)
            _market = randomMarket;
    }

    public void OnReleasedFromPool()
    {
        gameObject.SetActive(false);
        Agent.enabled = false;

        if (_home != null)
            _home.RemoveAssigned(this);

        if (_workplace != null)
            _workplace.RemoveAssigned(this);

        _home = null;
        _workplace = null;
        _sanctuary = null;
        _market = null;
    }
    #endregion
}
