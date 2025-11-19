using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    #region EDIROR PROPERTIES
    [Header("Villager Properties")]
    public House _home;
    public Workplace _workplace;
    public Sanctuary _sanctuary;
    public Market _market;
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
        ConditionStrategy isInStreet = new(this, IsInStreet);
        ConditionStrategy isOtherNearby = new(this, IsOtherNearby);
        InteractStrategy interactStrategy = new(this);

        SequenceNode interactionSequence = new(this);
        LeafNode isInStreetLeaf = new(this, "IsInStreet?", isInStreet);
        LeafNode isOtherNearbyLeaf = new(this, "IsOtherNearby?", isOtherNearby);
        LeafNode interactLeaf = new(this, "Talking", interactStrategy);

        CooldownDecorator talkCooldown = new(this, _interactionCooldown);
        talkCooldown.AddChild(interactLeaf);

        interactionSequence.AddChild(isInStreetLeaf);
        interactionSequence.AddChild(isOtherNearbyLeaf);
        interactionSequence.AddChild(talkCooldown);

        // Routine sequence
        SequenceNode routineSequence = new(this);

        if (sanctuaryEntrance != null)
        {
            GoToDestinationStrategy goToSanctuaryStrategy = new(this, sanctuaryEntrance);
            InInteriorStrategy prayingStrategy = new(this, _model);

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
            Working_VillagerStrategy workingStrategy = new(this, 60, 180);

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
    bool IsInStreet()
    {
        Debug.Log($"{name} is checking if it is in the street...");
        return true; //IsPathPending(); // TODO
    }

    bool IsOtherNearby()
    {
        Debug.Log($"{name} is checking for other nearby villagers to interact with...");

        try
        {
            var pool = NPCPoolManager.Instance;
            if (pool == null) return false;

            // Get a villager in the interaction range from this position
            Villager other = pool.GetAnyNearbyVillager(transform.position, _interactionRange, this);
            _interactionTarget = other; // May be null

            return other != null;
        }
        catch
        {
            _interactionTarget = null;
            return false;
        }
    }
    #endregion
}
