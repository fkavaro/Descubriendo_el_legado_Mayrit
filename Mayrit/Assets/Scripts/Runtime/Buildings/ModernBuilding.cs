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
    GameManager _gameManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        // Get dependencies from ServiceLocator
        _gameManager = ServiceLocator.Instance.Get<GameManager>();

        _pointOfInterest.IsBlocked = true;
        IsActive = _gameManager.ModernVisualizationValueSet;
    }

    void Start()
    {
        // Subscribe to events
        _gameManager.StateChangedEvent += OnGameStateChange;
        _gameManager.ModernVisualizationToggled += OnVisualizationToggled;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        _gameManager.StateChangedEvent -= OnGameStateChange;
        _gameManager.ModernVisualizationToggled -= OnVisualizationToggled;
    }
    #endregion

    #region CALLBACK METHODS
    void OnGameStateChange()
    {
        if (_gameManager.IsInThirdPersonState || _gameManager.IsAtTourStopState)
            IsActive = false;
        else if (_gameManager.IsInAerialState)
            IsActive = _gameManager.ModernVisualizationValueSet;
        else if (_gameManager.IsAtPOIState)
            IsActive = _gameManager.ModernVisualizationValueSet && _gameManager.GameplayState.AtPOIState.Data.IsModernBuilding && _pointOfInterest.Data == _gameManager.GameplayState.AtPOIState.Data;
    }

    void OnVisualizationToggled(bool value)
    {
        IsActive = value && _gameManager.IsInAerialState;
    }
    #endregion
}
