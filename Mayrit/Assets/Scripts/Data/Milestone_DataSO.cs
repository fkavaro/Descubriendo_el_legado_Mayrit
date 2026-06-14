using UnityEngine;

[CreateAssetMenu(fileName = "Milestone_DataSO", menuName = "Scriptable Objects/MilestoneData")]
public class Milestone_DataSO : DataSO
{
    [Header("Milestone specific data")]
    [SerializeField] SceneDatabase.SceneName _sceneName;
    [SerializeField] int _milestoneIndex;
    [SerializeField] Sprite _bgImage;
    [TextArea(3, 10)]
    [SerializeField] string _bgImageCaption;

    public SceneDatabase.SceneName SceneName => _sceneName;
    public int MilestoneIndex => _milestoneIndex;
    public Sprite BgImage => _bgImage;
}
