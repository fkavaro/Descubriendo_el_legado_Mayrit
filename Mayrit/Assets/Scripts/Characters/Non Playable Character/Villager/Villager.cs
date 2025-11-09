using System;
using UnityEngine;
using UnityEngine.AI;

public class Villager : ANPC<BehaviourTree>
{
    #region EDIROR PROPERTIES
    [Header("Villager Properties")]
    public House _home;
    public Building _workplace;
    #endregion

    #region PROPERTIES
    BehaviourTree _villagerBT;
    #endregion

    #region INHERITED
    public override BehaviourTree InitializeBehaviourSystem()
    {
        // Interact strategies
        ConditionStrategy isInStreet = new(this, IsInStreet);
        ConditionStrategy isOtherNearby = new(this, IsOtherNearby);
        ConditionStrategy isEnoughSinceLastInteraction = new(this, IsEnoughTimeSinceLastInteraction);
        InteractStrategy interactStrategy = new(this);

        // Routine strategies
        DeactivateModelStrategy deactivateModelStrategy = new(this, GO.transform.GetChild(0).gameObject);

        Spot mosqueEntrance = TownManager.Instance.GetMosqueEntranceSpot();
        GoToDestinationStrategy goToMosque = new(this, mosqueEntrance);
        //Praying_VillagerStrategy prayingStrategy = new(this);

        Spot workplaceEntrance = _workplace.GetRandomEntranceSpot();
        GoToDestinationStrategy goToWork = new(this, workplaceEntrance);
        //Working_VillagerStrategy workingStrategy = new(this);

        Spot marketEntrance = TownManager.Instance.GetMarketSpot();
        GoToDestinationStrategy goToShop = new(this, marketEntrance);
        //Shopping_VillagerStrategy shoppingStrategy = new(this);

        Spot homeEntrance = _home.GetRandomEntranceSpot();
        GoToDestinationStrategy goHome = new(this, homeEntrance);
        AtHome_VillagerStrategy atHomeStrategy = new(this, this);

        // Interact sequence
        SequenceNode interactSequence = new(this);

        LeafNode isInStreetLeaf = new(this, "IsInStreet", isInStreet);
        LeafNode isOtherNearbyLeaf = new(this, "IsOtherNearby", isOtherNearby);
        LeafNode isEnoughSinceLastInteractionLeaf = new(this, "IsEnoughSinceLastInteraction", isEnoughSinceLastInteraction);
        LeafNode talkLeaf = new(this, "Talking", interactStrategy);

        interactSequence.AddChild(isInStreetLeaf);
        interactSequence.AddChild(isOtherNearbyLeaf);
        interactSequence.AddChild(isEnoughSinceLastInteractionLeaf);
        interactSequence.AddChild(talkLeaf);

        // Routine sequence
        SequenceNode routineSequence = new(this);
        SequenceNode prayingSequence = new(this);
        SequenceNode workingSequence = new(this);
        SequenceNode shoppingSequence = new(this);
        SequenceNode atHomeSequence = new(this);

        LeafNode goToMosqueLeaf = new(this, "GoingToMosque", goToMosque);
        LeafNode prayLeaf = new(this, "Praying", deactivateModelStrategy);
        prayingSequence.AddChild(goToMosqueLeaf);
        prayingSequence.AddChild(prayLeaf);
        LeafNode goToWorkLeaf = new(this, "GoingToWork", goToWork);
        LeafNode workLeaf = new(this, "Working", deactivateModelStrategy);
        workingSequence.AddChild(goToWorkLeaf);
        workingSequence.AddChild(workLeaf);
        LeafNode goToShopLeaf = new(this, "GoingToShop", goToShop);
        LeafNode shopLeaf = new(this, "Shopping", deactivateModelStrategy);
        shoppingSequence.AddChild(goToShopLeaf);
        shoppingSequence.AddChild(shopLeaf);
        LeafNode goHomeLeaf = new(this, "GoingHome", goHome);
        LeafNode restLeaf = new(this, "Resting", atHomeStrategy);
        atHomeSequence.AddChild(goHomeLeaf);
        atHomeSequence.AddChild(restLeaf);

        routineSequence.AddChild(prayingSequence);
        routineSequence.AddChild(workingSequence);
        routineSequence.AddChild(shoppingSequence);
        routineSequence.AddChild(atHomeSequence);

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
        _home = home;

        // Add to its residents
        home.AssignNewResident(this);
    }

    public void AssignWorkplace(Building workPlace)
    {
        _workplace = workPlace;
    }

    public void OnReleasedFromPool()
    {
        gameObject.SetActive(false);
        Agent.enabled = false;

        _home.RemoveResident(this);
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
