using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PointOfInterest : Billboard
{
    #region EDITOR PROPERTIES
    [Header("Point of interest information")]
    [SerializeField] OrbitalStateSetting _orbitalStateSetting;
    [Header("Height Adjustment")]
    [SerializeField] bool _hideIfTooFar = true;
    [SerializeField] bool _fixHeight = false;
    [SerializeField] AnimationCurve _heightMultiplierCurve;
    [SerializeField] float _cameraDistance;
    [SerializeField] float _currentHeightOffset;
    #endregion

    #region INTERNAL PROPERTIES
    public DataSO Data => _orbitalStateSetting.DataToShow;

    float OriginalHeight => _originalPosition.y;
    float CameraDistance => Vector3.Distance(_originalPosition, _mainCamera.transform.position);

    UIDocument _uiDocument;
    Label _nameLabel;
    Button _nameButton;
    Vector3 _originalPosition;
    [HideInInspector] public bool IsBlocked = false;
    [HideInInspector] public bool IsSetAsShown = false;
    bool _shownDueToTutorial = true;

    // Dependency Injection
    UIManager _uiManager;
    SoundManager _soundManager;
    CameraManager _cameraManager;
    TutorialManager _tutorialManager;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        if (_orbitalStateSetting.DataToShow == null)
        {
            Debug.LogWarning($"[PointOfInterest] No information assigned to {gameObject.name}. Please assign a DataSO.", this);
            return;
        }

        if (_orbitalStateSetting.Target == null)
            _orbitalStateSetting.Target = transform;

        if (!SetupElements()) return;
    }

    void Awake()
    {
        _uiDocument = GetComponent<UIDocument>();
        _originalPosition = transform.position;
    }

    protected override void Start()
    {
        base.Start();

        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _tutorialManager = ServiceLocator.Instance.Get<TutorialManager>();

        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _uiManager.POIsVisualizationToggledEvent += OnVisualizationToggled;
        _tutorialManager.ShowPointsOfInterestEvent += OnShowInTutorialEvent;
        _tutorialManager.TutorialCompletedEvent += OnTutorialCompleted;

        IsShown = true;
    }

    protected override void Update()
    {
        if (IsShown)
            base.Update();

        if (_hideIfTooFar)
            IsShown = !_isTooFar;

        if (!_fixHeight) return;

        _cameraDistance = CameraDistance;
        _currentHeightOffset = _heightMultiplierCurve.Evaluate(_cameraDistance);

        transform.position = new Vector3(transform.position.x, OriginalHeight + _currentHeightOffset, transform.position.z);
    }

    void OnDisable()
    {
        _nameButton?.UnregisterCallback<ClickEvent>(OnClicked);
        if (_cameraManager != null) _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        if (_uiManager != null) _uiManager.POIsVisualizationToggledEvent -= OnVisualizationToggled;
        if (_tutorialManager != null)
        {
            _tutorialManager.ShowPointsOfInterestEvent -= OnShowInTutorialEvent;
            _tutorialManager.TutorialCompletedEvent -= OnTutorialCompleted;
        }
    }
    #endregion

    #region PRIVATE METHODS
    bool SetupElements()
    {
        var root = _uiDocument.rootVisualElement;
        _nameButton?.UnregisterCallback<ClickEvent>(OnClicked);
        _nameLabel = root.Q<Label>("Name");
        _nameButton = root.Q<Button>("NameButton");

        if (_nameLabel == null)
        {
            Debug.LogWarning("[PointOfInterest] No Label with name 'Name' was found in the UIDocument.", this);
            return false;
        }

        if (_nameButton == null)
        {
            Debug.LogWarning("[PointOfInterest] No Button with name 'NameButton' was found in the UIDocument.", this);
            return false;
        }

        _nameLabel.text = _orbitalStateSetting.DataToShow.Header;
        _nameButton.RegisterCallback<ClickEvent>(OnClicked);
        return true;
    }
    #endregion

    #region PUBLIC METHODS
    public bool IsShown
    {
        get => _uiDocument.enabled;
        set
        {
            if (_uiManager == null || _cameraManager == null) return;

            bool resolvedValue = value
                && (!IsBlocked || IsSetAsShown)
                && _uiManager.POIsVisibilityValueSet
                && _cameraManager.IsInAerialState
                && _shownDueToTutorial;

            if (_uiDocument.enabled == resolvedValue) return;

            _uiDocument.enabled = resolvedValue;

            if (resolvedValue)
            {
                base.Update();
                SetupElements();
            }
        }
    }
    #endregion

    #region CALLBACK METHODS

    void OnClicked(ClickEvent evt)
    {
        _uiManager.ShowContextualPanel(_orbitalStateSetting.DataToShow);
        _soundManager.PlayButtonClickSFX();

        if (_orbitalStateSetting.Target == null)
        {
            Debug.LogWarning($"[PointOfInterest] can't orbit around null target.", this);
            return;
        }

        _cameraManager.SwitchToOrbitalCamera(_orbitalStateSetting);
    }

    void OnCameraStateChanged() => IsShown = true;

    void OnVisualizationToggled(bool value) => IsShown = value;

    void OnShowInTutorialEvent(bool areShown)
    {
        _shownDueToTutorial = areShown;
        IsShown = _shownDueToTutorial;
    }

    void OnTutorialCompleted()
    {
        _tutorialManager.ShowPointsOfInterestEvent -= OnShowInTutorialEvent;
        _tutorialManager.TutorialCompletedEvent -= OnTutorialCompleted;
    }
    #endregion

    #region DEBUG GIZMOS
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Data != null)
            UnityEditor.Handles.Label(transform.position, string.IsNullOrEmpty(Data.Header) ? name : Data.Header);
#endif
    }
    #endregion
}
