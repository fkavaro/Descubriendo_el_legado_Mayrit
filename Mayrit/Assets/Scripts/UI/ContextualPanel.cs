using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ContextualPanel
{
    public event Action OnClosePanel;

    #region PRIVATE PROPERTIES
    readonly Label _header,
        _subHeader,
        _description,
        _imageCaption;

    readonly Button _closeButton,
        _playCharacterButton;

    readonly VisualElement _root,
        _icon,
        _image;
    #endregion

    // Constructor
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

        _closeButton.RegisterCallback<ClickEvent>(CloseContextualPanel);
        _playCharacterButton.RegisterCallback<ClickEvent>(PlayCharacter);
    }

    #region PUBLIC METHODS
    public void ShowInfo(AInformationSO objectInfo)
    {
        Reset();

        // Overwrite panel information
        _header.text = objectInfo.Header;
        _subHeader.text = objectInfo.SubHeader;
        _description.text = objectInfo.Description;

        // Theres is an icon
        if (objectInfo.Icon != null)
        {
            _icon.style.backgroundImage = new StyleBackground(objectInfo.Icon.texture);
            _icon.style.display = DisplayStyle.Flex;
        }

        // There is an image
        if (objectInfo.Image != null)
        {
            _image.style.backgroundImage = new StyleBackground(objectInfo.Image.texture);
            _image.style.display = DisplayStyle.Flex;
            _imageCaption.text = objectInfo.ImageCaption;
            _imageCaption.style.display = DisplayStyle.Flex;
        }

        // If the information type is Character, show the play button
        if (objectInfo is Character_InformationSO)
            _playCharacterButton.style.display = DisplayStyle.Flex;

        // Show panel
        _root.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        // Hide panel
        _root.style.display = DisplayStyle.None;

        Reset();

        OnClosePanel?.Invoke();
    }
    #endregion

    #region PRIVATE METHODS
    void CloseContextualPanel(ClickEvent evt)
    {
        Hide();
    }

    void PlayCharacter(ClickEvent evt)
    {
        CameraManager.Instance.SwitchToThirdPersonCamera();
    }

    void Reset()
    {
        _header.text = string.Empty;
        _subHeader.text = string.Empty;
        _description.text = string.Empty;
        _icon.style.backgroundImage = null;
        _icon.style.display = DisplayStyle.None;
        _image.style.backgroundImage = null;
        _image.style.display = DisplayStyle.None;
        _imageCaption.style.display = DisplayStyle.None;
        _playCharacterButton.style.display = DisplayStyle.None;
    }
    #endregion
}
