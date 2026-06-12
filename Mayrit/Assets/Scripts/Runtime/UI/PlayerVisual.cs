using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PlayerVisual : Billboard
{
    #region EDITOR PROPERTIES
    [SerializeField] DataSO _data;
    [Space]
    [SerializeField] OrbitalCameraSettings _orbitalCameraSettings;
    #endregion

    #region INTERNAL PROPERTIES
    UIDocument _uiDocument;
    Button _playerButton;

    // Dependency Injection
    ScenesController _scenesController;
    ProgressManager _progressManager;
    GameManager _gameManager;
    SoundSystem _soundManager;
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
    }

    protected override void Start()
    {
        base.Start();

        // Get dependencies from Service Locator
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _soundManager = ServiceLocator.Instance.Get<SoundSystem>();
        _tutorialManager = ServiceLocator.Instance.Get<TutorialManager>();

        // Subscribe to events and callbacks
        _scenesController.SceneLoadedPartiallyEvent += OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _gameManager.StateChangedEvent += OnGameStateChanged;
        _tutorialManager.ShowPlayerFollowerEvent += OnShowPlayerFollowerTutorialEvent;
        _tutorialManager.TutorialCompletedEvent += OnTutorialCompleted;
    }

    void OnDisable()
    {
        // Unsubscribe from events and callbacks
        _scenesController.SceneLoadedPartiallyEvent -= OnSceneLoadedPartially;
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        _gameManager.StateChangedEvent -= OnGameStateChanged;
        _tutorialManager.ShowPlayerFollowerEvent -= OnShowPlayerFollowerTutorialEvent;
        _tutorialManager.TutorialCompletedEvent -= OnTutorialCompleted;
    }
    #endregion

    #region PRIVATE METHODS
    void LocateOverPlayer()
    {
        PlayableCharacter playableCharacter = ServiceLocator.Instance.Get<PlayableCharacter>();

        if (!_gameManager.IsInAerialState || playableCharacter == null)
        {
            _playerButton.visible = false;
            return;
        }

        _playerButton.visible = true;
        transform.position = playableCharacter.transform.position + 10 * Vector3.up;

        _orbitalCameraSettings.Target = playableCharacter.transform;
        _data = playableCharacter.CharacterData;
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

    void OnGameStateChanged()
    {
        if (_gameManager.IsInAerialState)
            LocateOverPlayer();
    }

    void OnPlayerButtonClick(ClickEvent evt)
    {
        if (_data == null)
        {
            Debug.LogWarning($"[PlayerVisual] No information to show.", this);
            return;
        }

        if (_orbitalCameraSettings.Target == null)
        {
            Debug.LogWarning($"[PlayerVisual] Can't orbit around null target.", this);
            return;
        }

        // TODO invoke event (this should be a service)
        _gameManager.SwitchToAtPOIState(_data, _orbitalCameraSettings);
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
