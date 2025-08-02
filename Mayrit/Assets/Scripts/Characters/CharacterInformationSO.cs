using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInformationSO", menuName = "Scriptable Objects/CharacterInformationSO")]
public class CharacterInformationSO : ScriptableObject
{
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
