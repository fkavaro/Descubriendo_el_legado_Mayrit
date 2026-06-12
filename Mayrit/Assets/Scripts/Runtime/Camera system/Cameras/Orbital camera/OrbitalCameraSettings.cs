using System;
using UnityEngine;


[Serializable]
public class OrbitalCameraSettings
{
    public float OrbitSpeed => _orbitSpeed;
    public float ZoomValue => _zoomValue;
    public float HorizontalOffset => _horizontalOffset;

    public Transform Target;
    [SerializeField] float _orbitSpeed = 10f;
    [SerializeField] float _zoomValue = 70f;
    [SerializeField] float _horizontalOffset = 20f;
}