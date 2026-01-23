using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    Tour _currentTour;

    Button _pauseButton;
    VisualElement _tourArea,
        _onTourEndVisual;
    Label _tourName,
        _tourDescription;

    // Dependency Injection
    TourManager _tourManager;
    ProgressManager _progressManager;
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UIDocument uiDocument)
    : base("PlayerHUD", uiDocument) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = _screen.Q<Button>("PauseButton");
        _tourArea = _screen.Q<VisualElement>("TourArea");
        _tourName = _tourArea.Q<Label>("Name");
        _tourDescription = _tourArea.Q<Label>("Description");
        _onTourEndVisual = _screen.Q<VisualElement>("OnTourEnd");

        if (_pauseButton == null)
            Debug.LogWarning("_pauseButton not found");
        if (_tourArea == null)
            Debug.LogWarning("TourArea not found");
        if (_tourName == null)
            Debug.LogWarning("_tourName not found");
        if (_tourDescription == null)
            Debug.LogWarning("_tourDescription not found");
        if (_onTourEndVisual == null)
            Debug.LogWarning("_onTourEndVisual not found");
    }

    protected override void RegisterUICallbacksOnAwake()
    {
        _pauseButton.RegisterCallback<ClickEvent>(OnPauseClicked);
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _tourManager = ServiceLocator.Instance.Get<TourManager>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
    }

    protected override void SubscribeToServicesEventsOnStart()
    {
        base.SubscribeToServicesEventsOnStart();
        _tourManager.TourCompletedEvent += OnTourEnded;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
    }

    public override void StartState()
    {
        base.StartState();

        if (_currentTour == null)
            _currentTour = _tourManager.CurrentTour;

        if (_currentTour.IsCompleted)
        {
            _tourArea.style.display = DisplayStyle.None;
            _onTourEndVisual.style.display = DisplayStyle.Flex;
        }
        else
        {
            if (!_wasContextualPanelShown)
            {
                _onTourEndVisual.style.display = DisplayStyle.None;
                _tourArea.style.display = DisplayStyle.Flex;
                UpdateTourInfo();
            }
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        // Unlock cursor and make it visible (has been lock in 3rd person camera state start)
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }
    #endregion

    #region HUD STATE INHERITED METHODS
    protected override void OnContextualPanelShown()
    {
        _tourArea.style.display = DisplayStyle.None;
        _onTourEndVisual.style.display = DisplayStyle.None;
    }

    protected override void OnContextualPanelHidden()
    {
        if (_currentTour != null && _currentTour.IsCompleted)
            _onTourEndVisual.style.display = DisplayStyle.Flex;
        else
            _tourArea.style.display = DisplayStyle.Flex;
    }
    #endregion

    void UpdateTourInfo()
    {
        if (_currentTour == null)
        {
            Debug.LogWarning($"{_stateName}: CurrentTour is null on UpdateTourInfo");
            return;
        }

        _tourName.text = _currentTour.Data.Header;
        _tourDescription.text = _currentTour.Data.SubHeader;
    }

    #region CALLBACK METHODS
    void OnMilestoneChanged(MilestoneMapping mapping)
    {
        _currentTour = _tourManager.CurrentTour;
        UpdateTourInfo();
    }

    void OnTourEnded(Tour tour)
    {
        _tourArea.style.display = DisplayStyle.None;
        _onTourEndVisual.style.display = DisplayStyle.Flex;
    }
    #endregion
}