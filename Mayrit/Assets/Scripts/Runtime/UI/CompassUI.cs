using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class CompassUI
{
    #region PROPERTIES
    readonly VisualElement _root, _cardinalDirections, _nextPOIVisual, _nextPoiDirection;

    bool _isNextPOIShown;
    Camera _mainCamera;
    PointOfInterest _nextPOI;

    // Dependency Injection
    TourManager _tourManager;
    #endregion

    #region CONSTRUCTOR
    public CompassUI(VisualElement root)
    {
        _root = root;

        _cardinalDirections = _root.Q<VisualElement>("CardinalDirections");
        _nextPOIVisual = _root.Q<VisualElement>("NextPOI");
        _nextPoiDirection = _root.Q<VisualElement>("NextPOIDirection");

        if (_cardinalDirections == null)
            Debug.LogWarning("CompassUI: 'CardinalDirections' not found");
        if (_nextPOIVisual == null)
            Debug.LogWarning("CompassUI: 'NextPOI' not found");
        if (_nextPoiDirection == null)
            Debug.LogWarning("CompassUI: 'POIDirection' not found");

        IsShown(false);
        IsNextPOIShown(false);
    }
    #endregion

    #region PUBLIC METHODS
    public void Start()
    {
        _mainCamera = Camera.main; // TODO get from Camera Manager
        FixCardinalDirections();
        IsShown(true);
    }

    public void Update()
    {
        _mainCamera = Camera.main; // TODO get from Camera Manager
        FixCardinalDirections();

        if (_isNextPOIShown)
            FixPOIDirection();
    }

    public void IsShown(bool isShown)
    {
        _root.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void IsNextPOIShown(bool isShown)
    {
        _isNextPOIShown = isShown;

        if (isShown)
        {
            _nextPOIVisual.style.display = DisplayStyle.Flex;

            if (_tourManager == null)
                _tourManager = ServiceLocator.Instance.Get<TourManager>();

            if (_tourManager == null)
            {
                Debug.LogWarning("CompassUI: TourManager not found in ServiceLocator");
                _isNextPOIShown = false;
            }
        }
        else
        {
            _nextPOIVisual.style.display = DisplayStyle.None;
            _nextPoiDirection.style.display = DisplayStyle.None;

            _tourManager = null;
        }
    }
    #endregion

    #region PRIVATE METHODS
    void FixCardinalDirections()
    {
        // Rotate the cardinal directions in the opposite direction of the camera's Y rotation
        float cameraYRotation = _mainCamera.transform.eulerAngles.y;
        _cardinalDirections.style.rotate = new Rotate(-cameraYRotation);
    }

    void FixPOIDirection()
    {
        if (_tourManager.CurrentTour == null || _tourManager.CurrentTour.NextPOI == null)
        {
            _nextPOIVisual.style.display = DisplayStyle.None;
            _nextPoiDirection.style.display = DisplayStyle.None;
            return;
        }

        _nextPOI = _tourManager.CurrentTour.NextPOI;

        // Get the direction to the POI in world space
        Vector3 toPoi = _nextPOI.transform.position - _mainCamera.transform.position;
        Vector3 flatToPoi = Vector3.ProjectOnPlane(toPoi, Vector3.up).normalized;
        Vector3 flatForward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;

        if (flatToPoi.sqrMagnitude > 0.0001f && flatForward.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(flatForward, flatToPoi, Vector3.up);
            _nextPoiDirection.style.rotate = new Rotate(angle);
        }

        if (_nextPOIVisual.style.display == DisplayStyle.None)
            _nextPOIVisual.style.display = DisplayStyle.Flex;

        if (_nextPoiDirection.style.display == DisplayStyle.None)
            _nextPoiDirection.style.display = DisplayStyle.Flex;
    }
    #endregion
}
