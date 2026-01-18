using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ContextualPanel
{
    #region PROPERTIES
    public event Action PlayCharacterClickedEvent;
    public event Action ShownEvent;
    public event Action ClosedEvent;

    readonly Label _header,
        _subHeader,
        _description,
        _imageCaption;

    readonly Button _closeButton,
        _playCharacterButton;

    readonly VisualElement _root,
        _icon,
        _image;

    // Dependency Injection
    SoundManager _soundManager;

    // Tracking flags
    bool _hadIcon;
    bool _hadImage;
    bool _hadPlayButton;
    #endregion

    #region CONSTRUCTOR
    public ContextualPanel(VisualElement contextualPanelRoot)
    {
        _root = contextualPanelRoot;

        _header = _root.Q<Label>("Header");
        _subHeader = _root.Q<Label>("SubHeader");
        _description = _root.Q<Label>("Description");
        _closeButton = _root.Q<Button>("CloseButton");
        _icon = _root.Q<VisualElement>("Icon");
        _image = _root.Q<VisualElement>("Image");
        _imageCaption = _root.Q<Label>("Caption");
        _playCharacterButton = _root.Q<Button>("PlayCharacterButton");

        if (_header == null)
            Debug.LogWarning("_header not found");
        if (_subHeader == null)
            Debug.LogWarning("_subHeader not found");
        if (_description == null)
            Debug.LogWarning("_description not found");
        if (_closeButton == null)
            Debug.LogWarning("_closeButton button not found");
        if (_image == null)
            Debug.LogWarning("_image not found");
        if (_icon == null)
            Debug.LogWarning("_icon not found");
        if (_imageCaption == null)
            Debug.LogWarning("_imageCaption not found");
        if (_playCharacterButton == null)
            Debug.LogWarning("_playCharacterButton button not found");

        _closeButton.RegisterCallback<ClickEvent>(OnCloseButton);
        _playCharacterButton.RegisterCallback<ClickEvent>(OnPlayCharacter);

        // Get SoundManager dependency from Service Locator
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
    }
    #endregion

    #region PUBLIC METHODS
    public void ShowInfo(DataSO data, bool isCharacterData)
    {
        // Clear previous content
        _header.text = data.Header;
        _subHeader.text = data.SubHeader;
        _description.text = data.Description;

        // Data has icon
        if (data.Icon != null)
        {
            // Show it
            _icon.style.backgroundImage = new StyleBackground(data.Icon.texture);
            if (!_hadIcon)
            {
                _icon.style.display = DisplayStyle.Flex;
                _hadIcon = true;
            }
        }
        else
        {
            _icon.style.backgroundImage = new StyleBackground();
            _icon.style.display = DisplayStyle.None;
            _hadIcon = false;
        }

        // Handle image
        if (data.Image != null)
        {
            _image.style.backgroundImage = new StyleBackground(data.Image.texture);
            _imageCaption.text = data.ImageCaption;
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
            _hadImage = false;
        }

        // Handle play button
        if (isCharacterData)
        {
            if (!_hadPlayButton)
            {
                _playCharacterButton.style.display = DisplayStyle.Flex;
                _hadPlayButton = true;
            }
        }
        else
        {
            _playCharacterButton.style.display = DisplayStyle.None;
            _hadPlayButton = false;
        }

        _root.style.display = DisplayStyle.Flex; // Show

        ShownEvent?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS
    void Hide()
    {
        _root.style.display = DisplayStyle.None; // Hide

        // Only clear if something was shown
        if (_hadIcon)
        {
            _icon.style.backgroundImage = new StyleBackground();
            _icon.style.display = DisplayStyle.None;
            _hadIcon = false;
        }

        if (_hadImage)
        {
            _image.style.backgroundImage = new StyleBackground();
            _image.style.display = DisplayStyle.None;
            _imageCaption.style.display = DisplayStyle.None;
            _hadImage = false;
        }

        if (_hadPlayButton)
        {
            _playCharacterButton.style.display = DisplayStyle.None;
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
        Hide();
        ClosedEvent?.Invoke();

        if (_soundManager == null)
            _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        _soundManager.PlayButtonClickSFX();
    }

    void OnPlayCharacter(ClickEvent evt)
    {
        Hide();
        PlayCharacterClickedEvent?.Invoke();
    }
    #endregion
}
