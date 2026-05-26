using System;
using UnityEngine;
using UnityEngine.AI;

public class NPCInteractionController
{
    public event Action ConversationEndedEvent;

    #region CONSTANTS
    readonly INPC _npc;
    readonly NavMeshAgent _agent;
    readonly float _interactionRange;
    readonly CooldownDecorator _conversationCooldownNode;
    readonly NPCPoolManager _npcPoolManager;
    #endregion

    #region PROPERTIES
    public bool _inAccessZone = true;
    public bool _hasArrivedToMiddlePoint = false;
    public INPC _currentConversationTarget;
    public INPC _lastConversationTarget;

    bool _wasStoppedBeforeTalking;
    #endregion

    #region CONSTRUCTOR
    public NPCInteractionController(INPC npc, NavMeshAgent agent, float interactionRange, CooldownDecorator conversationCooldownNode)
    {
        _npc = npc;
        _agent = agent;
        _interactionRange = interactionRange;
        _conversationCooldownNode = conversationCooldownNode;

        // Get dependency from Service Locator
        _npcPoolManager = ServiceLocator.Instance.Get<NPCPoolManager>();
    }

    public void Reset()
    {
        _inAccessZone = false;
        _hasArrivedToMiddlePoint = false;
        _currentConversationTarget = null;
        _lastConversationTarget = null;
    }
    #endregion

    #region CONVERSATION STATE CHECKING METHODS
    public bool IsAvailableForConversation()
    {
        // Check if in access zone and model is active
        if (_npc.InAccessZone || !_npc.IsOutdoors)
            return false;

        // Check if conversation cooldown has finished (if cooldown system exists)
        if (_conversationCooldownNode != null && _conversationCooldownNode.IsCooldownActive)
        {
            // if (DebugMode)
            //     Debug.LogWarning($"[{Name}.IsAvailableForConversation()] not available for conversation: cooldown active.", GO);

            return false;
        }

        return true;
    }

    public bool IsTalking()
    {
        return _npc.CurrentConversationTarget != null || _npc.ConversationRole != INPC.RoleInConversation.None;
    }

    public bool IsFollowingConversation()
    {
        return IsTalking() && _npc.ConversationRole == INPC.RoleInConversation.Follower;
    }

    public bool IsStillTalkingWith(INPC otherNpc)
    {
        if (!IsTalking())
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.IsStillInConversationWith()] is not talking anymore", _npc.GO);
            return false;
        }

        if (_npc.CurrentConversationTarget != otherNpc)
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.IsStillInConversationWith()] current conversation target is not {otherNpc.Name}", _npc.GO);
            return false;
        }

        // Use generous distance while moving to middle point (2x interaction range)
        // Tighten once both arrive (1x interaction range)
        float maxDistance = (_npc.HasArrivedToMiddlePoint && otherNpc.HasArrivedToMiddlePoint)
            ? _interactionRange
            : _interactionRange * 2f;

        if (Vector3.Distance(_npc.GO.transform.position, otherNpc.GO.transform.position) < maxDistance)
            return true;
        else
        {
            if (_npc.DebugMode)
                Debug.Log($"[{_npc.Name}.IsStillInConversationWith()] too far from {otherNpc.Name} to continue conversation as {_npc.ConversationRole}", _npc.GO);

            return false;
        }
    }
    #endregion

    #region CONVERSATION PERFORMING METHODS
    public bool CanInitiateConversationWithSomeoneNearby<T>() where T : class, INPC
    {
        // False if already talking
        if (IsTalking())
            return false;

        T someoneNearby = _npcPoolManager.GetAnyNearby<T>(_npc.GO.transform.position, _interactionRange, _npc);

        // Return according to handshake
        return TryInitiateConversationWith(someoneNearby);
    }

    public bool TryInitiateConversationWith(INPC otherNpc)
    {
        // False if target is null or already talking, or if self is already talking
        if (otherNpc == null || otherNpc.InteractionController.IsTalking() || IsTalking())
            return false;

        // Set initiator role BEFORE attempting acceptance (handshake)
        _npc.ConversationRole = INPC.RoleInConversation.Initiator;

        // Verify the other NPC accepts the conversation with this as initiator
        if (!otherNpc.InteractionController.CanAcceptNewConversationFrom(_npc))
        {
            // Reset state on failure
            _npc.ConversationRole = INPC.RoleInConversation.None;
            _npc.CurrentConversationTarget = null;
            _npc.CurrentConversationTargetGO = null;
            return false;
        }

        // Assign current conversation target
        _npc.CurrentConversationTarget = otherNpc;
        _npc.CurrentConversationTargetGO = otherNpc.GO;

        // if (_npc.DebugMode)
        //     Debug.Log($"[{_npc.Name}.TryInitiateConversation()] successfully engaged in conversation with {otherNpc.Name}", _npc.GO);

        return true;
    }

    public bool CanAcceptNewConversationFrom(INPC initiator)
    {
        // Verify initiator has claimed the Initiator role (handshake verification)
        if (!initiator.ConversationRole.Equals(INPC.RoleInConversation.Initiator))
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because they are not an Initiator", _npc.GO);
            return false;
        }

        // Reject if not available for conversation
        if (!IsAvailableForConversation())
        {
            // if (DebugMode)
            //     Debug.LogWarning($"[{Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because not available", GO);
            return false;
        }

        // Reject if already talking with someone else or the same as last time
        if (IsTalking() || _npc.LastConversationTarget == initiator)
        {
            // if (DebugMode)
            //     Debug.LogWarning($"[{Name}.CanAcceptConversation()] cannot accept conversation from {initiator.Name} because already talked recently", GO);
            return false;
        }

        // Assign follower role and initiator as current conversation target
        _npc.ConversationRole = INPC.RoleInConversation.Follower;
        _npc.CurrentConversationTargetGO = initiator.GO;
        _npc.CurrentConversationTarget = initiator;

        return true;
    }

    public void Talk()
    {
        _wasStoppedBeforeTalking = _npc.MovementController.IsAgentStopped;
        _npc.MovementController.IsAgentStopped = true;
        _npc.AnimationController.ChangeToIdle();
    }
    #endregion

    #region END CONVERSATION METHODS
    public void EndConversationAsInitiator()
    {
        if (_npc.ConversationRole != INPC.RoleInConversation.Initiator)
        {
            if (_npc.DebugMode)
                Debug.LogWarning($"[{_npc.Name}.EndConversation()] trying to end conversation but not as Initiator", _npc.GO);
            return;
        }

        ConversationEndedEvent?.Invoke();
        ConversationSucceeded();
    }

    public void ConversationSucceeded()
    {
        UpdateConversationState(_npc.CurrentConversationTarget);

        // if (_npc.DebugMode)
        //     Debug.Log($"[{_npc.Name}.ConversationSucceeded()] conversation with {_npc.LastConversationTarget.Name} succeeded", _npc.GO);
    }

    public void ConversationInterrupted()
    {
        UpdateConversationState(null);

        if (_npc.DebugMode)
            Debug.Log($"[{_npc.Name}.ConversationInterrupted()] conversation was interrupted", _npc.GO);
    }

    public void UpdateConversationState(INPC otherNpc)
    {
        _npc.LastConversationTarget = otherNpc;
        _npc.LastConversationTargetGO = otherNpc?.GO;
        _npc.CurrentConversationTarget = null;
        _npc.CurrentConversationTargetGO = null;
        _npc.HasArrivedToMiddlePoint = false;
        _npc.ConversationDuration = 0f;
        _npc.ConversationRole = INPC.RoleInConversation.None;
        _npc.MovementController.IsAgentStopped = _wasStoppedBeforeTalking;
    }
    #endregion
}
