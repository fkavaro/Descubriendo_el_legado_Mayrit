using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public abstract class ACameraState : AState
{
    protected readonly CinemachineCamera _camera;
    protected readonly float _simulationSpeed;

    protected ACameraState(string name,
        CinemachineCamera camera,
        float simulationSpeed)
    : base(name)
    {
        _camera = camera;
        _simulationSpeed = simulationSpeed;
    }
}