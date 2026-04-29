using System;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class TourStop : MonoBehaviour
{
    #region PROPERTY HELPERS
    public DataSO Data => _data;
    public CinemachineCamera Camera => _camera;
    #endregion

    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this stop")]
    [SerializeField] DataSO _data;

    [Header("Detection Settings")]
    [SerializeField] bool _isVisited;
    [Tooltip("Layer mask used for trigger checks (defaults to PlayableCharacter layer if present)")]
    [SerializeField] LayerMask _detectionMask = ~0;
    [SerializeField] float _colliderRadius = 2f;
    [SerializeField] GameObject _vfxGO;

    [Header("Camera")]
    [SerializeField] CinemachineCamera _camera;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<TourStop> OnVisitedEvent;

    bool _isActive;
    bool _initialized;
    SphereCollider _sphereCollider;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        if (_initialized) return;

        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }

        // If detectionMask is left as default (all bits) and there's a layer named "PlayableCharacter",
        // restrict detection to that layer automatically so POIs only respond to the player.
        if (_detectionMask == (LayerMask)~0)
        {
            int playableLayer = LayerMask.NameToLayer("PlayableCharacter");
            if (playableLayer != -1)
                _detectionMask = 1 << playableLayer;
        }

        _initialized = true;
        Deactivate();
    }

    /// <summary>
    /// Called when another collider enters the TourStop trigger zone. If the collider is on a valid layer and the stop
    /// hasn't been visited yet, marks it as visited.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (_isVisited) return;

        // Check layer mask
        if (((1 << other.gameObject.layer) & _detectionMask) == 0) return;

        SetAsVisited();
    }

    void Update()
    {
        if (!_isActive) return;

        // TODO
        //! Rotate VFX
        if (_vfxGO != null)
            _vfxGO.transform.Rotate(Vector3.up, 50f * Time.deltaTime, Space.World);
    }
    #endregion

    #region PUBLIC METHODS
    public void Activate()
    {
        if (_sphereCollider != null)
            _sphereCollider.enabled = true;

        _vfxGO.SetActive(true);
        _isActive = true;
        enabled = true;
    }

    public void Deactivate()
    {
        if (_sphereCollider != null)
            _sphereCollider.enabled = false;

        _vfxGO.SetActive(false);
        _isActive = false;
        enabled = false;
    }

    public void Reset()
    {
        _isVisited = false;
        Deactivate();
    }
    #endregion

    #region PRIVATE METHODS
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
        Gizmos.color = _isVisited ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, _colliderRadius);

#if UNITY_EDITOR
        if (_data != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_colliderRadius + 0.2f),
            string.IsNullOrEmpty(_data.Header) ? name : _data.Header);
#endif
    }
    #endregion
}

