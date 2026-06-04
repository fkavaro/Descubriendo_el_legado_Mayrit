using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
public abstract class AObjective<T, TData> : MonoBehaviour
where T : AObjective<T, TData>
{
    #region EDITOR PROPERTIES
    [Header("Objective Settings")]
    [SerializeField] protected TData _data;
    [SerializeField] protected bool _isReached = false;
    [SerializeField] protected float _colliderRadius = 2f;
    [SerializeField] protected LayerMask _detectionMask = ~0;
    [SerializeField] protected GameObject _model;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<T> OnReachedEvent;
    protected SphereCollider _sphereCollider;
    protected CameraManager _cameraManager;
    #endregion

    #region PUBLIC ACCESSORS
    public TData Data => _data;
    public bool IsReached => _isReached;
    #endregion

    #region LIFE CYCLE
    protected virtual void Awake()
    {
        InitializeCollider();
        SetupDefaultLayerMask();

        if (_model == null)
            _model = transform.Find("Model").gameObject;
        if (_model == null)
            _model = transform.GetChild(0).gameObject;

        Complete();
    }

    protected virtual void Start()
    {
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
    }

    protected virtual void OnDisable()
    {
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_isReached) return;
        if (((1 << other.gameObject.layer) & _detectionMask) == 0) return;

        Complete();
        OnReachedEvent?.Invoke((T)this);
    }
    #endregion

    #region LOGIC
    public virtual void Reset()
    {
        _isReached = false;
        if (_sphereCollider != null) _sphereCollider.enabled = true;
        UpdateVisuals();
    }

    public virtual void Complete()
    {
        _isReached = true;
        _sphereCollider.enabled = false;
        UpdateVisuals();
    }

    protected virtual void UpdateVisuals()
    {
        if (_data != null)
            _model.SetActive(!_isReached);
        else
            _model.SetActive(false);
    }

    protected virtual void InitializeCollider()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }
    }

    protected virtual void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState)
            UpdateVisuals();
    }

    private void SetupDefaultLayerMask()
    {
        if (_detectionMask == (LayerMask)~0)
        {
            int playableLayer = LayerMask.NameToLayer("PlayableCharacter");
            if (playableLayer != -1)
                _detectionMask = 1 << playableLayer;
        }
    }
    #endregion
}