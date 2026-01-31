using UnityEngine;

[CreateAssetMenu(fileName = "Milestone_DataSO", menuName = "Scriptable Objects/MilestoneData")]
public class Milestone_DataSO : DataSO
{
    [Header("Milestone specific data")]

    // [SerializeField] GameObject _playableCharacter;
    // [SerializeField] GameObject _tour;
    [SerializeField] SceneDatabase.SceneName _sceneName;
    [SerializeField] float _wantedTime;
    [SerializeField] int _milestoneIndex;

    // public GameObject PlayableCharacter => _playableCharacter;
    // public GameObject Tour => _tour;
    public SceneDatabase.SceneName SceneName => _sceneName;
    public float WantedTime => _wantedTime;
    public int Index => _milestoneIndex;
}
