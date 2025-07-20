using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Utility System for decision making in agents.
/// </summary>
/// <typeparam name="TController"></typeparam>
public class UtilitySystem<TController> : ADecisionSystem<TController>
where TController : ABehaviourController<TController>
{
    /// <summary>
    /// List of actions available for the agent.
    /// </summary>
    List<IAction> _actions = new();
    IAction _currentAction;

    /// <summary>
    /// Dictionary to store the utility of each action.
    /// </summary>
    Dictionary<IAction, float> _actionUtilities = new();

    public UtilitySystem(TController controller) : base(controller) { }

    #region INHERITED METHODS
    protected override void DebugDecision()
    {
        controller.actionText.text = _currentAction.DebugDecision();
    }

    public override void Start()
    {
        CalculateActionsUtilities();
    }

    public override void Update()
    {
        if (!controller.isExecutionPaused)
            _currentAction.UpdateAction();

        // Check if it has finished
        if (_currentAction.IsFinished())
            CalculateActionsUtilities();
    }

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
        if (controller.debugMode)
            Debug.Log(controller.name + " making decision...");

        // Calculate the utility of each available action
        foreach (var action in _actions)
        {
            if (controller.debugMode)
                Debug.Log($"    {controller.name}: {action.Name} has utility of {action.Utility}");

            _actionUtilities.Add(action, action.Utility);
        }


        // Find the action with the highest utility
        IAction bestAction = _actionUtilities.OrderByDescending(pair => pair.Value).FirstOrDefault().Key;

        // If the best action has negative utility, continue with current action
        if (_actionUtilities[bestAction] < 0f || bestAction == null)
        {
            if (controller.debugMode)
                Debug.LogError($"   {controller.name}: best action is null or has negative utility, continuing with current action: {_currentAction.Name}");

            bestAction = _currentAction;
        }

        // // Start the best action if it's different from the current one
        // if (!IsCurrentAction(bestAction))
        // {
        //     _currentAction?.FinishAction();
        //     _currentAction = bestAction; // Update current action
        //     _currentAction.StartAction();
        // }

        _currentAction = bestAction; // Update current action
        _currentAction.StartAction();

        // Debug the decision made
        if (controller.debugMode)
            Debug.Log($"{controller.name} is {_currentAction.Name}");

        DebugDecision();

        // Clear the action utilities for the next decision cycle
        _actionUtilities.Clear();
    }
    #endregion
}
