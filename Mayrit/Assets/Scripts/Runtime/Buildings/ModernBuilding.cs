using System;
using UnityEngine;

public class ModernBuilding : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Settings")]
    [SerializeField] bool _isActive = false;
    [SerializeField] PointOfInterest _pointOfInterest;
    [SerializeField] GameObject _model;
    #endregion

    #region INTERNAL PROPERTIES
    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            _pointOfInterest.IsSetAsShown = _isActive;
            _pointOfInterest.IsShown = _isActive;
            _model.SetActive(_isActive);
        }
    }


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

        _pointOfInterest.IsBlocked = true;
        IsActive = _uiManager.ModernVisualizationValueSet;
    }

    void Start()
    {
        // Subscribe to events
        _cameraManager.CameraStateChangedEvent += FixActivation;
        _uiManager.ContextualPanelHiddenEvent += FixActivation;
        _uiManager.ModernVisualizationToggled += OnVisualizationToggled;
        _uiManager.ContextualPanelShownEvent += OnContextualPanelShown;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _cameraManager.CameraStateChangedEvent -= FixActivation;
        _uiManager.ContextualPanelHiddenEvent -= FixActivation;
        _uiManager.ModernVisualizationToggled -= OnVisualizationToggled;
        _uiManager.ContextualPanelShownEvent -= OnContextualPanelShown;
    }
    #endregion

    #region CALLBACK METHODS
    void FixActivation()
    {
        if (_cameraManager.IsInThirdPersonState || _cameraManager.IsInTourStopState)
            IsActive = false;
        else if (_cameraManager.IsInAerialState)
            IsActive = _uiManager.ModernVisualizationValueSet;
    }

    void OnVisualizationToggled(bool value)
    {
        IsActive = value && _cameraManager.IsInAerialState;
    }

    void OnContextualPanelShown(DataSO data)
    {
        if (!data.IsModernBuilding)
        {
            IsActive = false;
            return;
        }

        IsActive = (data == _pointOfInterest.Data) && _uiManager.ModernVisualizationValueSet;
    }
    #endregion
}
