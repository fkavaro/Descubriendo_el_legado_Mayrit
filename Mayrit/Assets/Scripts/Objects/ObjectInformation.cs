using UnityEngine;

[CreateAssetMenu(fileName = "ObjectInformation", menuName = "Scriptable Objects/ObjectInformation")]
public class ObjectInformation : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string objectName;
    [TextArea(5, 10)]
    [SerializeField] private string description;

    [Header("Image Information")]
    [SerializeField] private Sprite image;
    [SerializeField] private string imageCaption;

    // Public properties for read-only access
    public string Name => objectName;
    public string Description => description;
    public Sprite Image => image;
    public string ImageCaption => imageCaption;
}
