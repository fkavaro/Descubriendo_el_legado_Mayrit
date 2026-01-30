using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SoundDatabase
{
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
        UIButtonClick,
        UITourStart,
        UITourEnd,
        CameraTransition
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
}