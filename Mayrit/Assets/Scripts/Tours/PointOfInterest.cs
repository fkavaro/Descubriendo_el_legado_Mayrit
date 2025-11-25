using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class PointOfInterest : MonoBehaviour
{
    #region PROPERTY HELPERS
    public AInformationSO Data => _data;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this POI")]
    [SerializeField] AInformationSO _data;
    [Tooltip("Layer mask used for trigger checks (defaults to PlayableCharacter layer if present)")]
    [SerializeField] LayerMask detectionMask = ~0;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<PointOfInterest> OnVisitedEvent;

    bool _isVisited;
    readonly float _visitRadius = 2f;
    SphereCollider _sphereCollider;
    #endregion

    #region MONOBEHAVIOUR
    /// <summary>
    /// Sets the collider radius and trigger settings.
    /// </summary>
    private void Awake()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _visitRadius;
            _sphereCollider.isTrigger = true;
        }

        // If detectionMask is left as default (all bits) and there's a layer named "PlayableCharacter",
        // restrict detection to that layer automatically so POIs only respond to the player.
        if (detectionMask == (LayerMask)~0)
        {
            int playableLayer = LayerMask.NameToLayer("PlayableCharacter");
            if (playableLayer != -1)
                detectionMask = 1 << playableLayer;
        }
    }

    /// <summary>
    /// Called when another collider enters the POI trigger zone. If the collider is on a valid layer and the POI
    /// hasn't been visited yet, marks it as visited.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (_isVisited) return;

        // Check layer mask
        if (((1 << other.gameObject.layer) & detectionMask) == 0) return;

        SetAsVisited();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Marks as unvisited and enables the POI collider.
    /// </summary>
    public void Activate()
    {
        _isVisited = false;
        _sphereCollider.enabled = true;
    }

    /// <summary>
    /// Marks as visited and disables the POI collider.
    /// </summary>
    public void Deactivate()
    {
        _isVisited = true;
        _sphereCollider.enabled = false;
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Marks this POI as visited and invokes the OnVisited event.
    /// </summary>
    void SetAsVisited()
    {
        if (_isVisited) return;

        _isVisited = true;
        OnVisitedEvent?.Invoke(this);
    }
    #endregion

    #region DEBUG GIZMOW
    void OnDrawGizmos()
    {
        Gizmos.color = _isVisited ? Color.green : Color.yellow;
        Gizmos.DrawSphere(transform.position, _visitRadius);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (_visitRadius + 0.2f),
            string.IsNullOrEmpty(_data.Header) ? name : _data.Header);
#endif
    }
    #endregion
}

