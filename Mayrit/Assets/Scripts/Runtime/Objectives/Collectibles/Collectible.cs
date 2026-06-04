using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class Collectible : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Tooltip("Information associated with this collectible")]
    [SerializeField] CollectibleSO _collectibleInfo;

    [Header("Detection Settings")]
    [SerializeField] bool _isFound = false;
    [Tooltip("Layer mask used for trigger checks (defaults to PlayableCharacter layer if present)")]
    [SerializeField] LayerMask _detectionMask = ~0;
    [SerializeField] float _colliderRadius = 2f;
    [SerializeField] GameObject _model;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<Collectible> OnFoundEvent;

    SphereCollider _sphereCollider;

    CameraManager _cameraManager;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }

        _model = transform.GetChild(0).gameObject;

        // If detectionMask is left as default (all bits) and there's a layer named "PlayableCharacter",
        // restrict detection to that layer automatically so it only respond to the player.
        if (_detectionMask == (LayerMask)~0)
        {
            int playableLayer = LayerMask.NameToLayer("PlayableCharacter");
            if (playableLayer != -1)
                _detectionMask = 1 << playableLayer;
        }
    }

    void Start()
    {
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();

        if (_cameraManager != null)
            _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_isFound) return;

        // Check layer mask
        if (((1 << other.gameObject.layer) & _detectionMask) == 0) return;

        SetAsFound();
    }
    #endregion

    #region PUBLIC METHODS
    public CollectibleSO Info => _collectibleInfo;
    public bool IsFound
    {
        get => _isFound;
        set
        {
            if (_isFound != value)
            {
                _isFound = value;
                _model.SetActive(!_isFound);
                _sphereCollider.enabled = !_isFound;
            }
        }
    }
    #endregion

    #region PRIVATE METHODS
    void SetAsFound()
    {
        IsFound = true;
        OnFoundEvent?.Invoke(this);
    }
    #endregion

    #region CALLBACK METHODS
    private void OnCameraStateChanged()
    {
        _model.SetActive(_cameraManager.IsInThirdPersonState);
    }
    #endregion

    #region DEBUG GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, _colliderRadius);


#if UNITY_EDITOR
        if (_collectibleInfo != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_colliderRadius + 1f),
            string.IsNullOrEmpty(_collectibleInfo.Data.Header) ? name : _collectibleInfo.Data.Header);
#endif
    }
    #endregion
}
