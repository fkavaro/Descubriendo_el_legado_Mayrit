using UnityEngine;

[CreateAssetMenu(fileName = "MilestoneInformationSO", menuName = "Scriptable Objects/MilestoneInformationSO")]
public class MilestoneInformationSO : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _date;
    [TextArea(5, 10)]
    [SerializeField] private string _description;

    // Public properties for read-only access
    public string Name => _name;
    public string Date => _date;
    public string Description => _description;
}
