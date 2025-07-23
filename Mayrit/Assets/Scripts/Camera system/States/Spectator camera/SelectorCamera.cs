using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectorCamera : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    [Header("Object selection")]
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask _selectableLayer;
    #endregion

    #region PRIVATE PROPERTIES

    SelectableObject _currentSelected = null,
        _currentHover = null;
    Vector2 _cursorScreenPos;
    Ray _cameraRay;
    #endregion

    #region MONOBEHAVIOUR
    void Start()
    {
        GameManager.Instance._inputActions.Camera.Select.performed += OnSelectObject;
    }

    void Update()
    {
        // Get the current mouse position
        _cursorScreenPos = Mouse.current.position.ReadValue();

        // Create a ray from the camera through the mouse position
        _cameraRay = Camera.main.ScreenPointToRay(_cursorScreenPos);

        // Move tooltip with the cursor if it's not over an UI element
        UpdateTooltip();
    }
    #endregion

    #region PUBLIC METHODS
    /// <returns>The currently selected GameObject, or null if none is selected.</returns>
    public SelectableObject GetCurrentSelection()
    {
        return _currentSelected;
    }
    /// <returns>The currently hovered GameObject, or null if none is hovered.</returns>
    public SelectableObject GetCurrentHover()
    {
        return _currentHover;
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// This method is called when the 'SelectObject' input action is performed.
    /// It handles the raycast logic for object selection.
    /// </summary>
    /// <param name="context">The context of the input action callback.</param>
    void OnSelectObject(InputAction.CallbackContext context)
    {
        // Cursor over UI element
        if (UIManager.Instance._hudState.IsCursorOverUI(_cursorScreenPos))
        {
            ResetSelection();
            return;
        }

        Debug.DrawRay(_cameraRay.origin, _cameraRay.direction * 100, Color.green, 120f);

        // Ray has collided with a selectable object
        if (Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, _selectableLayer))
        {
            // If the ray hits a new object (different from the current selection)
            if (hit.collider.gameObject != _currentSelected)
            {
                ResetSelection();

                if (!hit.collider.gameObject.TryGetComponent<SelectableObject>(out var selectableObject))
                {
                    Debug.LogWarning("Selectable component not found.");
                    return;
                }

                _currentSelected = selectableObject;
                ApplySelection(); // Display the information panel of the selected object
            }
        }
        else
        {
            ResetSelection();
        }
    }

    /// <summary>
    /// Display the information panel of the selected object.
    /// </summary>
    void ApplySelection()
    {
        //_currentSelected.transform.localScale *= 2;
        UIManager.Instance._hudState.ShowHeritagePanel(_currentSelected);
    }

    /// <summary>
    /// Resets the current selection.
    /// </summary>
    void ResetSelection()
    {
        if (_currentSelected == null) return;

        //_currentSelected.transform.localScale /= 2;
        UIManager.Instance._hudState.HideHeritagePanel();
        _currentSelected = null;
    }

    /// <summary>
    /// Move tooltip with the cursor if it's not over an UI element.
    /// </summary>
    private void UpdateTooltip()
    {
        // Cursor over UI element
        if (UIManager.Instance._hudState.IsCursorOverUI(_cursorScreenPos))
        {
            ResetHover();
            return;
        }

        // Ray has collided with a selectable object
        if (Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, _selectableLayer))
        {
            // If the ray hits an object and it's different from the currently hovered one
            if (hit.collider.gameObject != _currentHover && hit.collider.gameObject != _currentSelected)
            {
                ResetHover();
                if (hit.collider.gameObject.TryGetComponent<SelectableObject>(out var selectableObject))
                {
                    _currentHover = selectableObject;
                    ApplyHover(); // Show a small tooltip with the object's name
                }
            }
        }
        else
        {
            // If the ray hits nothing, and there was a previously hovered object, reset its state
            ResetHover();
        }
    }

    /// <summary>
    /// Show a small tooltip with the object's name.
    /// </summary>
    void ApplyHover()
    {
        UIManager.Instance._hudState.ShowTooltip(_currentHover);
    }

    /// <summary>
    /// Resets the current hover.
    /// </summary>
    void ResetHover()
    {
        if (_currentHover == null) return;

        _currentHover = null;
        UIManager.Instance._hudState.HideTooltip();
    }
    #endregion
}
