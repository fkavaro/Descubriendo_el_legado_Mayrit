using UnityEngine;

[CreateAssetMenu(fileName = "DataSO", menuName = "Scriptable Objects/Data")]
public class DataSO : ScriptableObject
{
    public enum DataType
    {
        Milestone,
        Player,
        Landmark,
        ModernBuilding,
        Other
    }


    [Header("General information")]
    [SerializeField] DataType _dataType = DataType.Other;
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
    public bool IsMilestone => _dataType == DataType.Milestone;
    public bool IsPlayer => _dataType == DataType.Player;
    public bool IsLandmark => _dataType == DataType.Landmark;
    public bool IsModernBuilding => _dataType == DataType.ModernBuilding;
    public bool IsOther => _dataType == DataType.Other;
    public string Header => _header;
    public string SubHeader => _subHeader;
    public string Description => _description;
    public Sprite Icon => _icon;
    public Sprite Image => _image;
    public string ImageCaption => _imageCaption;
}
