using System;
using UnityEngine;

public class ModernSuperposition : MonoBehaviour
{
    #region PROPERTY HELPERS
    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            SetChildrenActive(_isActive);
        }
    }
    #endregion

    #region EDITOR PROPERTIES
    [Header("Settings")]
    public bool _isActive = false;
    #endregion

    #region LIFE CYCLE
    void Start()
    {
        // To know when to deactivate the mode if the camera changes to 3rd person
        CameraManager.Instance.OnCameraStateChangedEvent += OnCameraStateChanged;

        // To know when the button is pressed in the HUD
        UIManager.Instance.ModernSuperpositionToggledEvent += ToggleMode;
    }

    void OnValidate()
    {
        SetChildrenActive(IsActive);
    }
    #endregion 

    #region PUBLIC METHODS
    public void ToggleMode()
    {
        IsActive = !IsActive;
    }
    #endregion

    #region PRIVATE METHODS
    void OnCameraStateChanged()
    {
        if (CameraManager.Instance.IsInThirdPersonState)
            IsActive = false;
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
    #endregion
}
