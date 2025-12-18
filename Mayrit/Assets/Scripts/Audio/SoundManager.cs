using UnityEngine;
using System;
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
    [SerializeField, Range(0, 1)] private float _musicFadeDuration = 0.5f;
    [SerializeField] private List<MusicList> _musicLists = new();
    [SerializeField] private List<SFXlist> _SFXLists = new();
    #endregion

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
        _musicSource.loop = true;
    }

    void Update()
    {
        if (!Application.isPlaying)
            return;// To avoid error in editor

        // Update volumes in case they were changed in the inspector
        if (_effectsVolume != _effectsSource.volume)
            _effectsSource.volume = _effectsVolume;
        if (_musicVolume != _musicSource.volume)
            _musicSource.volume = _musicVolume;
    }
    #endregion

    #region MUSIC METHODS
    public void PlayMenuMusic(float volume = 1)
    {
        PlayMusic(MusicType.MenuMusic, volume);
    }

    public void PlayGameplayMusic(float volume = 1)
    {
        PlayMusic(MusicType.GameplayMusic, volume);
    }

    private void PlayMusic(MusicType type, float volume = 1)
    {
        // Return if type is none
        if (type == MusicType.None)
            return;

        // Takes all the clips of the type
        List<AudioClip> clips = _musicLists.Find(list => list._type == type)._sounds;

        if (clips.Count == 0)
        {
            Debug.LogWarning($"No audio clips found for music type {type}");
            return;
        }

        // Randomly selects a clip from the list
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];

        // Stop the current clip and change to the new clip
        _musicSource.Stop();
        _musicSource.clip = randomClip;
        _musicSource.volume = volume;
        _musicSource.Play();

        // Played in musicSource with a transition
        //StartCoroutine(MusicTransition(randomClip, volume));
    }

    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void UpdateMusicVolume(float volume)
    {
        _musicVolume = volume;
        _musicSource.volume = volume;
    }
    #endregion

    #region SOUND EFFECT METHODS
    public void PlayButtonClickSFX(float volume = 1)
    {
        PlaySFX(SFXType.UI_ButtonClick, volume);
    }

    public void PlayTourStartSFX(float volume = 1)
    {
        PlaySFX(SFXType.UI_TourStart, volume);
    }

    public void PlayTourEndSFX(float volume = 1)
    {
        PlaySFX(SFXType.UI_TourEnd, volume);
    }

    /// <summary>
    /// Plays random sound of a specific type
    /// </summary>
    public void PlaySFX(SFXType type, float volume = 1)
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
        _effectsSource.PlayOneShot(randomClip, volume);
    }

    public void StopSFX()
    {
        _effectsSource.Stop();
        _effectsSource.clip = null;
    }

    public void UpdateSFXVolume(float volume)
    {
        _effectsVolume = volume;
        _effectsSource.volume = volume;
    }
    #endregion

    // NOT WORKING /////////////////////////////////////////////////////////////
    // private IEnumerator MusicTransition(AudioClip newClip, float targetVolume)
    // {
    //     Debug.LogWarning("Transitioning to new music clip: " + newClip.name);

    //     float percent = 0;

    //     // Fade out the current clip
    //     while (percent < 1)
    //     {
    //         percent += Time.deltaTime * 1 / fadeDuration;
    //         musicSource.volume = Mathf.Lerp(1f, 0, percent);
    //         yield return null;
    //     }

    //     // Stop the current clip and change to the new clip
    //     //musicSource.Stop();
    //     musicSource.clip = newClip;
    //     musicSource.Play();

    //     percent = 0;
    //     // Fade in the new clip
    //     while (percent < 1)
    //     {
    //         percent += Time.deltaTime * 1 / fadeDuration;
    //         musicSource.volume = Mathf.Lerp(0, 1f, percent);
    //         yield return null;
    //     }

    //     // Ensure the volume is set to the target volume at the end
    //     musicSource.volume = targetVolume;
    // }
}