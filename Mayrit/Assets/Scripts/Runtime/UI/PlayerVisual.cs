using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    #region PROPERTIES
    [SerializeField] PlayableCharacter _playableCharacter;

    UIDocument _uiDocument;
    Button _playerButton;

    // Dependency Injection
    ScenesController _scenesController;
    ProgressManager _progressManager;
    CameraManager _cameraManager;
    SoundManager _soundManager;
    #endregion

    #region LIFE CYCLLE
    void Awake()
    {
        // Try to get the UIDocument component from the same GameObject
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        _playerButton = root.Q<Button>(name: "PlayerButton");
        if (_playerButton == null)
        {
            Debug.LogWarning("PlayerButtonVisual: No Button with name 'PlayerButton' was found in the UIDocument.");
            return;
        }

        _playerButton.visible = false;
        _playerButton.RegisterCallback<ClickEvent>(OnPlayerButtonClick);
    }

    void Start()
    {
        // Get dependencies from Service Locator
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();

        // Subscribe to events and callbacks
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from events and callbacks
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
    }
    #endregion

    #region PRIVATE METHODS
    void LocateOverPlayer()
    {
        _playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

        if (!_cameraManager.IsInSpectatorState || _playableCharacter == null)
        {
            _playerButton.visible = false;
            return;
        }

        _playerButton.visible = true;
        transform.position = _playableCharacter.transform.position + 10 * Vector3.up;
    }
    #endregion

    #region CALLBACK METHODS
    void OnSceneLoadedPartially(SceneDatabase.SceneType type, SceneDatabase.SceneName name)
    {
        // Milestone loaded: locate over player
        if (type == SceneDatabase.SceneType.Milestone)
            LocateOverPlayer();
    }

    void OnMilestoneChanged(Milestone_DataSO milestoneMapping)
    {
        LocateOverPlayer();
    }

    void OnCameraStateChanged()
    {
        LocateOverPlayer();
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        _cameraManager.SwitchToOrbitalCamera(_playableCharacter.GetComponent<SelectableObject>());
        _soundManager.PlayButtonClickSFX();
    }
    #endregion
}
