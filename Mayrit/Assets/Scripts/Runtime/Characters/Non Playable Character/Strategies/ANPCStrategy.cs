using UnityEngine;

/// <summary>
/// AStrategy defines the contract for all strategies used in the behaviour tree nodes.
/// </summary>
public abstract class ANPCStrategy<NPCtype> : IStrategy
where NPCtype : INPC
{
    #region PROPERTIES
    protected readonly NPCtype _npc;
    #endregion

    #region CONSTRUCTOR
    public ANPCStrategy(NPCtype npc)
    {
        _npc = npc;
    }
    #endregion

    #region TO BE IMPLEMENTED METHODS
    public virtual Node.Status Start()
    {
        return Node.Status.Success;
    }
    public virtual Node.Status Update()
    {
        return Node.Status.Success;
    }
    public virtual void Reset() { }
    #endregion

    protected void CleanupStaleConversation()
    {
        if (_npc.InteractionController.IsTalking())
            _npc.InteractionController.ConversationInterrupted();
    }

    protected bool TryEnsureDestination(Spot destinationSpot)
    {
        if (!_npc.MovementController.IsDestinationSpot(destinationSpot))
            return _npc.MovementController.TrySetDestinationSpot(destinationSpot);
        else // Ensure automatic rotation
            _npc.MovementController.SetAgentAutomaticRotation(true);

        return true;
    }
}
