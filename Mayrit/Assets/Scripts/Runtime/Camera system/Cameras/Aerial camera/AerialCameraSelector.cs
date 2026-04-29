using System;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: remove later
// public class AerialCameraSelector
// {
//     #region EVENTS
//     public event Action<SelectableObject> ObjectSelectedEvent;
//     #endregion

//     #region PROPERTIES
//     // Configuration
//     readonly LayerMask _selectableLayer;

//     // Selection state
//     SelectableObject _currentSelected;
//     SelectableObject _currentHover;

//     // Input state
//     Vector2 _cursorScreenPos;
//     Ray _cameraRay;
//     bool _isSelectPressed;

//     // Dependencies
//     readonly GameManager _gameManager;
//     readonly UIManager _uiManager;
//     #endregion

//     #region CONSTRUCTOR
//     public AerialCameraSelector(AerialCameraData AerialCameraData)
//     {
//         _selectableLayer = aerialCameraData.selectableLayer;

//         // Get dependencies from ServiceLocator
//         _gameManager = ServiceLocator.Instance.Get<GameManager>();
//         _uiManager = ServiceLocator.Instance.Get<UIManager>();
//     }
//     #endregion

//     #region LIFE CYCLE
//     public void Update()
//     {
//         // Update input state
//         _isSelectPressed = _gameManager.InputActions.Camera.Select.IsPressed();
//         _cursorScreenPos = Mouse.current.position.ReadValue();
//         _cameraRay = Camera.main.ScreenPointToRay(_cursorScreenPos);

//         // Update hover tooltip
//         UpdateTooltip();

//         // Handle selection
//         if (_isSelectPressed)
//             SelectObject();
//     }
//     #endregion

//     #region SELECTION METHODS
//     /// <summary>
//     /// Handles object selection via raycast when the select input is pressed.
//     /// Toggles selection off if clicking the same object or empty space.
//     /// </summary>
//     void SelectObject()
//     {
//         // Ignore input when cursor is over UI
//         if (_uiManager.IsCursorOverUI)
//             return;

//         // Perform raycast to find selectable objects
//         if (!Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, _selectableLayer))
//         {
//             // Clicked empty space - deselect
//             ResetSelection();
//             return;
//         }

//         // Get the SelectableObject component from the hit object
//         if (!hit.collider.TryGetComponent<SelectableObject>(out var selectableObject))
//             return;

//         // Check if clicking the same object (toggle off)
//         if (selectableObject == _currentSelected)
//         {
//             ResetSelection();
//             return;
//         }

//         // Select the new object
//         ResetSelection();
//         _currentSelected = selectableObject;
//         ApplySelection();
//     }

//     /// <summary>
//     /// Applies the current selection by invoking the selection event and resetting hover state.
//     /// </summary>
//     void ApplySelection()
//     {
//         if (_currentSelected == null)
//             return;

//         ObjectSelectedEvent?.Invoke(_currentSelected);
//         ResetHover();
//     }

//     /// <summary>
//     /// Clears the current selection state.
//     /// </summary>
//     void ResetSelection()
//     {
//         _currentSelected = null;
//     }
//     #endregion

//     #region TOOLTIP METHODS
//     /// <summary>
//     /// Updates the hover tooltip based on the object under the cursor.
//     /// </summary>
//     void UpdateTooltip()
//     {
//         // Hide tooltip when cursor is over UI
//         if (_uiManager.IsCursorOverUI)
//         {
//             ResetHover();
//             return;
//         }

//         // Check for selectable objects under cursor
//         if (!Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, _selectableLayer))
//         {
//             // Nothing under cursor - clear hover
//             ResetHover();
//             return;
//         }

//         // Get the selectable component
//         if (!hit.collider.TryGetComponent<SelectableObject>(out var selectableObject))
//             return;

//         if (_currentHover != selectableObject)
//             _currentHover = selectableObject;
//         ApplyHover();
//     }

//     /// <summary>
//     /// Displays the tooltip for the currently hovered object.
//     /// </summary>
//     void ApplyHover()
//     {
//         if (_currentHover != null)
//             _uiManager.ShowTooltip(_currentHover.Data);
//     }

//     /// <summary>
//     /// Clears the hover state and hides the tooltip.
//     /// </summary>
//     void ResetHover()
//     {
//         if (_currentHover == null)
//             return;

//         _currentHover = null;
//         _uiManager.HideTooltip();
//     }
//     #endregion
// }
