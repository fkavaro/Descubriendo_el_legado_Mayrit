using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHUD_UIState : AHUDState
{
    #region  PROPERTIES
    public bool _showTourEnd;
    Tour _currentTour;

    Button _pauseButton;
    VisualElement _tourArea,
        _onTourEndVisual;
    Label _tourName,
        _tourDescription;
    #endregion

    #region CONSTRUCTOR
    public PlayerHUD_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("PlayerHUD", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region UI STATE INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        base.ConfigureUIElementsOnAwake();

        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
        _tourArea = GetByName<VisualElement>("TourArea");
        _tourName = GetByName<Label>("Name", _tourArea);
        _tourDescription = GetByName<Label>("Description", _tourArea);
        _onTourEndVisual = GetByName<VisualElement>("OnTourEnd");
    }

    protected override void GetServicesDependenciesOnStart()
    {
        base.GetServicesDependenciesOnStart();

        _currentTour = ServiceLocator.Instance.Get<Tour>();

        if (_currentTour == null)
            Debug.LogWarning("PlayerHUD_UIState: No Tour found in ServiceLocator on StartState");
    }

    public override void StartState()
    {
        base.StartState();

        ShowTourEndVisual(_showTourEnd);
        _compass.IsNextPOIShown(true);
    }

    public override void ExitState()
    {
        base.ExitState();

        // Unlock cursor and make it visible (has been lock in 3rd person camera state start)
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        _compass.IsNextPOIShown(false);
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
        ShowTourEndVisual(_currentTour.IsCompleted);
    }
    #endregion

    #region PUBLIC METHODS
    void ShowTourEndVisual(bool show)
    {
        if (show)
        {
            _tourArea.style.display = DisplayStyle.None;
            _onTourEndVisual.style.display = DisplayStyle.Flex;
        }
        else
        {
            _onTourEndVisual.style.display = DisplayStyle.None;
            _tourArea.style.display = DisplayStyle.Flex;
            _tourName.text = _currentTour.Data.Header;
            _tourDescription.text = _currentTour.Data.SubHeader;
        }
    }
    #endregion
}