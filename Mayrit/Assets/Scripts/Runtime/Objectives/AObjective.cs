using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
public abstract class AObjective<T, TData> : MonoBehaviour
where T : AObjective<T, TData>
{
    #region EDITOR PROPERTIES
    [Header("Objective Settings")]
    [SerializeField] protected TData _data;
    public bool IsCurrentObjective = false;
    [SerializeField] protected bool _isReached = false;
    [SerializeField] protected float _colliderRadius = 2f;
    [SerializeField] protected LayerMask _detectionMask = ~0;
    [SerializeField] protected GameObject _model;
    [SerializeField] protected GameObject _vfx;
    #endregion

    #region INTERNAL PROPERTIES
    public event Action<T> OnReachedEvent;
    protected SphereCollider _sphereCollider;
    protected GameManager _gameManager;
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
    }

    protected virtual void Start()
    {
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _gameManager.StateChangedEvent += OnGameStateChanged;
    }

    protected virtual void OnDisable()
    {
        _gameManager.StateChangedEvent -= OnGameStateChanged;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (_isReached) return;
        if (((1 << other.gameObject.layer) & _detectionMask) == 0) return;

        OnReachedEvent?.Invoke((T)this);
    }
    #endregion

    #region LOGIC
    public virtual void Enable()
    {
        _isReached = false;
        _sphereCollider.enabled = true;
        UpdateModel();
        UpdateVFX();
    }

    public virtual void Disable()
    {
        Complete();
        UpdateModel();
        UpdateVFX();
    }

    public virtual void Complete()
    {
        _isReached = true;
        _sphereCollider.enabled = false;
    }

    public virtual void UpdateModel()
    {
        if (_data != null)
            _model.SetActive(!_isReached && IsCurrentObjective);
        else
            _model.SetActive(false);
    }

    public virtual void UpdateVFX()
    {
        if (_data != null)
            _vfx.SetActive(!_isReached && IsCurrentObjective);
        else
            _vfx.SetActive(false);
    }

    void InitializeCollider()
    {
        if (TryGetComponent(out _sphereCollider))
        {
            _sphereCollider.radius = _colliderRadius;
            _sphereCollider.isTrigger = true;
        }
    }

    void OnGameStateChanged()
    {
        if (_gameManager.IsInThirdPersonState)
        {
            UpdateModel();
            UpdateVFX();
        }
        else
            OnGameStateChangeNotThirdPerson();
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

    protected virtual void OnGameStateChangeNotThirdPerson()
    {
        _model.SetActive(false);
        _vfx.SetActive(false);
    }
}