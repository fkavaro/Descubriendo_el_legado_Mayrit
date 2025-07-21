using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SelectorCamera : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    [Header("Object selection")]
    [Tooltip("UIDocument component that contains HUD.")]
    public HUDController hud;
    [Tooltip("Layer mask to define which objects are selectable.")]
    public LayerMask selectableLayer;
    #endregion

    #region PRIVATE PROPERTIES
    GameInputActions inputActions;
    GameObject currentSelected = null, // Currently selected GameObject
        currentHover = null;
    Vector2 mousePosition;
    IPanel panel;
    #endregion

    #region MONOBEHAVIOUR
    void OnEnable()
    {
        // Enable the Action Map containing your 'SelectObject' action when the script is enabled
        inputActions.Camera.Enable();
    }

    void OnDisable()
    {
        // Disable the Action Map when the script is disabled or destroyed
        inputActions.Camera.Disable();
    }

    void Awake()
    {
        inputActions = new();
        inputActions.Camera.Enable();
        inputActions.Camera.Select.performed += OnSelectObject;

        if (hud != null)
        {
            panel = hud.UIDocument.rootVisualElement.panel;
            if (panel == null)
            {
                Debug.LogError("UI Toolkit Panel not found on the provided UIDocument's rootVisualElement.");
            }
        }
        else
        {
            Debug.LogError("mainUIDocument is not assigned in the Inspector for ObjectHover script! UI blocking might not work.");
        }
    }

    void Update()
    {
        // Get the current mouse position
        mousePosition = Mouse.current.position.ReadValue();

        // // There is a valid panel and the panel detects an element under the mouse
        // if (panel != null)
        // {
        //     // The Pick method returns the VisualElement under the specified point.
        //     // If it returns a non-null element, the mouse is over UI.
        //     VisualElement pickedElement = panel.Pick(mousePosition);

        //     // You might want to refine this: sometimes picking the root visual element
        //     // doesn't mean you're "over" interactive UI.
        //     // Consider checking if pickedElement != panel.visualTree.
        //     // A common heuristic is to check if it's not the root element itself.
        //     // For simple cases, just checking if pickedElement is not null is often enough.
        //     if (pickedElement != null && pickedElement != hud.UIDocument.rootVisualElement)
        //     {
        //         // The mouse is over a UI Toolkit element (that is not the root of the document itself).
        //         // Do NOT perform game object raycast.
        //         if (currentHover != null)
        //             ResetHover();

        //         return;
        //     }
        // }

        // Create a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        // Perform the raycast. It checks for colliders on objects within the specified 'hoverableLayer'.
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            //Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            // If the ray hits an object and it's different from the currently hovered one
            if (hit.collider.gameObject != currentHover && hit.collider.gameObject != currentSelected)
            {
                // If there was a previous hover object, reset its state (un-highlight it)
                ResetHover();

                // Set the newly hit object as the current hover object
                currentHover = hit.collider.gameObject;
                //Debug.Log("Hovering over: " + currentHover.name);

                // Show a small tooltip with the object's name
                ApplyHover(currentHover);
            }
        }
        else
        {
            // If the ray hits nothing, and there was a previously hovered object, reset its state
            if (currentHover != null)
                ResetHover();
        }
    }
    #endregion

    #region PUBLIC METHODS
    /// <returns>The currently selected GameObject, or null if none is selected.</returns>
    public GameObject GetCurrentSelection()
    {
        return currentSelected;
    }
    /// <returns>The currently hovered GameObject, or null if none is hovered.</returns>
    public GameObject GetCurrentHover()
    {
        return currentHover;
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
        //Debug.Log("Selection executed");

        // Get the current mouse position from the Input System
        mousePosition = Mouse.current.position.ReadValue();

        // // Cursor is currently over a UI element.
        // // There is a valid panel and the panel detects an element under the mouse
        // if (panel != null)
        // {
        //     // The Pick method returns the VisualElement under the specified point.
        //     // If it returns a non-null element, the mouse is over UI.
        //     VisualElement pickedElement = panel.Pick(mousePosition);

        //     // You might want to refine this: sometimes picking the root visual element
        //     // doesn't mean you're "over" interactive UI.
        //     // Consider checking if pickedElement != panel.visualTree.
        //     // A common heuristic is to check if it's not the root element itself.
        //     // For simple cases, just checking if pickedElement is not null is often enough.
        //     if (pickedElement != null && pickedElement != hud.UIDocument.rootVisualElement)
        //     {
        //         // The mouse is over a UI Toolkit element (that is not the root of the document itself).
        //         // Do NOT perform game object raycast.
        //         if (currentSelected != null)
        //             ResetSelection();

        //         return;
        //     }
        // }

        // Create a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 120f);

        // Perform the raycast. It checks for colliders on objects within the specified 'selectableLayer'.
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            //Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            // If the ray hits a new object (different from the current selection)
            if (hit.collider.gameObject != currentSelected)
            {
                // Deselect the previously selected object (if any)
                ResetSelection();

                // Set the newly hit object as the current selection
                currentSelected = hit.collider.gameObject;
                //Debug.Log("Object selected: " + currentSelection.name);

                // Display the information panel of the selected object
                ApplySelection(currentSelected);
            }
        }
        else
        {
            //Debug.LogWarning("Raycast did not hit any selectable object.");
            // If the ray hits nothing, deselect any previously selected object
            ResetSelection();
        }
    }

    /// <summary>
    /// Display the information panel of the selected object.
    /// </summary>
    void ApplySelection(GameObject selectedHover)
    {
        // Renderer objRenderer = objToHighlight.GetComponent<Renderer>();
        // if (objRenderer != null && selectedMaterial != null)
        // {
        //     // Store the original material before changing it
        //     originalMaterial = objRenderer.material;
        //     // Apply the highlight material
        //     objRenderer.material = selectedMaterial;
        // }

        selectedHover.transform.localScale *= 2;
    }

    /// <summary>
    /// Resets the current selection, restoring the object's original material.
    /// </summary>
    void ResetSelection()
    {
        if (currentSelected != null)
        {
            // // Restore the original material if it was changed
            // Renderer objRenderer = currentSelection.GetComponent<Renderer>();
            // if (objRenderer != null && originalMaterial != null)
            //     objRenderer.material = originalMaterial;

            // Debug.Log("Object deselected: " + currentSelection.name);
            // currentSelection = null;
            // originalMaterial = null;

            currentSelected.transform.localScale /= 2;
            currentSelected = null;
        }
    }

    /// <summary>
    /// Show a small tooltip with the object's name.
    /// </summary>
    void ApplyHover(GameObject hoverObject)
    {
        // Renderer objRenderer = hoverObject.GetComponent<Renderer>();
        // if (objRenderer != null && hoverMaterial != null)
        // {
        //     // Store the original material before changing it
        //     originalMaterial = objRenderer.material;
        //     // Apply the hover highlight material
        //     objRenderer.material = hoverMaterial;
        // }
        hud.PlaceTooltip(hoverObject);

        hoverObject.transform.localScale *= 1.2f;
    }

    /// <summary>
    /// Resets the current hover state, restoring the object's original material.
    /// </summary>
    void ResetHover()
    {
        if (currentHover != null)
        {
            // // Restore the original material if it was changed
            // Renderer objRenderer = currentHover.GetComponent<Renderer>();
            // if (objRenderer != null && originalMaterial != null)
            // {
            //     objRenderer.material = originalMaterial;
            // }
            // Debug.Log("Stopped hovering over: " + currentHover.name);
            // currentHover = null;
            // originalMaterial = null;

            currentHover.transform.localScale /= 1.2f;
            currentHover = null;

            hud.HideTooltip();
        }
    }
    #endregion
}
