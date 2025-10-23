using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class NameVisual : MonoBehaviour
{
    UIDocument uiDocument;
    Label nameLabel;
    [SerializeField] string cachedName;

    void OnEnable()
    {
        // Try to get the UIDocument component from the same GameObject
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Try to find a Label with name 'Name' in the document
        nameLabel = root.Q<Label>(name: "Name");
        if (nameLabel == null)
        {
            Debug.LogWarning("NameVisual: No Label with name 'Name' was found in the UIDocument.");
            return;
        }

        // No cached name: use the game object's parent name
        if (string.IsNullOrEmpty(cachedName))
        {
            cachedName = gameObject.transform.parent != null ?
                                gameObject.transform.parent.name :
                                "Unnamed";
        }

        nameLabel.text = cachedName;
    }
}
