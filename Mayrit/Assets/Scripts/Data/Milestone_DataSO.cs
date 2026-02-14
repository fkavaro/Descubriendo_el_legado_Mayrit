using UnityEngine;

[CreateAssetMenu(fileName = "Milestone_DataSO", menuName = "Scriptable Objects/MilestoneData")]
public class Milestone_DataSO : DataSO
{
    [Header("Milestone specific data")]
    [SerializeField] SceneDatabase.SceneName _sceneName;
    [SerializeField] float _wantedTime;
    [SerializeField] int _milestoneIndex;

    public SceneDatabase.SceneName SceneName => _sceneName;
    public float WantedTime => _wantedTime;
}
