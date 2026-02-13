using System;
using UnityEngine;

public class ModernSuperposition : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Settings")]
    public bool _isActive = false;
    #endregion

    #region INTERNAL PROPERTIES
    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            SetChildrenActive(_isActive);
        }
    }

    bool _wasActive;

    // Dependency Injection
    CameraManager _cameraManager;
    UIManager _uiManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        // Get dependencies from ServiceLocator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        IsActive = _uiManager.IsModernVisualizationOn;
    }

    void Start()
    {
        // Subscribe to events
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _uiManager.ModernVisualizationToggled += OnVisualizationToggled;
    }

    void OnValidate()
    {
        SetChildrenActive(IsActive);
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        _uiManager.ModernVisualizationToggled -= OnVisualizationToggled;
    }
    #endregion

    #region PRIVATE METHODS
    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
    #endregion

    #region CALLBACK METHODS
    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState || _cameraManager.IsInPOIState)
            IsActive = false;
        else
            IsActive = _wasActive;
    }

    void OnVisualizationToggled(bool value)
    {
        IsActive = value;
        _wasActive = value;
    }
    #endregion
}
