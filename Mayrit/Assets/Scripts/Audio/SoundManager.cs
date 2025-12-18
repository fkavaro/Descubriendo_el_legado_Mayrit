using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#region ENUMS
public enum MusicType
{
    None,
    MenuMusic,
    GameplayMusic,
}

public enum SFXType
{
    None,
    UI_ButtonClick,
    UI_TourStart,
    UI_TourEnd,
    Camera_Change
}
#endregion

#region STRUCTS
[Serializable]
public struct MusicList
{
    [SerializeField] public MusicType _type;
    [SerializeField] public List<AudioClip> _sounds;
}

[Serializable]
public struct SFXlist
{
    [SerializeField] public SFXType _type;
    [SerializeField] public List<AudioClip> _sounds;
}
#endregion

/// <summary>
/// Handles music and sound effects reproduction. 
/// Requires two audioSource components 
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [SerializeField] private AudioSource _effectsSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField, Range(0, 1)] private float _effectsVolume = 1f;
    [SerializeField, Range(0, 1)] private float _musicVolume = 1f;
    [SerializeField, Range(0, 2)] private float _musicFadeDuration = 0.5f;
    [SerializeField, Range(0, 1)] private float _resumeGuardSeconds = 0.25f;
    [SerializeField] private List<MusicList> _musicLists = new();
    [SerializeField] private List<SFXlist> _SFXLists = new();
    [SerializeField] private bool _skipToNextTrack;
    #endregion

    // Playlist state
    // Current active playlist type; None when stopped
    private MusicType _currentMusicType = MusicType.None;
    // Background coroutine that watches for track end and advances
    private Coroutine _playlistCoroutine;
    // Per-type shuffled queues used to play tracks sequentially without repeats
    private readonly Dictionary<MusicType, Queue<AudioClip>> _musicQueues = new();
    // True while app focus is lost or app is paused; prevents unintended advancing
    private bool _suspendAutoAdvance = false;
    // After regaining focus, ignore auto-advance checks for a brief window
    private float _ignoreAdvanceUntilTime = 0f;
    // True while a fade transition is in progress; avoids volume overrides
    private bool _isFadingMusic = false;

    #region LIFE CYCLE
#if UNITY_EDITOR
    void OnEnable()
    {
        MusicType[] musicTypes = (MusicType[])Enum.GetValues(typeof(MusicType));
        SFXType[] sfxTypes = (SFXType[])Enum.GetValues(typeof(SFXType));

        // Ensure all music types are represented in the list
        foreach (MusicType type in musicTypes)
        {
            if (type == MusicType.None)
                continue;
            if (!_musicLists.Exists(list => list._type == type))
                _musicLists.Add(new MusicList { _type = type, _sounds = new() });
        }

        // Ensure all SFX types are represented in the list
        foreach (SFXType type in sfxTypes)
        {
            if (type == SFXType.None)
                continue;
            if (!_SFXLists.Exists(list => list._type == type))
                _SFXLists.Add(new SFXlist { _type = type, _sounds = new() });
        }

    }
#endif

    void Start()
    {
        if (_effectsSource == null || _musicSource == null)
        {
            Debug.LogError("SoundManager: AudioSource references are missing!");
            return;
        }

        _effectsSource.loop = false;
        _musicSource.loop = false;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // When focus is lost in the editor/player, audio may pause.
        // Prevents the playlist loop from advancing to the next track in that case.
        _suspendAutoAdvance = !hasFocus;
        if (hasFocus)
        {
            // Give audio a brief moment to resume before checking isPlaying
            _ignoreAdvanceUntilTime = Time.unscaledTime + _resumeGuardSeconds;
        }
    }

    void OnApplicationPause(bool paused)
    {
        // Same protection when app pauses/resumes (mobile, editor options, etc.)
        _suspendAutoAdvance = paused;
        if (!paused)
        {
            _ignoreAdvanceUntilTime = Time.unscaledTime + _resumeGuardSeconds;
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor

        // Update volumes in case they were changed in the inspector
        if (_effectsVolume != _effectsSource.volume)
            _effectsSource.volume = _effectsVolume;
        // Avoid overriding fade-in/out transitions
        if (!_isFadingMusic && _musicVolume != _musicSource.volume)
            _musicSource.volume = _musicVolume;

        if (_skipToNextTrack)
        {
            _skipToNextTrack = false;
            SkipToNextMusicTrack();
        }
    }
    #endregion

    #region MUSIC METHODS
    /// <summary>
    /// Starts the menu music playlist. Tracks are shuffled, then played sequentially.
    /// </summary>
    public void PlayMenuMusic(float volume = 1)
    {
        PlayMusic(MusicType.MenuMusic, volume);
    }

    /// <summary>
    /// Starts the gameplay music playlist. Tracks are shuffled, then played sequentially.
    /// </summary>
    public void PlayGameplayMusic(float volume = 1)
    {
        PlayMusic(MusicType.GameplayMusic, volume);
    }

    /// <summary>
    /// Starts or switches the active music playlist to <paramref name="type"/> with the target volume.
    /// </summary>
    private void PlayMusic(MusicType type, float volume = 1)
    {
        if (type == MusicType.None) return;

        _musicVolume = volume;

        // Reset if switching to a different playlist type
        if (_currentMusicType != type)
        {
            _currentMusicType = type;
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        // Start auto-advance loop if not already running
        if (Application.isPlaying && _playlistCoroutine == null)
        {
            _playlistCoroutine = StartCoroutine(PlaylistLoop());
            Debug.Log($"SoundManager: Started {type} playlist.");
        }
    }

    /// <summary>
    /// Pauses the current music track. Does not change playlist state.
    /// </summary>
    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    /// <summary>
    /// Stops music playback and clears playlist state.
    /// </summary>
    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
        _currentMusicType = MusicType.None;

        if (_playlistCoroutine != null)
        {
            StopCoroutine(_playlistCoroutine);
            _playlistCoroutine = null;
        }
    }

    /// <summary>
    /// Updates the target music volume. If a fade is in progress, the fade will reach this value.
    /// </summary>
    public void UpdateMusicVolume(float volume)
    {
        _musicVolume = volume;
    }
    #endregion

    #region PLAYLIST HELPERS
    /// <summary>
    /// Watches the music source and advances only when a track actually ends.
    /// Respects focus/pause guards to avoid unintended skipping on resume.
    /// </summary>
    private IEnumerator PlaylistLoop()
    {
        while (_currentMusicType != MusicType.None)
        {
            // Skip if paused/unfocused or within resume guard window
            if (!_suspendAutoAdvance && Time.unscaledTime >= _ignoreAdvanceUntilTime)
            {
                if (_musicSource.clip == null)
                {
                    PlayNextTrack();
                }
                // Advance only when clip truly ended (not just paused)
                else if (_musicSource.clip != null && !_musicSource.isPlaying &&
                    _musicSource.timeSamples >= _musicSource.clip.samples - 1)
                {
                    PlayNextTrack();
                }
            }
            yield return null;
        }

        _playlistCoroutine = null;
    }

    /// <summary>
    /// Dequeues and plays the next track from the current playlist. Uses fade if configured.
    /// </summary>
    private void PlayNextTrack()
    {
        if (_currentMusicType == MusicType.None) return;

        var queue = EnsureQueue(_currentMusicType);
        if (queue.Count == 0)
        {
            Debug.LogWarning($"No audio clips found for music type {_currentMusicType}");
            return;
        }

        var nextClip = queue.Dequeue();
        if (nextClip == null) return;

        Debug.Log($"SoundManager | {_currentMusicType} playlist - Advancing to next track: {nextClip.name}.");

        // Direct approach
        _musicSource.Stop();
        _musicSource.clip = nextClip;
        _musicSource.volume = _musicVolume;
        _musicSource.Play();

        // For crossfade transitions
        // if (_musicFadeDuration > 0f && _musicSource.clip != null)
        //     StartCoroutine(CrossfadeToClip(nextClip));
    }

    /// <summary>
    /// Ensures a shuffled queue exists for the given type and refills it when empty.
    /// </summary>
    private Queue<AudioClip> EnsureQueue(MusicType type)
    {
        if (!_musicQueues.TryGetValue(type, out var queue))
        {
            queue = new Queue<AudioClip>();
            _musicQueues[type] = queue;
        }

        // Refill queue when empty to loop the playlist
        if (queue.Count == 0)
        {
            var clips = GetMusicClips(type);
            if (clips.Count > 0)
            {
                Shuffle(clips);
                foreach (var clip in clips)
                {
                    if (clip != null) queue.Enqueue(clip);
                }
            }
        }

        return queue;
    }

    /// <summary>
    /// Returns the configured clips for a music type from the inspector lists.
    /// </summary>
    private List<AudioClip> GetMusicClips(MusicType type)
    {
        int idx = _musicLists.FindIndex(list => list._type == type);
        return idx >= 0 ? _musicLists[idx]._sounds ?? new List<AudioClip>() : new List<AudioClip>();
    }

    /// <summary>
    /// Fisher–Yates shuffle.
    /// </summary>
    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    #endregion

    #region TRANSITIONS
    /// <summary>
    /// Performs a simple crossfade on the same AudioSource:
    /// first fades out the current clip, swaps to the next, then fades in.
    /// </summary>
    private IEnumerator CrossfadeToClip(AudioClip nextClip)
    {
        if (nextClip == null) yield break;

        _isFadingMusic = true;
        float halfDuration = _musicFadeDuration * 0.5f;

        // Fade out current track
        float elapsed = 0f;
        float startVol = _musicSource.volume;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
            yield return null;
        }

        // Switch to next track
        _musicSource.Stop();
        _musicSource.clip = nextClip;
        _musicSource.volume = 0f;
        _musicSource.Play();

        // Fade in new track
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, _musicVolume, elapsed / halfDuration);
            yield return null;
        }

        _musicSource.volume = _musicVolume;
        _isFadingMusic = false;
    }

    /// <summary>
    /// Skips to the next track in the current playlist.
    /// If a fade is in progress, it is cancelled before skipping.
    /// </summary>
    public void SkipToNextMusicTrack()
    {
        // Manual skip ignores guard; if fading, stop fade first
        if (_isFadingMusic)
            StopAllCoroutines();
        _playlistCoroutine = StartCoroutine(PlaylistLoop());
        PlayNextTrack();
    }
    #endregion

    #region SOUND EFFECT METHODS
    /// <summary>
    /// Plays a UI button click sound effect.
    /// </summary>
    public void PlayButtonClickSFX()
    {
        PlaySFX(SFXType.UI_ButtonClick);
    }

    /// <summary>
    /// Plays a tour start sound effect.
    /// </summary>
    public void PlayTourStartSFX()
    {
        PlaySFX(SFXType.UI_TourStart);
    }

    /// <summary>
    /// Plays a tour end sound effect.
    /// </summary>
    public void PlayTourEndSFX()
    {
        PlaySFX(SFXType.UI_TourEnd);
    }

    /// <summary>
    /// Plays a random sound of the specified SFX type using <see cref="AudioSource.PlayOneShot(AudioClip,float)"/>.
    /// </summary>
    public void PlaySFX(SFXType type)
    {
        // Return if type is none
        if (type == SFXType.None)
            return;

        // Takes all the clips of the type
        List<AudioClip> clips = _SFXLists.Find(list => list._type == type)._sounds;

        if (clips.Count == 0)
        {
            Debug.LogWarning($"No audio clips found for sound type {type}");
            return;
        }

        // Randomly selects a clip from the list
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];


        // Played in effectsSource
        _effectsSource.PlayOneShot(randomClip, _effectsVolume);
    }

    /// <summary>
    /// Stops any currently playing sound effect.
    /// </summary>
    public void StopSFX()
    {
        _effectsSource.Stop();
        _effectsSource.clip = null;
    }

    /// <summary>
    /// Updates the target SFX volume immediately.
    /// </summary>
    public void UpdateSFXVolume(float volume)
    {
        _effectsVolume = volume;
    }
    #endregion
}