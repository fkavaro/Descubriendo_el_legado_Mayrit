using UnityEngine;

[CreateAssetMenu(fileName = "EventInformationSO", menuName = "Scriptable Objects/EventInformationSO")]
public class EventInformationSO : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string _name;
    [SerializeField] private string _date;
    [TextArea(5, 10)]
    [SerializeField] private string _description;

    // Public properties for read-only access
    public string Name => _name;
    public string Date => _date;
    public string Description => _description;
}
