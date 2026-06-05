using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ContextualPanel_UIState : AUIState
{
    #region PROPERTIES
    public event Action PlayTourClickedEvent;
    public event Action ResetTourClickedEvent;
    public event Action ClosedEvent;

    public DataSO DataToShow;

    Label _header,
        _subHeader,
        _description,
        _imageCaption;

    Button _closeButton,
        _startTourButton,
        _resetTourButton,
        _pauseButton;

    VisualElement _image;

    Tour CurrentTour => ServiceLocator.Instance.Get<Tour>();

    // Tracking flags
    bool _hadImage;
    bool _hadPlayButton;
    #endregion

    #region CONSTRUCTOR
    public ContextualPanel_UIState(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("ContextualPanel", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _header = GetByName<Label>("Header");
        _subHeader = GetByName<Label>("SubHeader");
        _description = GetByName<Label>("Description");
        _imageCaption = GetByName<Label>("Caption");
        _closeButton = GetButtonAndRegisterCallback("CloseContextualPanelButton", OnCloseButton);
        _image = GetByName<VisualElement>("Image");
        _startTourButton = GetButtonAndRegisterCallback("StartTourButton", OnStartTour);
        _resetTourButton = GetButtonAndRegisterCallback("ResetTourButton", OnResetTour);
        _pauseButton = GetButtonAndRegisterCallback("PauseButton", OnPauseClicked);
    }

    public override void StartState()
    {
        base.StartState();

        _gameManager.InputActions.UI.CloseContextualPanel.performed += OnCloseAction;

        if (DataToShow == null)
        {
            Debug.LogWarning("[ContextualPanel] No data to show!");
            return;
        }

        _header.text = DataToShow.Header;
        _subHeader.text = DataToShow.SubHeader;
        _description.text = DataToShow.Description;

        if (DataToShow.Image != null)
        {
            _image.style.backgroundImage = new StyleBackground(DataToShow.Image.texture);
            _imageCaption.text = DataToShow.ImageCaption;
            if (!_hadImage)
            {
                _image.style.display = DisplayStyle.Flex;
                _imageCaption.style.display = DisplayStyle.Flex;
                _hadImage = true;
            }
        }
        else
        {
            _image.style.backgroundImage = new StyleBackground();
            _image.style.display = DisplayStyle.None;
            _imageCaption.style.display = DisplayStyle.None;
            _imageCaption.text = string.Empty;
            _hadImage = false;
        }

        if (DataToShow.IsPlayer)
        {
            if (!_hadPlayButton)
            {
                _startTourButton.style.display = DisplayStyle.Flex;
                _hadPlayButton = true;

                // Only show reset button if tour is already completed
                if (CurrentTour.IsCompleted)
                    _resetTourButton.style.display = DisplayStyle.Flex;
                else
                    _resetTourButton.style.display = DisplayStyle.None;
            }
        }
        else
        {
            _startTourButton.style.display = DisplayStyle.None;
            _resetTourButton.style.display = DisplayStyle.None;
            _hadPlayButton = false;
        }
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.UI.CloseContextualPanel.performed -= OnCloseAction;
    }
    #endregion

    #region PRIVATE METHODS
    void ClearContextualPanel()
    {
        if (_hadImage)
        {
            _image.style.backgroundImage = new StyleBackground();
            _image.style.display = DisplayStyle.None;
            _imageCaption.style.display = DisplayStyle.None;
            _hadImage = false;
        }

        if (_hadPlayButton)
        {
            _startTourButton.style.display = DisplayStyle.None;
            _hadPlayButton = false;
        }

        _header.text = string.Empty;
        _subHeader.text = string.Empty;
        _description.text = string.Empty;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCloseButton(ClickEvent evt)
    {
        ClosedEvent?.Invoke();
        _soundManager.PlayButtonClickSFX();
        ClearContextualPanel();
    }

    void OnCloseAction(InputAction.CallbackContext context)
    {
        ClosedEvent?.Invoke();
        _soundManager.PlayButtonClickSFX();
        ClearContextualPanel();
    }

    void OnStartTour(ClickEvent evt)
    {
        PlayTourClickedEvent?.Invoke();
        ClearContextualPanel();
    }

    void OnResetTour(ClickEvent evt)
    {
        ResetTourClickedEvent?.Invoke();
        ClearContextualPanel();
    }

    void OnPauseClicked(ClickEvent evt)
    {
        _uiManager.SwitchToPauseState();
        _soundManager.PlayButtonClickSFX();
    }
    #endregion
}
