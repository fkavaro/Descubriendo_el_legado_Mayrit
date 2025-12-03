using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    #region EDIROR PROPERTIES
    [Header("Villager")]
    [SerializeField] protected House _home;
    [SerializeField] protected Workplace _workplace;
    [SerializeField] protected Sanctuary _sanctuary;
    [SerializeField] protected Market _market;
    #endregion

    #region INTERNAL PROPERTIES
    BehaviourTree _villagerBT;
    #endregion

    #region INHERITED
    public override BehaviourTree InitializeBehaviourSystem()
    {
        // Get entrance spots
        Spot sanctuaryEntrance = null;
        if (_sanctuary != null)
            sanctuaryEntrance = _sanctuary.GetRandomAccessSpot();

        Spot workplaceEntrance = null;
        if (_workplace != null)
            workplaceEntrance = _workplace.GetRandomWorkingSpot();

        Spot homeEntrance = null;
        if (_home != null)
            homeEntrance = _home.GetRandomAccessSpot();

        // Conversation sequence
        ConditionStrategy isInStreetStrategy = new(this, () => IsInStreet);
        ConditionStrategy isFollowingConversationStrategy = new(this, IsFollowingConversation);
        GoToMiddlePointStrategy goToMiddlePointStrategyForFollower = new(this);
        GoToMiddlePointStrategy goToMiddlePointStrategyForInitiator = new(this);
        ConversationFollowerStrategy followConversationStrategy = new(this);
        ConditionStrategy canSomeoneNearbyTalkStrategy = new(this, CanSomeoneNearbyTalk);
        ConversationInitiatorStrategy initiateConversationStrategy = new(this);

        CooldownDecorator conversationCooldown = new(this, _conversationCooldown);
        SequenceNode conversationSequence = new(this);
        SelectorNode roleSelector = new(this);
        SequenceNode followConversationSequence = new(this);
        SequenceNode initiateConversationSequence = new(this);

        LeafNode isInStreetLeaf = new(this, "Is in street?", isInStreetStrategy);
        LeafNode isBeingTalkedToLeaf = new(this, "Is following conversation?", isFollowingConversationStrategy);
        LeafNode goToMiddlePointLeafForFollower = new(this, "Going to middle point", goToMiddlePointStrategyForFollower);
        LeafNode goToMiddlePointLeafForInitiator = new(this, "Going to middle point", goToMiddlePointStrategyForInitiator);
        LeafNode followConversationLeaf = new(this, "Talking", followConversationStrategy);
        LeafNode isOtherNearbyLeaf = new(this, "Can someone nearby talk?", canSomeoneNearbyTalkStrategy);
        LeafNode initiateConversationLeafCheck = new(this, "Talking", initiateConversationStrategy);

        conversationCooldown.AddChild(conversationSequence);
        conversationSequence.AddChild(isInStreetLeaf);
        conversationSequence.AddChild(roleSelector);
        roleSelector.AddChild(followConversationSequence);
        followConversationSequence.AddChild(isBeingTalkedToLeaf);
        followConversationSequence.AddChild(goToMiddlePointLeafForFollower);
        followConversationSequence.AddChild(followConversationLeaf);
        roleSelector.AddChild(initiateConversationSequence);
        initiateConversationSequence.AddChild(isOtherNearbyLeaf);
        initiateConversationSequence.AddChild(goToMiddlePointLeafForInitiator);
        initiateConversationSequence.AddChild(initiateConversationLeafCheck);

        // Routine sequence
        SequenceNode routineSequence = new(this);

        if (sanctuaryEntrance != null)
        {
            GoToDestinationStrategy goToSanctuaryStrategy = new(this, sanctuaryEntrance);
            InInteriorStrategy prayingStrategy = new(this);

            SequenceNode prayingSequence = new(this);
            LeafNode goToSanctuaryLeaf = new(this, "Going to sanctuary", goToSanctuaryStrategy);
            LeafNode prayLeaf = new(this, "Praying", prayingStrategy);
            prayingSequence.AddChild(goToSanctuaryLeaf);
            prayingSequence.AddChild(prayLeaf);

            routineSequence.AddChild(prayingSequence);
        }

        if (workplaceEntrance != null)
        {
            GoToDestinationStrategy goToWorkStrategy = new(this, workplaceEntrance, true);
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

        if (homeEntrance != null)
        {
            GoToDestinationStrategy goToHomeStrategy = new(this, homeEntrance);
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
        behaviourSelector.AddChild(conversationCooldown); // First: higher priority
        behaviourSelector.AddChild(routineSequence);

        InfiniteLoopNode infiniteLoop = new(this, behaviourSelector);
        _villagerBT = new(this, infiniteLoop);

        return _villagerBT;
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

    #region CONDITIONAL STRATEGIES METHODS
    bool IsFollowingConversation()
    {
        return _conversationRole.Equals(INPC.RoleInConversation.Follower);
    }

    bool CanSomeoneNearbyTalk()
    {
        // Return if already talking
        if (!_conversationRole.Equals(INPC.RoleInConversation.None))
        {
            if (DebugMode)
                Debug.LogWarning($"[CanSomeoneNearbyTalk()] {Name} cannot look for someone to talk to because is already in conversation.");

            return false;
        }

        var pool = NPCPoolManager.Instance;
        if (pool == null) return false;

        // Get a villager in the interaction range from this position
        Villager someoneNearby = pool.GetAnyNearbyVillager(transform.position, _interactionRange, this);

        // If no candidate found, return false
        if (someoneNearby == null)
            return false;

        // Conversation is accepted by the target
        if (someoneNearby.CanAcceptConversation(this))
        {
            // Set other as current interaction target
            _currentConversationTarget = someoneNearby;
            _currentConversationTargetGO = someoneNearby.GO;

            // This is the initiator
            _conversationRole = INPC.RoleInConversation.Initiator;

            if (DebugMode)
                Debug.Log($"[CanSomeoneNearbyTalk()] {Name} found {_currentConversationTarget.Name} to talk to.");

            return true;
        }
        // Conversation is denied
        else
            return false;
    }
    #endregion
}
