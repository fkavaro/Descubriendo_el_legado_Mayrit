using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
public abstract class ObjectiveObjectBase<T, TData> : MonoBehaviour
where T : ObjectiveObjectBase<T, TData>
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
    protected virtual void OnEnable()
    {
        InitializeCollider();
        SetupDefaultLayerMask();

        if (_model == null)
            _model = transform.Find("Model").gameObject;
        if (_model == null)
            _model = transform.GetChild(0).gameObject;
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
        OnReachedEvent?.Invoke((T)this);
    }

    protected virtual void UpdateVisuals()
    {
        _model.SetActive(!_isReached);
    }

    protected virtual void InitializeCollider()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }
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

    protected virtual void OnCameraStateChanged()
    {
        if (!_isReached)
            _model.SetActive(_cameraManager.IsInThirdPersonState);
    }
    #endregion
}