using UnityEngine;

// TODO: improve
public class SelectableObject : MonoBehaviour
{
    public AInformationSO Data => _data;
    [SerializeField] AInformationSO _data;
}
