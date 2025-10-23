using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class NameVisual : MonoBehaviour
{
    UIDocument _uiDocument;
    Label _nameLabel;
    [SerializeField] string _cachedName;

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

        // No cached name: use the game object's parent name
        if (string.IsNullOrEmpty(_cachedName))
        {
            _cachedName = gameObject.transform.parent != null ?
                                gameObject.transform.parent.name :
                                "Unnamed";
        }

        _nameLabel.text = _cachedName;
    }
}
