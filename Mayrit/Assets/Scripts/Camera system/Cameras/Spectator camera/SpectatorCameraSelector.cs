using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorCameraSelector
{
    #region PUBLIC PROPERTIES
    public event Action<SelectableObject> ObjectSelectedEvent;
    public LayerMask _selectableLayer;
    #endregion

    #region PRIVATE PROPERTIES
    SelectableObject _currentSelected = null,
        _currentHover = null;
    Vector2 _cursorScreenPos;
    Ray _cameraRay;
    bool _isSelectPressed;

    // Dependency Injection
    readonly CameraManager _cameraManager;
    readonly GameManager _gameManager;
    readonly UIManager _uiManager;
    #endregion

    #region CONSTRUCTOR
    public SpectatorCameraSelector()
    {
        // Get dependencies from ServiceLocator
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _uiManager = ServiceLocator.Instance.Get<UIManager>();

        // Validate dependencies
        if (_cameraManager == null)
            Debug.LogError("SpectatorCameraSelector: CameraManager not found in ServiceLocator!");
        else
            _selectableLayer = _cameraManager._selectableLayer;

        if (_gameManager == null)
            Debug.LogError("SpectatorCameraSelector: GameManager not found in ServiceLocator!");

        if (_uiManager == null)
            Debug.LogError("SpectatorCameraSelector: UIManager not found in ServiceLocator!");
    }
    #endregion

    #region LIFE CYCLE
    public void Update()
    {
        _isSelectPressed = _gameManager.InputActions.Camera.Select.IsPressed();

        // Get the current mouse position
        _cursorScreenPos = Mouse.current.position.ReadValue();

        // Create a ray from the camera through the mouse position
        _cameraRay = Camera.main.ScreenPointToRay(_cursorScreenPos);

        // Move tooltip with the cursor if it's not over an UI element
        UpdateTooltip();

        if (_isSelectPressed)
            SelectObject();
    }
    #endregion

    #region SELECTION METHODS
    /// <summary>
    /// This method is called when the 'SelectObject' input action is performed.
    /// It handles the raycast logic for object selection.
    /// </summary>
    void SelectObject()
    {
        // Cursor over UI element
        if (_uiManager.IsCursorOverUI())
            return;

        // Ray has collided with a selectable object
        if (Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, _selectableLayer))
        {
            // If the ray hits a new object (different from the current selection)
            if (hit.collider.gameObject != _currentSelected)
            {
                ResetSelection();

                if (!hit.collider.gameObject.TryGetComponent<SelectableObject>(out var selectableObject))
                {
                    //Debug.LogWarning(hit.collider.gameObject.name + ": selectable component not found");
                    return;
                }

                _currentSelected = selectableObject;
                ApplySelection(); // Display the information panel of the selected object
            }
            // Hits the same
            else
                ResetSelection();
        }
        // If the ray hits nothing
        else
            ResetSelection();
    }

    /// <summary>
    /// Display the information panel of the selected object.
    /// </summary>
    void ApplySelection()
    {
        ObjectSelectedEvent?.Invoke(_currentSelected);
        ResetHover();
    }

    /// <summary>
    /// Resets the current selection.
    /// </summary>
    void ResetSelection()
    {
        if (_currentSelected == null) return;
        if (_uiManager.IsCursorOverUI()) return;

        _currentSelected = null;
    }
    #endregion

    #region TOOLTIP METHODS
    /// <summary>
    /// Move tooltip with the cursor if it's not over an UI element.
    /// </summary>
    void UpdateTooltip()
    {
        // Cursor over UI element
        if (_uiManager.IsCursorOverUI())
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
        _uiManager.ShowTooltip(_currentHover.Data);
    }

    /// <summary>
    /// Resets the current hover.
    /// </summary>
    void ResetHover()
    {
        if (_currentHover == null) return;

        _currentHover = null;
        _uiManager.HideTooltip();
    }
    #endregion
}
