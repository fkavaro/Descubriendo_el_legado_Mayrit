using System;
using UnityEngine;

public class Collectible : AObjective<Collectible, CollectibleSO>
{
    public OrbitalStateSetting OrbitalStateSetting => _orbitalStateSetting;

    [SerializeField] OrbitalStateSetting _orbitalStateSetting;

    UIManager _uiManager;

    protected override void Awake()
    {
        base.Awake();

        _orbitalStateSetting.DataToShow = _data.Data;
        _orbitalStateSetting.TransitionToApply = CameraTransition.ThirdPersonCamera;
    }

    protected override void Start()
    {
        base.Start();

        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _uiManager.StateChangedEvent += OnUIStateChanged;
    }

    protected override void OnTriggerEnterAction()
    {
        Complete();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _uiManager.StateChangedEvent -= OnUIStateChanged;
    }

    void OnUIStateChanged()
    {
        if (_uiManager.IsInContextualPanelState) return;

        if (_isReached)
            UpdateVisuals();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, _colliderRadius);

#if UNITY_EDITOR
        if (_data != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_colliderRadius + 1f),
            string.IsNullOrEmpty(_data.Data.Header) ? name : _data.Data.Header);
#endif
    }
}