using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputActionConditionSO", menuName = "Scriptable Objects/Tutorial Conditions/Input Action")]
public class InputActionConditionSO : ATutorialStepConditionSO
{
    [SerializeField] InputActionReference _action;
    [SerializeField, Min(0f)] float _delaySeconds = 5f;

    bool _enabledByThisCondition;
    bool _completionPending;
    float _pendingTime;

    public override void BeginListening()
    {
        if (_action == null || _action.action == null)
        {
            Debug.LogWarning("InputActionConditionSO has no action assigned.");
            return;
        }

        _completionPending = false;
        _pendingTime = 0f;

        if (!_action.action.enabled)
        {
            _action.action.Enable();
            _enabledByThisCondition = true;
        }

        _action.action.performed += OnPerformed;
    }

    public override void EndListening()
    {
        if (_action?.action != null)
        {
            _action.action.performed -= OnPerformed;
            if (_enabledByThisCondition) _action.action.Disable();
        }

        _enabledByThisCondition = false;
        _completionPending = false;
        _pendingTime = 0f;
    }

    public override void Tick(float deltaTime)
    {
        if (!_completionPending) return;

        _pendingTime += deltaTime;
        if (_pendingTime >= _delaySeconds)
        {
            EndListening();
            MarkCompleted();
        }
    }

    void OnPerformed(InputAction.CallbackContext ctx)
    {
        if (_delaySeconds <= 0f)
        {
            EndListening();
            MarkCompleted();
            return;
        }

        _completionPending = true;
        _pendingTime = 0f;

        // prevent multiple triggers while waiting
        _action.action.performed -= OnPerformed;

        MarkTriggered();
    }
}