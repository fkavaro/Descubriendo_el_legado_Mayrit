using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ASelection_TutorialStepConditionSO", menuName = "Scriptable Objects/Tutorial Conditions/Selection")]
public class ContextualPanelShownConditionSO : ATutorialStepConditionSO
{
    [SerializeField] List<DataSO.DataType> _dataTypes = new();
    UIManager _uiManager;

    public void SetUIManager(UIManager uiManager)
    {
        _uiManager = uiManager;
    }

    public override void BeginListening()
    {
        _uiManager.StateChangedEvent += OnUIStateChanged;
    }

    public override void EndListening()
    {
        _uiManager.StateChangedEvent -= OnUIStateChanged;
    }

    void OnUIStateChanged()
    {
        if (!_uiManager.IsInContextualPanelState) return;

        DataSO data = _uiManager.ContextualPanelState.DataToShow;

        if (_dataTypes.Contains(data.Type))
        {
            EndListening();
            MarkCompleted();
        }
    }
}
