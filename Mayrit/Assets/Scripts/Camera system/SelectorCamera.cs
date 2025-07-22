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
    GameInputActions _inputActions;
    GameObject _currentSelected = null,
        _currentHover = null;
    Vector2 _mousePosition;
    Ray _cameraRay;
    #endregion

    #region MONOBEHAVIOUR
    // When script is enabled
    void OnEnable()
    {
        _inputActions.Camera.Enable();
    }

    // When script is disabled or destroyed
    void OnDisable()
    {
        _inputActions.Camera.Disable();
    }

    void Awake()
    {
        _inputActions = new();
        _inputActions.Camera.Enable();
        _inputActions.Camera.Select.performed += OnSelectObject;
    }

    void Start()
    {

    }

    void Update()
    {
        // Get the current mouse position
        _mousePosition = Mouse.current.position.ReadValue();

        // Create a ray from the camera through the mouse position
        _cameraRay = Camera.main.ScreenPointToRay(_mousePosition);

        // Move tooltip with the cursor if it's not over an UI element
        UpdateTooltip();
    }
    #endregion

    #region PUBLIC METHODS
    /// <returns>The currently selected GameObject, or null if none is selected.</returns>
    public GameObject GetCurrentSelection()
    {
        return _currentSelected;
    }
    /// <returns>The currently hovered GameObject, or null if none is hovered.</returns>
    public GameObject GetCurrentHover()
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
        if (UIManager.Instance.hudState.IsCursorOverUI(_mousePosition))
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
                _currentSelected = hit.collider.gameObject;
                ApplySelection(_currentSelected); // Display the information panel of the selected object
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
    void ApplySelection(GameObject selectedHover)
    {
        selectedHover.transform.localScale *= 2;
    }

    /// <summary>
    /// Resets the current selection.
    /// </summary>
    void ResetSelection()
    {
        if (_currentSelected == null) return;

        _currentSelected.transform.localScale /= 2;
        _currentSelected = null;
    }

    /// <summary>
    /// Move tooltip with the cursor if it's not over an UI element.
    /// </summary>
    private void UpdateTooltip()
    {
        // Cursor over UI element
        if (UIManager.Instance.hudState.IsCursorOverUI(_mousePosition))
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
                _currentHover = hit.collider.gameObject;
                ApplyHover(_currentHover); // Show a small tooltip with the object's name
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
    void ApplyHover(GameObject hoverObject)
    {
        UIManager.Instance.hudState.PlaceTooltip(hoverObject);
    }

    /// <summary>
    /// Resets the current hover.
    /// </summary>
    void ResetHover()
    {
        if (_currentHover == null) return;

        _currentHover = null;
        UIManager.Instance.hudState.HideTooltip();
    }
    #endregion
}
