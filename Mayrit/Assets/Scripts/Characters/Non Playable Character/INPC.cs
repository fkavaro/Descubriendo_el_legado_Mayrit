using System;
using UnityEngine.AI;
using UnityEngine;

public interface INPC : ICharacter
{
    public enum RoleInConversation
    {
        Initiator,
        Follower,
        None
    }

    #region PROPERTIES HELPERS
    public event Action ConversationEndedEvent;

    public NavMeshAgent Agent { get; }
    NPCMovementController MovementController { get; }
    float AvoidanceRadius { get; }
    float MaxSamplingDistance { get; }
    int AvoidancePriorityVariance { get; }
    int BaseAvoidancePriority { get; }
    public float WalkSpeedVariance { get; }
    bool IsStopped { get; set; }
    public string GivenName { get; }
    public string FamilyName { get; }
    RoleInConversation ConversationRole { get; set; }
    bool NotInAccessZone { get; set; }
    bool HasArrivedToMiddlePoint { get; set; }
    public INPC CurrentConversationTarget { get; set; }
    public INPC LastConversationTarget { get; set; }
    public float ConversationDuration { get; set; }
    #endregion

    #region METHODS
    /// <summary>
    /// Sets the NPC's full name.
    /// </summary>
    public void SetFullName(string given, string family);

    /// <summary>
    /// Returns true if is not in an access zone and its model is active.
    /// </summary>
    /// <returns></returns>
    public bool IsAvailableForConversation();

    /// <summary>
    /// Returns true if the NPC is available for conversation and has any conversation role assigned.
    /// </summary>
    public bool IsTalking();

    /// <summary>
    /// Returns true if still talking with the other npc and both are close enough.
    /// </summary>
    public bool IsStillTalkingWith(INPC otherNpc);

    /// <summary>
    /// Returns true if the NPC has follower role.
    /// </summary>
    public bool IsFollowingConversation();

    /// <summary>
    /// Attempts to initiate a conversation with a target NPC (performs handshake).
    /// Sets initiator role, calls acceptance, and handles cleanup on failure.
    /// </summary>
    /// <returns>True if the target accepted the conversation</returns>
    public bool TryInitiateConversationWith(INPC target);

    /// <summary>
    /// Returns true if the character is available to start an interaction.
    /// </summary>
    /// <returns>True if can accept the conversation</returns>
    public bool CanAcceptNewConversationFrom(INPC initiator);

    /// <summary>
    /// Called on the initiator character to start the interaction
    /// </summary>
    public void Talk();

    /// <summary>
    /// Only if initiator:
    /// Updates the conversation state with current target NPC and invokes conversation ended event.
    /// </summary>
    public void EndConversationAsInitiator();

    /// <summary>
    /// Updates the conversation state with current target NPC
    /// </summary>
    public void ConversationSucceeded();

    /// <summary>
    /// Updates the conversation state will null target
    /// </summary>
    public void ConversationInterrupted();

    /// <summary>
    /// Update conversation state with optional other NPC and its GameObject.
    /// </summary>
    public void UpdateConversationState(INPC otherNpc = null, GameObject otherNpcGO = null);
    #endregion
}
