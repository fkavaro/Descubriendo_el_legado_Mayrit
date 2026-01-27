using UnityEngine;

[CreateAssetMenu(fileName = "DataSO", menuName = "Scriptable Objects/Data")]
public class DataSO : ScriptableObject
{
    [Header("General information")]
    [SerializeField] string _header;
    [SerializeField] string _subHeader;

    [TextArea(5, 10)]
    [SerializeField] string _description;

    [Header("Image information")]
    [SerializeField] Sprite _icon;
    [SerializeField] Sprite _image;
    [TextArea(3, 10)]
    [SerializeField] string _imageCaption;

    // PROPERTY HELPERS
    public string Header => _header;
    public string SubHeader => _subHeader;
    public string Description => _description;
    public Sprite Icon => _icon;
    public Sprite Image => _image;
    public string ImageCaption => _imageCaption;
}
