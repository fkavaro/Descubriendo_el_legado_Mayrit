using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class AccessZone : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Access Zone Settings")]
    [Tooltip("Layer mask for NPCs that should avoid talking in this zone")]
    [SerializeField] private LayerMask _npcLayerMask;
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

    void OnTriggerStay(Collider other)
    {
        // Check if the colliding object is on the NPC layer
        if (!IsNPCLayer(other.gameObject.layer))
            return;

        // Try to get the INPC interface
        if (!other.TryGetComponent<INPC>(out var npc))
        {
            Debug.LogWarning("AccessZone: Colliding object does not implement INPC interface", other.gameObject);
            return;
        }

        if (npc.NotInAccessZone)
            npc.NotInAccessZone = false;
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the colliding object is on the NPC layer
        if (!IsNPCLayer(other.gameObject.layer))
            return;

        // Try to get the INPC interface
        if (!other.TryGetComponent<INPC>(out var npc))
            return;

        if (!npc.NotInAccessZone)
            npc.NotInAccessZone = true;
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
