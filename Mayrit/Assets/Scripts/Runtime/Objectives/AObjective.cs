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
    [SerializeField] protected GameObject _vfx;
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
        if (_vfx == null)
            _vfx = transform.Find("VFX").gameObject;
        if (_vfx == null && transform.childCount > 1)
            _vfx = transform.GetChild(1).gameObject;

        CompleteAndUpdateVisuals();
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

    protected void OnTriggerEnter(Collider other)
    {
        if (_isReached) return;
        if (((1 << other.gameObject.layer) & _detectionMask) == 0) return;

        OnTriggerEnterAction();
        OnReachedEvent?.Invoke((T)this);
    }

    protected virtual void OnTriggerEnterAction()
    {
        CompleteAndUpdateVisuals();
    }
    #endregion

    #region LOGIC
    public virtual void Reset()
    {
        _isReached = false;
        _sphereCollider.enabled = true;
        UpdateVisuals();
    }

    public virtual void CompleteAndUpdateVisuals()
    {
        Complete();
        UpdateVisuals();
    }

    protected virtual void Complete()
    {
        _isReached = true;
        _sphereCollider.enabled = false;
    }

    protected virtual void UpdateVisuals()
    {
        if (_data != null)
        {
            _model.SetActive(!_isReached);
            _vfx.SetActive(!_isReached);
        }
        else
        {
            _model.SetActive(false);
            _vfx.SetActive(false);
        }
    }

    void InitializeCollider()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState)
            UpdateVisuals();
    }

    void SetupDefaultLayerMask()
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