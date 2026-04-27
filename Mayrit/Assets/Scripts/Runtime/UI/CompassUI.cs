using UnityEngine;
using UnityEngine.UIElements;

public class CompassUI : AUIState
{
    #region PROPERTIES
    readonly VisualElement _rootParent;
    readonly VisualElement _root;

    VisualElement _cardinalDirections;
    VisualElement _nextTourStopVisual;
    VisualElement _nextTourStopDirection;

    Camera _mainCamera;
    TourStop _nextTourStop;

    // Dependency Injection
    TourManager _tourManager;
    TutorialManager _tutorialManager;
    #endregion

    #region CONSTRUCTOR
    public CompassUI(UIDocument uIDocument, VisualElement root) : base("Compass", uIDocument)
    {
        if (root == null)
        {
            Debug.LogError("CompassUI: Root VisualElement is null");
            return;
        }

        _root = root;
        _rootParent = _root.parent;
    }

    protected override void ConfigureUIElementsOnAwake()
    {
        _cardinalDirections = GetByName<VisualElement>("CardinalDirections", _root);
        _nextTourStopVisual = GetByName<VisualElement>("NextTourStop", _root);
        _nextTourStopDirection = GetByName<VisualElement>("NextTourStopDirection", _root);
    }
    #endregion

    #region INHERITED METHODS
    public override void AwakeState()
    {
        base.AwakeState();

        IsShown = false;
        IsNextTourStopShown = false;
    }

    public override void StartState()
    {
        _tutorialManager = ServiceLocator.Instance.Get<TutorialManager>();

        if (!_tutorialManager.HasCompletedTutorial)
            _tutorialManager.ShowCompassTutorialEvent += OnShowCompassTutorialEvent;
        else
            _rootParent.style.display = DisplayStyle.Flex;

        _mainCamera = Camera.main; // TODO get from Camera Manager
        FixCardinalDirections();
    }

    public override void UpdateState()
    {
        FixCardinalDirections();

        if (IsNextTourStopShown)
            FixTourStopDirection();
    }
    #endregion

    #region PUBLIC METHODS
    public bool IsShown
    {
        get => _rootParent.style.display == DisplayStyle.Flex && _root.style.display == DisplayStyle.Flex;
        set => _root.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public bool IsNextTourStopShown
    {
        get => _nextTourStopVisual.style.display == DisplayStyle.Flex;
        set
        {
            if (value)
            {
                if (_tourManager == null)
                    _tourManager = ServiceLocator.Instance.Get<TourManager>();

                if (_tourManager == null)
                {
                    Debug.LogWarning("CompassUI: TourManager not found in ServiceLocator");
                    return;
                }

                _nextTourStopVisual.style.display = DisplayStyle.Flex;
            }
            else
            {
                _nextTourStopVisual.style.display = DisplayStyle.None;
                _nextTourStopDirection.style.display = DisplayStyle.None;

                _tourManager = null;
            }
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

    void FixTourStopDirection()
    {
        if (_tourManager.CurrentTour == null || _tourManager.CurrentTour.NextTourStop == null)
        {
            _nextTourStopVisual.style.display = DisplayStyle.None;
            _nextTourStopDirection.style.display = DisplayStyle.None;
            return;
        }

        _nextTourStop = _tourManager.CurrentTour.NextTourStop;

        // Get the direction to the TourStop in world space
        Vector3 toTourStop = _nextTourStop.transform.position - _mainCamera.transform.position;
        Vector3 flatToTourStop = Vector3.ProjectOnPlane(toTourStop, Vector3.up).normalized;
        Vector3 flatForward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;

        if (flatToTourStop.sqrMagnitude > 0.0001f && flatForward.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(flatForward, flatToTourStop, Vector3.up);
            _nextTourStopDirection.style.rotate = new Rotate(angle);
        }

        if (_nextTourStopVisual.style.display == DisplayStyle.None)
            _nextTourStopVisual.style.display = DisplayStyle.Flex;

        if (_nextTourStopDirection.style.display == DisplayStyle.None)
            _nextTourStopDirection.style.display = DisplayStyle.Flex;
    }
    #endregion

    void OnShowCompassTutorialEvent(bool isShown)
    {
        _rootParent.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
        IsShown = isShown;

        if (isShown) _tutorialManager.ShowCompassTutorialEvent -= OnShowCompassTutorialEvent;
    }
}
