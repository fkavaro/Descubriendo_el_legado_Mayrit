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

        // Interaction sequence
        ConditionStrategy isInStreetStrategy = new(this, () => IsInStreet);
        ConditionStrategy IsNotAlreadyTalkingStrategy = new(this, () => IsInteracting == false);
        ConditionStrategy isOtherNearbyStrategy = new(this, IsOtherNearby);
        InteractionStrategy interactionStrategy = new(this);

        SequenceNode interactionSequence = new(this);
        LeafNode isInStreetLeaf = new(this, "IsInStreet?", isInStreetStrategy);
        LeafNode noOneIsTalkingLeaf = new(this, "IsNotAlreadyTalking?", IsNotAlreadyTalkingStrategy);
        LeafNode isOtherNearbyLeaf = new(this, "IsOtherNearby?", isOtherNearbyStrategy);
        LeafNode interactLeaf = new(this, "Talking", interactionStrategy);

        CooldownDecorator talkCooldown = new(this, _interactionCooldown);
        talkCooldown.AddChild(interactLeaf);

        interactionSequence.AddChild(isInStreetLeaf);
        interactionSequence.AddChild(noOneIsTalkingLeaf);
        interactionSequence.AddChild(isOtherNearbyLeaf);
        interactionSequence.AddChild(talkCooldown);

        // Routine sequence
        SequenceNode routineSequence = new(this);

        if (sanctuaryEntrance != null)
        {
            GoToDestinationStrategy goToSanctuaryStrategy = new(this, sanctuaryEntrance);
            InInteriorStrategy prayingStrategy = new(this);

            SequenceNode prayingSequence = new(this);
            LeafNode goToSanctuaryLeaf = new(this, "GoingToSanctuary", goToSanctuaryStrategy);
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
            LeafNode goToWorkLeaf = new(this, "GoingToWork", goToWorkStrategy);
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
            LeafNode goToMarketStallLeaf = new(this, "GoingToMarket", goToMarketStrategy);
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
            LeafNode goHomeLeaf = new(this, "GoingHome", goToHomeStrategy);
            LeafNode restLeaf = new(this, "Resting", atHomeStrategy);
            atHomeSequence.AddChild(goHomeLeaf);
            atHomeSequence.AddChild(restLeaf);

            routineSequence.AddChild(atHomeSequence);
        }

        // Behaviour sequence
        SelectorNode behaviourSelector = new(this);
        behaviourSelector.AddChild(interactionSequence); // First: higher priority
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

    #region PRIVATE METHODS
    bool IsOtherNearby()
    {
        try
        {
            var pool = NPCPoolManager.Instance;
            if (pool == null) return false;

            // Get a villager in the interaction range from this position
            Villager other = pool.GetAnyNearbyVillager(transform.position, _interactionRange, this);
            CurrentInteractionTarget = other; // May be null

            return other != null;
        }
        catch
        {
            CurrentInteractionTarget = null;
            return false;
        }
    }
    #endregion
}
