using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class AccessZone : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Access Zone Settings")]
    [Tooltip("Layer mask for NPCs that should avoid talking in this zone")]
    [SerializeField] private LayerMask _npcLayerMask;
    #endregion

    #region PROPERTIES
    /// <summary>
    /// Set of NPCs currently in this access zone
    /// </summary>
    private readonly HashSet<INPC> _npcsInZone = new();
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        // Ensure this GameObject has a trigger collider
        Collider triggerCollider = GetComponent<Collider>();

        if (!triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;

        // Setup NPC layer mask if not already set
        if (_npcLayerMask == 0)
        {
            _npcLayerMask = LayerMask.GetMask("NonPlayableCharacter");
            if (_npcLayerMask == 0)
                Debug.LogWarning($"NonPlayableCharacter layer not found. Please ensure NPCs are on this layer.", gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"AccessZone: OnTriggerEnter by {other.gameObject.name}", gameObject);

        // Check if the colliding object is on the NPC layer
        if (!IsNPCLayer(other.gameObject.layer))
        {
            Debug.Log($"AccessZone: Ignored object on layer {LayerMask.LayerToName(other.gameObject.layer)}", gameObject);
            return;
        }

        // Try to get the INPC interface
        if (!other.TryGetComponent<INPC>(out var npc))
        {
            Debug.LogWarning("AccessZone: Colliding object does not implement INPC interface", other.gameObject);
            return;
        }

        // Add NPC to the zone, prevent talking, and invoke enter event
        if (_npcsInZone.Add(npc))
        {
            npc.ShouldTalk = false;
            Debug.Log($"NPC {npc.GivenName} entered AccessZone", gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the colliding object is on the NPC layer
        if (!IsNPCLayer(other.gameObject.layer))
            return;

        // Try to get the INPC interface
        if (!other.TryGetComponent<INPC>(out var npc))
            return;

        // Remove NPC from the zone, allow talking, and invoke exit event
        if (_npcsInZone.Remove(npc))
        {
            npc.ShouldTalk = true;
            Debug.Log($"NPC {npc.GivenName} exited AccessZone", gameObject);
        }
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Checks if a layer is the NPC layer
    /// </summary>
    bool IsNPCLayer(int layer)
    {
        return (_npcLayerMask.value & (1 << layer)) != 0;
    }
    #endregion
}
