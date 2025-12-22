using System;
using UnityEngine;

public class ModernSuperposition : MonoBehaviour
{
    #region PROPERTY HELPERS
    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            SetChildrenActive(_isActive);
        }
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Settings")]
    public bool _isActive = false;

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
    }

    void Start()
    {
        // Subscribe to events
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _uiManager.ModernSuperpositionToggledEvent += ToggleMode;
    }

    void OnValidate()
    {
        SetChildrenActive(IsActive);
    }
    #endregion 

    #region PUBLIC METHODS
    public void ToggleMode()
    {
        IsActive = !IsActive;
    }
    #endregion

    #region PRIVATE METHODS
    void OnCameraStateChanged()
    {
        if (!_cameraManager.IsInSpectatorState)
            IsActive = false;
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
    #endregion
}
