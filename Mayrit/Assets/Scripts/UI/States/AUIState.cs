using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public abstract class AUIState : AState<UIManager, StackFiniteStateMachine<UIManager>>
{
    public UIDocument _UIDocument;
    public VisualElement _screen;

    protected AUIState(string name, StackFiniteStateMachine<UIManager> stateMachine)
    : base(name, stateMachine) { }

    /// <summary>
    /// Returns true if the cursor is over any UI element that is a descendant of _screen.
    /// </summary>
    /// <param name="cursorPos">Screen-space position of the cursor (Input.mousePosition).</param>
    public virtual bool IsCursorOverUI()
    {
        if (_UIDocument == null || _screen == null)
            return false;

        // Get the current mouse position
        Vector2 cursorPos = Mouse.current.position.ReadValue();

        // Convert to UI Toolkit coordinates (Y is flipped)
        Vector2 panelPosition = new(cursorPos.x, Screen.height - cursorPos.y);

        // Pick the topmost VisualElement at the given position
        var panel = _UIDocument.rootVisualElement.panel;
        VisualElement pickedElement = panel?.Pick(panelPosition);

        // Check if the picked element is a descendant of _screen (and not _screen itself)
        if (pickedElement != null && pickedElement != _screen && _screen.Contains(pickedElement))
        {
            //Debug.Log("Cursor over " + pickedElement.name);
            return true;
        }

        return false;
    }
}
