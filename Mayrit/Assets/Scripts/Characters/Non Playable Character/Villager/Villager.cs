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
        Spot homeEntrance = null;
        if (_home != null)
            homeEntrance = _home.GetRandomEntranceSpot();

        Spot sanctuaryEntrance = null;
        if (_sanctuary != null)
            sanctuaryEntrance = _sanctuary.GetRandomEntranceSpot();

        Spot workplaceEntrance = null;
        if (_workplace != null)
            workplaceEntrance = _workplace.GetRandomWorkingSpot();

        Spot marketEntrance = null;
        if (TownManager.ExistingInstance != null)
            marketEntrance = TownManager.ExistingInstance.GetMarketSpot();

        // Interact strategies
        ConditionStrategy isInStreet = new(this, IsInStreet);
        ConditionStrategy isOtherNearby = new(this, IsOtherNearby);
        ConditionStrategy isEnoughSinceLastInteraction = new(this, IsEnoughTimeSinceLastInteraction);
        InteractStrategy interactStrategy = new(this);

        // Routine strategies
        DeactivateModelStrategy deactivateModelStrategy = new(this, _model);

        GoToDestinationStrategy goToSanctuaryStrategy = sanctuaryEntrance != null ?
            new(this, sanctuaryEntrance) :
            null;
        GoToDestinationStrategy goToWorkStrategy = workplaceEntrance != null ?
            new(this, workplaceEntrance) :
            null;
        GoToDestinationStrategy goToMarketStrategy = marketEntrance != null ?
            new(this, marketEntrance) :
            null;
        GoToDestinationStrategy goHomeStrategy = homeEntrance != null ?
            new(this, homeEntrance) :
            null;
        AtHome_VillagerStrategy atHomeStrategy = new(this, this);

        // Interact sequence
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

        if (goToSanctuaryStrategy != null)
        {
            SequenceNode prayingSequence = new(this);
            LeafNode goToSanctuaryLeaf = new(this, "GoingToSanctuary", goToSanctuaryStrategy);
            LeafNode prayLeaf = new(this, "Praying", deactivateModelStrategy);
            prayingSequence.AddChild(goToSanctuaryLeaf);
            prayingSequence.AddChild(prayLeaf);
            routineSequence.AddChild(prayingSequence);
        }
        if (goToWorkStrategy != null)
        {
            SequenceNode workingSequence = new(this);
            LeafNode goToWorkLeaf = new(this, "GoingToWork", goToWorkStrategy);
            LeafNode workLeaf = new(this, "Working", deactivateModelStrategy);
            workingSequence.AddChild(goToWorkLeaf);
            workingSequence.AddChild(workLeaf);
            routineSequence.AddChild(workingSequence);
        }
        if (goToMarketStrategy != null)
        {
            SequenceNode shoppingSequence = new(this);
            Shopping_VillagerStrategy shoppingStrategy = new(this);
            LeafNode goToShopLeaf = new(this, "GoingToShop", goToMarketStrategy);
            LeafNode shopLeaf = new(this, "Shopping", shoppingStrategy);
            shoppingSequence.AddChild(goToShopLeaf);
            shoppingSequence.AddChild(shopLeaf);
            routineSequence.AddChild(shoppingSequence);
        }
        if (goHomeStrategy != null)
        {
            SequenceNode atHomeSequence = new(this);
            LeafNode goHomeLeaf = new(this, "GoingHome", goHomeStrategy);
            LeafNode restLeaf = new(this, "Resting", atHomeStrategy);
            atHomeSequence.AddChild(goHomeLeaf);
            atHomeSequence.AddChild(restLeaf);
            routineSequence.AddChild(atHomeSequence);
        }

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
