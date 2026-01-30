using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AMusicState : AState
{
    protected SoundDatabase.MusicType _musicType;
    protected SoundController _soundController;

    public AMusicState(string statename, SoundController soundController, SoundDatabase.MusicType musicType)
    : base(statename)
    {
        _soundController = soundController;
        _musicType = musicType;
    }

    public override void StartState()
    {
        base.StartState();

        _soundController.PlayMusic(_musicType);
    }
}