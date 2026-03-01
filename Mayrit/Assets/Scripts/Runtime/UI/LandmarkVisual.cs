using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LandmarkVisual : Billboard
{
    #region EDITOR PROPERTIES
    [Header("Landmark information")]
    [SerializeField] bool _hideIfTooFar = true;
    [SerializeField] OrbitalStateSetting _orbitalStateSetting;
    [Header("Height Adjustment")]
    [SerializeField] bool _fixHeight = false;
    [SerializeField] AnimationCurve _heightMultiplierCurve;
    [SerializeField] float _cameraDistance;
    [SerializeField] float _currentHeightOffset;
    #endregion

    #region INTERNAL PROPERTIES
    public DataSO Data => _orbitalStateSetting.DataToShow;

    float OriginalHeight => _originalPosition.y;
    float CameraDistance
    {
        get
        {
            return Vector3.Distance(_originalPosition, _mainCamera.transform.position);
        }
    }

    VisualElement _rootVisual;
    Label _nameLabel;
    Button _nameButton;
    Vector3 _originalPosition;
    [HideInInspector] public bool IsBlocked = false;
    [HideInInspector] public bool IsSetAsShown = false;

    // Dependency Injection
    UIManager _uiManager;
    SoundManager _soundManager;
    CameraManager _cameraManager;
    #endregion

    #region LIFE CYCLE
    void OnEnable()
    {
        if (_orbitalStateSetting.DataToShow == null)
        {
            Debug.LogWarning($"[LandmarkVisual] No information assigned to {gameObject.name}. Please assign a DataSO with the landmark's information.", this);
            return;
        }

        if (_orbitalStateSetting.Target == null)
            _orbitalStateSetting.Target = transform; // Default to self if no target assigned

        _rootVisual = GetComponent<UIDocument>().rootVisualElement;

        _nameLabel = _rootVisual.Q<Label>("Name");
        _nameButton = _rootVisual.Q<Button>("NameButton");

        if (_nameLabel == null)
        {
            Debug.LogWarning("[LandmarkVisual] No Label with name 'Name' was found in the UIDocument.", this);
            return;
        }

        if (_nameButton == null)
        {
            Debug.LogWarning("[LandmarkVisual] No Button with name 'NameButton' was found in the UIDocument., this");
            return;
        }

        _nameLabel.text = _orbitalStateSetting.DataToShow.Header;
        _nameButton.RegisterCallback<ClickEvent>(OnClicked);

        // Get dependencies from Service Locator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _uiManager.LandmarkVisualizationToggled += OnVisualizationToggled;

        IsShown = _uiManager.IsLandmarkVisualizationOn;

        _originalPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();

        if (_hideIfTooFar)
        {
            if (_isTooFar)
            {
                if (_rootVisual.style.display != DisplayStyle.None)
                    _rootVisual.style.display = DisplayStyle.None;
            }
            else
            {
                if (_rootVisual.style.display != DisplayStyle.Flex)
                    _rootVisual.style.display = DisplayStyle.Flex;
            }
        }

        if (!_fixHeight) return;

        _currentHeightOffset = _heightMultiplierCurve.Evaluate(CameraDistance);
        _cameraDistance = CameraDistance;

        transform.position = new Vector3(transform.position.x, OriginalHeight + _currentHeightOffset, transform.position.z);
    }

    void OnDisable()
    {
        _nameButton.UnregisterCallback<ClickEvent>(OnClicked);
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        _uiManager.LandmarkVisualizationToggled -= OnVisualizationToggled;
    }
    #endregion

    #region PUBLIC METHODS
    public bool IsShown
    {
        get
        {
            //return _rootVisual.style.display == DisplayStyle.Flex;
            return _rootVisual.visible;
        }
        set
        {
            // This causes jittering
            // _rootVisual.style.display = value ?
            //     DisplayStyle.Flex :
            //     DisplayStyle.None;
            _rootVisual.visible = value && _uiManager.IsLandmarkVisualizationOn;
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
            Debug.LogWarning($"[LandmarkVisual] can't orbit around null target.", this);
            return;
        }

        _cameraManager.SwitchToOrbitalCamera(_orbitalStateSetting);
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInThirdPersonState || _cameraManager.IsInPOIState || _cameraManager.IsInOrbitalState)
            IsShown = false;
        else if (!IsBlocked)
            IsShown = _uiManager.IsLandmarkVisualizationOn;
    }

    void OnVisualizationToggled(bool value)
    {
        if (!IsBlocked)
            IsShown = value;
        else
            IsShown = IsSetAsShown && value;
    }
    #endregion
}
