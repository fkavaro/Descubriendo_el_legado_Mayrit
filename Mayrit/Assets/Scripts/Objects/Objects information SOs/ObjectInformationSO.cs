using UnityEngine;

[CreateAssetMenu(fileName = "ObjectInformationSO", menuName = "Scriptable Objects/ObjectInformationSO")]
public class ObjectInformationSO : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string _name;
    [TextArea(5, 10)]
    [SerializeField] private string _description;

    [Header("Image Information")]
    [SerializeField] private Sprite _image;
    [SerializeField] private string _imageCaption;

    // Public properties for read-only access
    public string Name => _name;
    public string Description => _description;
    public Sprite Image => _image;
    public string ImageCaption => _imageCaption;
}
