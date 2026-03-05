using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    #region EDITOR PROPERTIES
    [Space]
    [SerializeField] OrbitalStateSetting _orbitalStateSetting;
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;
    Button _playerButton;

    // Dependency Injection
    ScenesController _scenesController;
    ProgressManager _progressManager;
    CameraManager _cameraManager;
    SoundManager _soundManager;
    TutorialManager _tutorialManager;
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

        _orbitalStateSetting.IsForCharacter = true;
    }

    void Start()
    {
        // Get dependencies from Service Locator
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _cameraManager = ServiceLocator.Instance.Get<CameraManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundManager>();
        _tutorialManager = ServiceLocator.Instance.Get<TutorialManager>();

        // Subscribe to events and callbacks
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _cameraManager.CameraStateChangedEvent += OnCameraStateChanged;
        _tutorialManager.ShowPlayerFollowerEvent += OnShowPlayerFollowerTutorialEvent;
        _tutorialManager.TutorialCompletedEvent += OnTutorialCompleted;
    }

    void OnDisable()
    {
        // Unsubscribe from events and callbacks
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        _cameraManager.CameraStateChangedEvent -= OnCameraStateChanged;
        _tutorialManager.ShowPlayerFollowerEvent -= OnShowPlayerFollowerTutorialEvent;
        _tutorialManager.TutorialCompletedEvent -= OnTutorialCompleted;
    }
    #endregion

    #region PRIVATE METHODS
    void LocateOverPlayer()
    {
        PlayableCharacter playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

        if (!_cameraManager.IsInSpectatorState || playableCharacter == null)
        {
            _playerButton.visible = false;
            return;
        }

        _playerButton.visible = true;
        transform.position = playableCharacter.transform.position + 10 * Vector3.up;

        _orbitalStateSetting.Target = playableCharacter.transform;
        _orbitalStateSetting.DataToShow = playableCharacter.CharacterData;
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
        if (_orbitalStateSetting.DataToShow == null)
        {
            Debug.LogWarning($"[PlayerVisual] No information to show.", this);
            return;
        }

        if (_orbitalStateSetting.Target == null)
        {
            Debug.LogWarning($"[PlayerVisual] Can't orbit around null target.", this);
            return;
        }

        _cameraManager.SwitchToOrbitalCamera(_orbitalStateSetting);
        _soundManager.PlayButtonClickSFX();
    }

    void OnShowPlayerFollowerTutorialEvent(bool isShown)
    {
        _playerButton.style.display = isShown ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void OnTutorialCompleted()
    {
        _tutorialManager.ShowPlayerFollowerEvent -= OnShowPlayerFollowerTutorialEvent;
        _tutorialManager.TutorialCompletedEvent -= OnTutorialCompleted;
    }
    #endregion
}
