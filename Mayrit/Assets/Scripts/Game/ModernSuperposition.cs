using System;
using UnityEngine;

public class ModernSuperposition : MonoBehaviour
{
    [Header("Settings")]
    public bool _isActive = false;

    bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            SetChildrenActive(_isActive);
        }
    }

    void Start()
    {
        // To know when to deactivate the mode if the camera changes to 3rd person
        CameraManager.Instance.OnCameraStateChanged += CheckCameraState;

        // To know when the button is pressed in the HUD
        SpectatorHUD_UIState spectatorHUD = UIManager.Instance._spectatorHUDState;
        if (spectatorHUD != null)
            spectatorHUD.OnModernSuperpositionToggled += ToggleMode;
    }

    void OnValidate()
    {
        SetChildrenActive(IsActive);
    }

    public void ToggleMode()
    {
        IsActive = !IsActive;
    }

    void CheckCameraState()
    {
        if (CameraManager.Instance._thirdPersonState.IsCurrentState())
            IsActive = false;
    }

    void SetChildrenActive(bool isActive)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(isActive);
    }
}
