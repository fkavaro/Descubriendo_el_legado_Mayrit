using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class LandmarkVisual : MonoBehaviour
{
    UIDocument _uiDocument;
    Label _nameLabel;
    Button _nameButton;

    public DataSO _information;

    UIManager _uiManager;

    void Awake()
    {
        // Get dependency from Service Locator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        // Validate dependency
        if (_uiManager == null)
            Debug.LogError("LandmarkVisual: UIManager not found in ServiceLocator!");
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
        // Open contextual panel with landmark information
        _uiManager.ShowContextualPanel(_information);
    }
}
