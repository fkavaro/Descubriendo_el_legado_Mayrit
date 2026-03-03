using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ClickButtonConditionSO", menuName = "Scriptable Objects/Tutorial Conditions/Click Button")]
public class ClickButtonConditionSO : ATutorialStepConditionSO
{
    [SerializeField] string _buttonName;
    UIDocument _uiDocument;
    Button _button;

    public override void SetUIDocument(UIDocument uiDocument)
    {
        _uiDocument = uiDocument;
    }

    public override void BeginListening()
    {
        if (_uiDocument == null)
        {
            Debug.LogWarning("ClickButtonConditionSO has no UIDocument assigned.");
            return;
        }

        _button = _uiDocument.rootVisualElement.Q<Button>(_buttonName);
        if (_button == null)
        {
            Debug.LogWarning($"Button '{_buttonName}' not found for tutorial condition.");
            return;
        }

        _button.clicked += OnClicked;
    }

    public override void EndListening()
    {
        if (_button != null) _button.clicked -= OnClicked;
        _button = null;
    }

    void OnClicked()
    {
        EndListening();
        MarkCompleted();
    }
}