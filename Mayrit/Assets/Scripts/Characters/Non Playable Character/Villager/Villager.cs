using System;
using UnityEngine;

public class Villager : ANPC<BehaviourTree>
{
    #region EDIROR PROPERTIES
    [Header("Villager Properties")]
    public GameObject _model;
    public House _home;
    public Workplace _workplace;
    public Sanctuary _sanctuary;
    #endregion

    #region PROPERTIES
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

        Market market = null;
        if (TownManager.ExistingInstance != null)
            market = TownManager.ExistingInstance.GetRandomMarket();

        Spot homeEntrance = null;
        if (_home != null)
            homeEntrance = _home.GetRandomAccessSpot();

        // Interact sequence
        ConditionStrategy isInStreet = new(this, IsInStreet);
        ConditionStrategy isOtherNearby = new(this, IsOtherNearby);
        ConditionStrategy isEnoughSinceLastInteraction = new(this, IsEnoughTimeSinceLastInteraction);
        InteractStrategy interactStrategy = new(this);

        SequenceNode interactSequence = new(this);
        LeafNode isInStreetLeaf = new(this, "IsInStreet?", isInStreet);
        LeafNode isOtherNearbyLeaf = new(this, "IsOtherNearby?", isOtherNearby);
        LeafNode isEnoughSinceLastInteractionLeaf = new(this, "IsEnoughSinceLastInteraction?", isEnoughSinceLastInteraction);
        LeafNode talkLeaf = new(this, "Talking", interactStrategy);
        interactSequence.AddChild(isInStreetLeaf);
        interactSequence.AddChild(isOtherNearbyLeaf);
        interactSequence.AddChild(isEnoughSinceLastInteractionLeaf);
        interactSequence.AddChild(talkLeaf);

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
        else
            Debug.LogWarning("Villager " + name + " doesn't pray.");

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
        else
            Debug.LogWarning("Villager " + name + " doesn't have a job.");

        if (market != null)
        {
            GoToMarket_VillagerStrategy goToMarketStrategy = new(this, market);
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
        else
            Debug.LogWarning("Villager " + name + " doesn't shop.");

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
        else
            Debug.LogWarning("Villager " + name + " doesn't rest.");

        // Behaviour sequence
        SelectorNode behaviourSelector = new(this);
        behaviourSelector.AddChild(interactSequence);
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

    public void OnReleasedFromPool()
    {
        gameObject.SetActive(false);
        Agent.enabled = false;

        if (_home != null)
            _home.RemoveAssigned(this);

        _home = null;
    }
    #endregion

    #region PRIVATE METHODS
    private bool IsEnoughTimeSinceLastInteraction()
    {
        // TODO
        return false;
    }

    private bool IsOtherNearby()
    {
        // TODO
        return false;
    }

    private bool IsInStreet()
    {
        // TODO
        return false;
    }
    #endregion
}
