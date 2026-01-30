using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles music and sound effects reproduction. 
/// Requires two audioSource components.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioListener))]
public class SoundManager : ABehaviourEntity<FiniteStateMachine<AMusicState>>
{
    #region GETTERS
    public AudioSource EffectsSource => _effectsSource;
    public AudioSource MusicSource => _musicSource;
    public float MusicFadeDuration => _musicFadeDuration;
    public float ResumeGuardSeconds => _resumeGuardSeconds;
    public List<SoundDatabase.MusicList> MusicLists => _musicLists;
    public List<SoundDatabase.SFXlist> SFXLists => _SFXLists;
    public float MusicVolumeSet => _uiManager.MusicVolumeValueSet;
    public float SFXVolumeSet => _uiManager.SFXVolumeValueSet;
    #endregion

    #region EDITOR PROPERTIES
    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioSource _musicSource;
    [Range(0, 1)] public float EffectsVolume = 1f;
    [Range(0, 1)] public float MusicVolume = 1f;
    [SerializeField, Range(0, 2)] private float _musicFadeDuration = 0.5f;
    [SerializeField, Range(0, 1)] private float _resumeGuardSeconds = 0.25f;
    [SerializeField] private List<SoundDatabase.MusicList> _musicLists = new();
    [SerializeField] private List<SoundDatabase.SFXlist> _SFXLists = new();
    [SerializeField] private bool _skipToNextTrack;
    #endregion

    #region INTERNAL PROPERTIES
    AudioListener _audioListener;
    SoundController _soundController;

    // States
    FiniteStateMachine<AMusicState> _fsm;
    MainMenu_MusicState _mainMenuState;
    GamePlay_MusicState _gamePlayState;

    // Dependency Injection
    ScenesController _scenesController;
    UIManager _uiManager;
    #endregion

    #region INHERITED
    public override FiniteStateMachine<AMusicState> DefineBehaviourSystemOnAwake()
    {
        _fsm = new(this);

        _soundController = new(this);

        // States initialization
        _mainMenuState = new(_soundController);
        _gamePlayState = new(_soundController);

        // State AwakeState calls
        _mainMenuState.AwakeState();
        _gamePlayState.AwakeState();

        _fsm.SetInitialState(_mainMenuState);

        return _fsm;
    }
    #endregion

    #region LIFE CYCLE
#if UNITY_EDITOR
    void OnEnable()
    {
        SoundDatabase.MusicType[] musicTypes = (SoundDatabase.MusicType[])Enum.GetValues(typeof(SoundDatabase.MusicType));
        SoundDatabase.SFXType[] sfxTypes = (SoundDatabase.SFXType[])Enum.GetValues(typeof(SoundDatabase.SFXType));

        // Ensure all music types are represented in the list
        foreach (SoundDatabase.MusicType type in musicTypes)
        {
            if (type == SoundDatabase.MusicType.None)
                continue;
            if (!_musicLists.Exists(list => list._type == type))
                _musicLists.Add(new SoundDatabase.MusicList { _type = type, _sounds = new() });
        }

        // Ensure all SFX types are represented in the list
        foreach (SoundDatabase.SFXType type in sfxTypes)
        {
            if (type == SoundDatabase.SFXType.None)
                continue;
            if (!_SFXLists.Exists(list => list._type == type))
                _SFXLists.Add(new SoundDatabase.SFXlist { _type = type, _sounds = new() });
        }
    }
#endif

    protected override void Awake()
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor

        // Only allow the registered SoundManager to initialize
        var registered = ServiceLocator.Instance.Get<SoundManager>();
        if (registered != null && registered != this)
        {
            Destroy(gameObject);
            return;
        }

        // Register to Service Locator
        ServiceLocator.Instance.Register(this);

        _audioListener = GetComponent<AudioListener>();

        base.Awake();
    }



    protected override void Start()
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor

        if (_effectsSource == null || _musicSource == null)
        {
            Debug.LogError("SoundManager: AudioSource references are missing!");
            return;
        }

        _effectsSource.loop = false;
        _musicSource.loop = false;

        // Get dependencies from ServiceLocator
        _uiManager = ServiceLocator.Instance.Get<UIManager>();
        _scenesController = ServiceLocator.Instance.Get<ScenesController>();

        // Subscribe to events
        _scenesController.SceneChangedEvent += OnSceneChanged;
        _uiManager.MusicVolumeChangedEvent += _soundController.UpdateMusicVolume;
        _uiManager.SFXVolumeChangedEvent += _soundController.UpdateSFXVolume;

        // Set initial volumes
        _soundController.Start();

        base.Start();
    }

    protected override void Update()
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor

        // TODO remove or implement in game
        if (_skipToNextTrack)
        {
            _skipToNextTrack = false;
            _soundController.SkipToNextMusicTrack();
        }

        base.Update();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor
        _soundController.OnApplicationFocus(hasFocus);
    }

    void OnApplicationPause(bool paused)
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor
        _soundController.OnApplicationPause(paused);
    }
    #endregion

    #region PUBLIC METHODS
    public void PlaySFX(SoundDatabase.SFXType type) => _soundController.PlaySFX(type);
    public void PlayButtonClickSFX() => _soundController.PlaySFX(SoundDatabase.SFXType.UIButtonClick);
    public void PlayCameraTransitionSFX() => _soundController.PlaySFX(SoundDatabase.SFXType.CameraTransition);
    public void PlayTourStartSFX() => _soundController.PlaySFX(SoundDatabase.SFXType.UITourStart);
    public void PlayTourEndSFX() => _soundController.PlaySFX(SoundDatabase.SFXType.UITourEnd);

    /// <summary>
    /// Stops any currently playing sound effect.
    /// </summary>
    public void ResetSFX() => _soundController.ResetSFX();

    /// <summary>
    /// Pauses the current music track. Does not change playlist state.
    /// </summary>
    public void PauseMusic() => _soundController.PauseMusic();

    /// <summary>
    /// Stops music playback and clears playlist state.
    /// </summary>
    public void ResetMusic() => _soundController.ResetMusic();
    #endregion

    #region CALLBACK METHODS
    private void OnSceneChanged(Dictionary<string, string> loadedScenes, List<string> unloadedSlots)
    {
        // Disable audio listener if GameScene has been loaded and enable if has been unloaded
        if (loadedScenes.ContainsValue(SceneDatabase.Name.GamePlayScene))
            _audioListener.enabled = false;
        // TODO: else if (loadedScenes.ContainsValue(SceneDatabase.Name.MainMenuScene))
        else if (unloadedSlots.Contains(SceneDatabase.Slot.Session))
            _audioListener.enabled = true;
    }
    #endregion
}