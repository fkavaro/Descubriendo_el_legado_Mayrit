using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LandmarkVisual : MonoBehaviour
{
    UIDocument _uiDocument;
    Label _nameLabel;
    Button _nameButton;

    public DataSO _information;

    // Dependency Injection
    UIManager _uiManager;
    SoundManager _soundManager;
    CameraManager _cameraManager;

    void Awake()
    {
        // Get dependency from Service Locator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();

        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
    }


    void OnEnable()
    {
        // Try to get the UIDocument component from the same GameObject
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // Try to find a Label with name 'Name' in the document
        _nameLabel = root.Q<Label>(name: "Name");
        if (_nameLabel == null)
        {
            Debug.LogWarning("NameVisual: No Label with name 'Name' was found in the UIDocument.");
            return;
        }

        _nameLabel.text = _information.Header;

        // Try to find a Button with name 'NameButton' in the document
        _nameButton = root.Q<Button>(name: "NameButton");
        if (_nameButton == null)
        {
            Debug.LogWarning("LandmarkVisual: No Button with name 'NameButton' was found in the UIDocument.");
            return;
        }

        // Register click event
        _nameButton.RegisterCallback<ClickEvent>(OnNameButtonClick);
    }

    void OnNameButtonClick(ClickEvent evt)
    {
        _uiManager.ShowContextualPanel(_information);
        _soundManager.PlayButtonClickSFX();
    }

    void OnCameraStateChanged()
    {
        if (_cameraManager.IsInSpectatorState)
        {
            _nameButton.visible = true;
        }
        else
            _nameButton.visible = false;
    }
}
