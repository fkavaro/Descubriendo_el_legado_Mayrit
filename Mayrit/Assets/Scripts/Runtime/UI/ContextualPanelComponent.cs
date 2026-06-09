using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ContextualPanelComponent : AUIState
{
    #region PROPERTIES
    public event Action ContinueClickedEvent;
    public event Action ResetTourClickedEvent;
    public event Action ClosedEvent;


    Label _header,
        _subHeader,
        _disclaimer,
        _description,
        _conservation,
        _imageCaption;

    Button _closeButton,
        _continueButton,
        _resetTourButton;

    VisualElement _image,
        _disclaimerArea,
        _glosaryArea,
        _glosaryList,
        _conservationArea,
        _sourcesArea,
        _sourcesList,
        _lowerArea,
        _closeArea,
        _loadingAnimation;

    Tour CurrentTour => ServiceLocator.Instance.Get<Tour>();

    // Tracking flags
    //bool _hadImage;
    //bool _hadPlayButton;
    #endregion

    #region CONSTRUCTOR
    public ContextualPanelComponent(UIDocument uiDocument, float fadeInDuration, float fadeOutDuration)
    : base("ContextualPanel", uiDocument, fadeInDuration, fadeOutDuration) { }
    #endregion

    #region INHERITED METHODS
    protected override void ConfigureUIElementsOnAwake()
    {
        _closeArea = GetByName<VisualElement>("CloseArea");
        _closeButton = GetButtonAndRegisterCallback("CloseContextualPanelButton", OnCloseButton, _closeArea);

        _disclaimerArea = GetByName<VisualElement>("DisclaimerArea");
        _disclaimer = GetByName<Label>("Disclaimer", _disclaimerArea);

        _glosaryArea = GetByName<VisualElement>("GlosaryArea");
        _glosaryList = GetByName<VisualElement>("GlosaryList", _glosaryArea);

        _header = GetByName<Label>("Header");
        _subHeader = GetByName<Label>("SubHeader");
        _description = GetByName<Label>("Description");

        _conservationArea = GetByName<VisualElement>("ConservationArea");
        _conservation = GetByName<Label>("Conservation");

        _sourcesArea = GetByName<VisualElement>("SourcesArea");
        _sourcesList = GetByName<VisualElement>("SourcesList", _sourcesArea);

        _imageCaption = GetByName<Label>("Caption");
        _image = GetByName<VisualElement>("Image");


        _lowerArea = GetByName<VisualElement>("LowerArea");
        _continueButton = GetButtonAndRegisterCallback("ContinueButton", OnContinue, _lowerArea);
        _resetTourButton = GetButtonAndRegisterCallback("ResetTourButton", OnResetTour, _lowerArea);
        _loadingAnimation = GetByName<VisualElement>("LoadingAnimation");
    }

    public void ShowData(DataSO data)
    {
        base.StartState();

        _gameManager.InputActions.UI.CloseContextualPanel.performed += OnCloseAction;

        if (data == null)
        {
            Debug.LogWarning("[ContextualPanel] No data to show!");
            return;
        }

        _header.text = data.Header;
        _subHeader.text = data.SubHeader;
        _description.text = data.Description;

        if (data.ShowDisclaimer)
        {
            _disclaimer.text = data.Disclaimer;
            _disclaimerArea.style.display = DisplayStyle.Flex;
        }
        else
        {
            _disclaimerArea.style.display = DisplayStyle.None;
            _disclaimer.text = string.Empty;
        }

        _glosaryList.Clear();
        if (data.GlosaryDefinitions != null && data.GlosaryDefinitions.Count > 0)
        {

            foreach (var definition in data.GlosaryDefinitions)
            {
                var defElement = new Label(definition.WordWithDefinition);
                defElement.AddToClassList("text");
                _glosaryList.Add(defElement);
            }
            _glosaryArea.style.display = DisplayStyle.Flex;
        }
        else
        {
            _glosaryArea.style.display = DisplayStyle.None;
        }

        if (data.Image != null)
        {
            _image.style.backgroundImage = new StyleBackground(data.Image.texture);
            _imageCaption.text = data.ImageCaption;
            // if (!_hadImage)
            // {
            _image.style.display = DisplayStyle.Flex;
            _imageCaption.style.display = DisplayStyle.Flex;
            //_hadImage = true;
            //}
        }
        else
        {
            _image.style.backgroundImage = new StyleBackground();
            _image.style.display = DisplayStyle.None;
            _imageCaption.style.display = DisplayStyle.None;
            _imageCaption.text = string.Empty;
            //_hadImage = false;
        }

        if (data.Conservation != null && data.Conservation != string.Empty)
        {
            _conservation.text = data.Conservation;
            _conservationArea.style.display = DisplayStyle.Flex;
        }
        else
        {
            _conservationArea.style.display = DisplayStyle.None;
            _conservation.text = string.Empty;
        }

        _sourcesList.Clear();
        if (data.Sources != null && data.Sources.Count > 0)
        {
            foreach (var source in data.Sources)
            {
                var sourceElement = new Label(source);
                sourceElement.AddToClassList("text");
                _sourcesList.Add(sourceElement);
            }
            _sourcesArea.style.display = DisplayStyle.Flex;
        }
        else
        {
            _sourcesArea.style.display = DisplayStyle.None;
        }

        if (data.IsPlayer)
        {
            // if (!_hadPlayButton)
            // {
            _lowerArea.style.display = DisplayStyle.Flex;
            _continueButton.style.display = DisplayStyle.Flex;
            //_hadPlayButton = true;

            // Only show reset button if tour is already completed
            if (CurrentTour.IsCompleted)
                _resetTourButton.style.display = DisplayStyle.Flex;
            else
                _resetTourButton.style.display = DisplayStyle.None;
            //}
        }
        else
        {
            _lowerArea.style.display = DisplayStyle.None;
            _continueButton.style.display = DisplayStyle.None;
            _resetTourButton.style.display = DisplayStyle.None;
            //_hadPlayButton = false;
        }

        _closeArea.style.display = DisplayStyle.Flex;
    }

    public void ShowDataWhileLoading(DataSO data)
    {
        ShowData(data);

        _closeArea.style.display = DisplayStyle.None;
        _lowerArea.style.display = DisplayStyle.Flex;
        _loadingAnimation.style.display = DisplayStyle.Flex;
        _continueButton.style.display = DisplayStyle.None;
    }

    public void AfterLoading()
    {
        _closeArea.style.display = DisplayStyle.None;
        _lowerArea.style.display = DisplayStyle.Flex;
        _loadingAnimation.style.display = DisplayStyle.None;
        _continueButton.style.display = DisplayStyle.Flex;
    }

    public override void ExitState()
    {
        base.ExitState();

        _gameManager.InputActions.UI.CloseContextualPanel.performed -= OnCloseAction;
    }
    #endregion

    #region PUBLIC METHODS
    public void Reset()
    {
        // if (_hadImage)
        // {
        //     _image.style.backgroundImage = new StyleBackground();
        //     _image.style.display = DisplayStyle.None;
        //     _imageCaption.style.display = DisplayStyle.None;
        //     _hadImage = false;
        // }

        // if (_hadPlayButton)
        // {
        //     _continueButton.style.display = DisplayStyle.None;
        //     _hadPlayButton = false;
        // }

        _header.text = string.Empty;
        _subHeader.text = string.Empty;
        _disclaimer.text = string.Empty;
        _glosaryList.Clear();
        _description.text = string.Empty;
        _image.style.backgroundImage = new StyleBackground();
        _image.style.display = DisplayStyle.None;
        _imageCaption.style.display = DisplayStyle.None;
        _conservation.text = string.Empty;
        _sourcesList.Clear();
        _lowerArea.style.display = DisplayStyle.None;
        _loadingAnimation.style.display = DisplayStyle.None;
        _continueButton.style.display = DisplayStyle.None;
        _resetTourButton.style.display = DisplayStyle.None;
    }
    #endregion

    #region CALLBACK METHODS
    void OnCloseAction(InputAction.CallbackContext context)
    {
        OnCloseButton(null);
    }

    void OnCloseButton(ClickEvent evt)
    {
        ClosedEvent?.Invoke();
        _soundManager.PlayButtonClickSFX();
        Reset();
    }

    void OnContinue(ClickEvent evt)
    {
        ContinueClickedEvent?.Invoke();
        Reset();
    }

    void OnResetTour(ClickEvent evt)
    {
        ResetTourClickedEvent?.Invoke();
        Reset();
    }
    #endregion
}
