using System;
using UnityEngine;

public class ModernBuilding : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Header("Settings")]
    [SerializeField] bool _isActive = false;
    [SerializeField] LandmarkVisual _landmarkVisual;
    [SerializeField] GameObject _model;
    #endregion

    #region INTERNAL PROPERTIES
    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            _landmarkVisual.IsShown = _isActive;
            _landmarkVisual.IsSetAsShown = _isActive;
            _model.SetActive(_isActive);
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

        _landmarkVisual.IsBlocked = true;
        IsActive = _uiManager.IsModernVisualizationOn;
        _wasActive = IsActive;
    }

    void Start()
    {
        // Subscribe to events
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _uiManager.ModernVisualizationToggled += OnVisualizationToggled;
        _uiManager.ContextualPanelShownEvent += OnContextualShownPanel;
        _uiManager.ContextualPanelHiddenEvent += OnContextualHiddenPanel;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        _uiManager.ModernVisualizationToggled -= OnVisualizationToggled;
        _uiManager.ContextualPanelShownEvent -= OnContextualShownPanel;
        _uiManager.ContextualPanelHiddenEvent -= OnContextualHiddenPanel;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState || _cameraManager.IsInPOIState)
            IsActive = false;
        else if (_wasActive)
            IsActive = true;
    }

    void OnVisualizationToggled(bool value)
    {
        IsActive = value;
        _wasActive = value;
    }

    void OnContextualShownPanel(DataSO data, bool isCharacterData)
    {
        if (isCharacterData)
            IsActive = false;

        if (data == null || _landmarkVisual.Data == null)
            return;

        IsActive = data == _landmarkVisual.Data;
    }

    void OnContextualHiddenPanel()
    {
        IsActive = _wasActive;
    }
    #endregion
}
