using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Utility System for decision making in agents.
/// </summary>
public class UtilitySystem : ABehaviourSystem
{
    #region PROPERTIES
    readonly List<IAction> _actions = new();
    IAction _currentAction;
    readonly Dictionary<IAction, float> _actionUtilities = new();
    #endregion

    #region CONSTRUCTOR
    protected UtilitySystem(IBehaviourEntity entity)
    : base(entity) { }
    #endregion

    #region INHERITED METHODS
    /// <summary>
    /// Resets all actions and starts again.
    /// </summary>
    public override void Reset()
    {
        // Reset each action
        foreach (var action in _actions)
            action.Reset();

        // Start again
        Start();
    }

    /// <summary>
    /// Debugs the current action of the utility system.
    /// </summary>
    public override void DebugDecision()
    {
        _behaviourEntity.CurrentAction = _currentAction.DebugAction();
    }
    #endregion

    #region LIFE CYCLE
    public override void Start()
    {
        CalculateActionsUtilities();
    }

    public override void Update()
    {
        if (!_behaviourEntity.IsExecutionPaused)
            _currentAction.UpdateAction();

        // Check if it has finished
        if (_currentAction.IsFinished())
            CalculateActionsUtilities();
    }
    #endregion

    #region PUBLIC METHODS
    public void AddAction(IAction action)
    {
        if (action == null) return; // Ignore null actions
        if (_actions.Contains(action)) return; // Ignore duplicate actions

        _actions.Add(action);
    }

    public bool IsCurrentAction(IAction action)
    {
        if (_currentAction == null) return false; // No current action
        return _currentAction == action; // Check if the current action is the same as the given one
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Calculates the utility of each available action and chooses the greatest as the best.
    /// If it's not already the current executing, it starts the action.
    /// </summary>
    void CalculateActionsUtilities()
    {
        // Calculate the utility of each available action
        foreach (var action in _actions)
            _actionUtilities.Add(action, action.Utility);


        // Find the action with the highest utility
        IAction bestAction = _actionUtilities.OrderByDescending(pair => pair.Value).FirstOrDefault().Key;

        // If the best action has negative utility, continue with current action
        if (_actionUtilities[bestAction] < 0f || bestAction == null)
            bestAction = _currentAction;

        // // Only start the best action if it's different from the current one
        // if (!IsCurrentAction(bestAction))
        // {
        //     _currentAction?.FinishAction();
        //     _currentAction = bestAction; // Update current action
        //     _currentAction.StartAction();
        // }

        _currentAction = bestAction; // Update current action
        _currentAction.StartAction();

        DebugDecision();

        // Clear the action utilities for the next decision cycle
        _actionUtilities.Clear();
    }
    #endregion
}
