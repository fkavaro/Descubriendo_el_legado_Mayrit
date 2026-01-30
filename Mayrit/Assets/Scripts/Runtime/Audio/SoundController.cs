using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SoundController
{
    #region GETTERS
    AudioSource EffectsSource => _soundManager.EffectsSource;
    AudioSource MusicSource => _soundManager.MusicSource;
    float MusicFadeDuration => _soundManager.MusicFadeDuration;
    float ResumeGuardSeconds => _soundManager.ResumeGuardSeconds;
    List<SoundDatabase.MusicList> MusicLists => _soundManager.MusicLists;
    List<SoundDatabase.SFXlist> SFXLists => _soundManager.SFXLists;
    float MusicVolumeSet => _soundManager.MusicVolumeSet;
    float SFXVolumeSet => _soundManager.SFXVolumeSet;
    #endregion

    #region PROPERTIES
    readonly SoundManager _soundManager;
    SoundDatabase.MusicType _currentMusicType;
    Coroutine _playlistCoroutine;
    readonly Dictionary<SoundDatabase.MusicType, Queue<AudioClip>> _musicQueues = new();
    bool _suspendAutoAdvance;
    float _ignoreAdvanceUntilTime;
    bool _isFadingMusic;
    #endregion

    #region CONSTRUCTOR
    public SoundController(SoundManager soundManager)
    {
        _soundManager = soundManager;
        _currentMusicType = SoundDatabase.MusicType.None;
        _suspendAutoAdvance = false;
        _ignoreAdvanceUntilTime = 0f;
        _isFadingMusic = false;
    }
    #endregion

    #region LYFECYCLE
    public void Start()
    {
        UpdateMusicVolume(MusicVolumeSet);
        UpdateSFXVolume(SFXVolumeSet);
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        _suspendAutoAdvance = !hasFocus;
        if (hasFocus)
            _ignoreAdvanceUntilTime = Time.unscaledTime + ResumeGuardSeconds;
    }

    public void OnApplicationPause(bool paused)
    {
        _suspendAutoAdvance = paused;
        if (!paused)
            _ignoreAdvanceUntilTime = Time.unscaledTime + ResumeGuardSeconds;
    }
    #endregion

    #region SFX METHODS
    /// <summary>
    /// Plays a random sound of the specified SFX type using <see cref="AudioSource.PlayOneShot(AudioClip,float)"/>.
    /// </summary>
    public void PlaySFX(SoundDatabase.SFXType type)
    {
        // Return if type is none
        if (type == SoundDatabase.SFXType.None)
            return;

        // Takes all the clips of the type
        List<AudioClip> clips = SFXLists.Find(list => list._type == type)._sounds;

        if (clips.Count == 0)
        {
            Debug.LogWarning($"No audio clips found for sound type {type}");
            return;
        }

        // Randomly selects a clip from the list
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];

        // Played in effectsSource
        EffectsSource.PlayOneShot(randomClip);
    }

    /// <summary>
    /// Stops any currently playing sound effect.
    /// </summary>
    public void ResetSFX()
    {
        EffectsSource.Stop();
        EffectsSource.clip = null;
    }

    /// <summary>
    /// Updates the target SFX volume immediately.
    /// </summary>
    public void UpdateSFXVolume(float volume)
    {
        _soundManager.EffectsVolume = volume;
        EffectsSource.volume = volume;
    }
    #endregion

    #region MUSIC METHODS
    /// <summary>
    /// Starts or switches the active music playlist to <paramref name="type"/> with the target volume.
    /// </summary>
    public void PlayMusic(SoundDatabase.MusicType type)
    {
        if (type == SoundDatabase.MusicType.None) return;

        // Reset if switching to a different playlist type
        if (_currentMusicType != type)
        {
            _currentMusicType = type;
            MusicSource.Stop();
            MusicSource.clip = null;
        }

        // Start auto-advance loop if not already running
        if (Application.isPlaying && _playlistCoroutine == null)
        {
            _playlistCoroutine = _soundManager.StartCoroutine(PlaylistLoop());
            //Debug.Log($"SoundManager: Started {type} playlist.");
        }
    }

    /// <summary>
    /// Pauses the current music track. Does not change playlist state.
    /// </summary>
    public void PauseMusic()
    {
        MusicSource.Pause();
    }

    /// <summary>
    /// Stops music playback and clears playlist state.
    /// </summary>
    public void ResetMusic()
    {
        MusicSource.Stop();
        MusicSource.clip = null;
        _currentMusicType = SoundDatabase.MusicType.None;

        if (_playlistCoroutine != null)
        {
            _soundManager.StopCoroutine(_playlistCoroutine);
            _playlistCoroutine = null;
        }
    }

    /// <summary>
    /// Updates the target music volume. If a fade is in progress, the fade will reach this value.
    /// </summary>
    public void UpdateMusicVolume(float volume)
    {
        _soundManager.MusicVolume = volume;
        MusicSource.volume = volume;
    }
    #endregion

    #region PLAYLIST HELPERS
    /// <summary>
    /// Watches the music source and advances only when a track actually ends.
    /// Respects focus/pause guards to avoid unintended skipping on resume.
    /// </summary>
    private IEnumerator PlaylistLoop()
    {
        while (_currentMusicType != SoundDatabase.MusicType.None)
        {
            // Skip if paused/unfocused or within resume guard window
            if (!_suspendAutoAdvance && Time.unscaledTime >= _ignoreAdvanceUntilTime)
            {
                if (MusicSource.clip == null)
                {
                    PlayNextTrack();
                }
                // Advance only when clip truly ended (not just paused)
                else if (MusicSource.clip != null && !MusicSource.isPlaying &&
                    MusicSource.timeSamples >= MusicSource.clip.samples - 1)
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
        if (_currentMusicType == SoundDatabase.MusicType.None) return;

        var queue = EnsureQueue(_currentMusicType);
        if (queue.Count == 0)
        {
            Debug.LogWarning($"No audio clips found for music type {_currentMusicType}");
            return;
        }

        var nextClip = queue.Dequeue();
        if (nextClip == null) return;

        //Debug.Log($"SoundManager | {_currentMusicType} playlist - Advancing to next track: {nextClip.name}.");

        // Direct approach
        MusicSource.Stop();
        MusicSource.clip = nextClip;
        MusicSource.volume = _soundManager.MusicVolume;
        MusicSource.Play();

        // For crossfade transitions
        // if (_musicFadeDuration > 0f && _musicSource.clip != null)
        //     StartCoroutine(CrossfadeToClip(nextClip));
    }

    /// <summary>
    /// Ensures a shuffled queue exists for the given type and refills it when empty.
    /// </summary>
    private Queue<AudioClip> EnsureQueue(SoundDatabase.MusicType type)
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
    private List<AudioClip> GetMusicClips(SoundDatabase.MusicType type)
    {
        int idx = MusicLists.FindIndex(list => list._type == type);
        return idx >= 0 ? MusicLists[idx]._sounds ?? new List<AudioClip>() : new List<AudioClip>();
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
    /// Skips to the next track in the current playlist.
    /// If a fade is in progress, it is cancelled before skipping.
    /// </summary>
    public void SkipToNextMusicTrack()
    {
        // Manual skip ignores guard; if fading, stop fade first
        if (_isFadingMusic)
            _soundManager.StopAllCoroutines();
        _playlistCoroutine = _soundManager.StartCoroutine(PlaylistLoop());
        PlayNextTrack();
    }

    /// <summary>
    /// Performs a simple crossfade on the same AudioSource:
    /// first fades out the current clip, swaps to the next, then fades in.
    /// </summary>
    private IEnumerator CrossfadeToClip(AudioClip nextClip)
    {
        if (nextClip == null) yield break;

        _isFadingMusic = true;
        float halfDuration = MusicFadeDuration * 0.5f;

        // Fade out current track
        float elapsed = 0f;
        float startVol = MusicSource.volume;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(startVol, 0f, elapsed / halfDuration);
            yield return null;
        }

        // Switch to next track
        MusicSource.Stop();
        MusicSource.clip = nextClip;
        MusicSource.volume = 0f;
        MusicSource.Play();

        // Fade in new track
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            MusicSource.volume = Mathf.Lerp(0f, _soundManager.MusicVolume, elapsed / halfDuration);
            yield return null;
        }

        MusicSource.volume = _soundManager.MusicVolume;
        _isFadingMusic = false;
    }
    #endregion
}