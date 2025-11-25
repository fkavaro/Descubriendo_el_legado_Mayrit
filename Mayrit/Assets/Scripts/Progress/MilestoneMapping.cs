using UnityEngine;

[System.Serializable]
public class MilestoneMapping
{
    [SerializeField] Milestone_InformationSO _milestoneData;
    [SerializeField] GameObject _playableCharacter;
    [SerializeField] Tour _tour;

    public Milestone_InformationSO Data => _milestoneData;
    public GameObject PlayableCharacter => _playableCharacter;
    public Tour Tour => _tour;
}
