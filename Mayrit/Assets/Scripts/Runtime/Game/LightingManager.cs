using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LightingManager : MonoBehaviour
{
    #region PROPERTIES HELPERS
    public float CurrentTime => _currentTime;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Time Settings")]
    [SerializeField] bool _hasTransitions = false;
    [Tooltip("In hours")]
    [Range(0f, 24f)]
    [SerializeField] float _currentTime;
    [SerializeField] MilestonesTimesSO _milestoneTimes;

    [Header("Transition Settings")]

    [Tooltip("Whether the time will advance automatically or just to reach a wanted time")]
    [SerializeField] bool _isDynamic = false;
    [Tooltip("Wanted time in hours to be reached. If dynamic time is enabled, this will be ignored")]
    [Range(0f, 24f)]
    [SerializeField] float _wantedTime = 10f;
    [Tooltip("Time cycle speed multiplier")]
    [SerializeField] float _transitionSpeed = 0.1f;

    [Header("Sun Light Settings")]
    [SerializeField] Light _sunSource;
    [SerializeField] float _sunAngle;
    [Range(0f, 90f)]
    [SerializeField] float _sunLatitude = 20f;
    [Range(-180f, 180f)]
    [SerializeField] float _sunLongitude = -90;
    [SerializeField] float _sunMaxIntensity;
    [SerializeField] AnimationCurve _sunIntensityCurve;
    [SerializeField] AnimationCurve _sunTemperatureCurve;

    [Header("Moon Light Settings")]
    [SerializeField] Light _moonSource;
    [Range(0f, 90f)]
    [SerializeField] float _moonLatitude = 40f;
    [Range(-180f, 180f)]
    [SerializeField] float _moonLongitude = 90;
    [SerializeField] float _moonMaxIntensity;
    [SerializeField] AnimationCurve _moonIntensityCurve;
    #endregion

    #region INTERNAL PROPERTIES
    [HideInInspector] public bool _isDayTime = true; // Whether current time is between 6 and 18 hours or not
    bool _isWantedTimeReached; // Whether the current time is close enough to the wanted time
    float _normalisedTime; // Normalised time value between 0 and 1, where 0 is midnight and 1 is the next midnight
    float _timeVelocity; // Velocity for SmoothDamp

    MilestoneSetting _milestoneSetting;

    // Dependency Injection
    ProgressManager _progressManager;
    GameManager _gameManager;
    #endregion

    #region LIFE CYCLE
#if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying)
            return;

        _milestoneSetting = GetComponentInParent<MilestoneSetting>();

        _currentTime = _milestoneSetting.MilestonePreviewIndex >= 0
        ? _milestoneTimes.List[_milestoneSetting.MilestonePreviewIndex]._time
        : 10f; // Default to 10am if no milestone preview index is set

        UpdateLighting();
        CheckActiveLightSource();
    }
#endif

    void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!_hasTransitions)
            return;

        // Get dependencies from ServiceLocator
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        //_currentTime = _progressManager.CurrentMilestoneData.WantedTime;

        // Subscribe to ProgressManager event to set the wanted time when the game starts
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

        UpdateLighting();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasTransitions)
            return;

        // Difference between current time and wanted time is less than threshold of 0.1f
        _isWantedTimeReached = Mathf.Abs(_currentTime - _wantedTime) < 0.1f;

        // If dynamic time is enabled or if the wanted time has not been reached yet
        if (!_gameManager.IsInPauseState && (_isDynamic || !_isWantedTimeReached))
        {
            UpdateTimeOfDay();
            UpdateLighting();
            CheckActiveLightSource();
        }
    }

    void OnDisable()
    {
        if (_hasTransitions)
            _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        ServiceLocator.Instance.Unregister(this);
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTimeOfDay()
    {
        // Smoothly interpolate towards wanted time with damping for smooth transitions
        const float smoothTime = 1f; // Time to reach wanted time
        _currentTime = Mathf.SmoothDamp(_currentTime, _wantedTime, ref _timeVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime * _transitionSpeed);

        // Ensure current time wraps around after 24 hours
        if (_currentTime >= 24f)
            _currentTime = 0f;
    }

    void UpdateLighting()
    {
        _sunAngle = _currentTime / 24f * 360f; // Calculate sun angle based on current time

        // Rotate light sources based on latitude, longitude, and angle
        _sunSource.transform.localRotation = Quaternion.Euler(_sunLatitude - 90, _sunLongitude, 0) * Quaternion.Euler(0, _sunAngle, 0);
        _moonSource.transform.localRotation = Quaternion.Euler(90 - _moonLatitude, _moonLongitude, 0) * Quaternion.Euler(0, _sunAngle, 0);

        // Normalise current time to a value between 0 and 1
        _normalisedTime = _currentTime / 24f;

        // Modify sun properties based on the evaluated curves
        _sunSource.intensity = _sunMaxIntensity * _sunIntensityCurve.Evaluate(_normalisedTime);
        _sunSource.colorTemperature = _sunTemperatureCurve.Evaluate(_normalisedTime) * 10000f; // In kelvin units

        // Modify moon properties based on the evaluated curves
        _moonSource.intensity = _moonMaxIntensity * _moonIntensityCurve.Evaluate(_normalisedTime);
    }

    void CheckActiveLightSource()
    {
        // During day time. Use a small hysteresis window to avoid toggling at the
        // exact threshold when time steps are near the boundary.
        const float dayStart = 6f;
        const float dayEnd = 18f;
        const float hysteresis = 0.05f; // hours (~3 minutes at 1x timeSpeed)

        if (_currentTime >= dayStart + hysteresis && _currentTime < dayEnd - hysteresis)
        {
            _isDayTime = true;

            // Enable sun shadows if not enabled
            if (_sunSource.shadows == LightShadows.None)
                _sunSource.shadows = LightShadows.Hard;

            // Disable moon shadows if enabled
            if (_moonSource.shadows != LightShadows.None)
                _moonSource.shadows = LightShadows.None;
        }
        else // During night time
        {
            _isDayTime = false;

            // Disable sun shadows if enabled
            if (_sunSource.shadows != LightShadows.None)
                _sunSource.shadows = LightShadows.None;

            // Enable moon shadows if not enabled
            if (_moonSource.shadows == LightShadows.None)
                _moonSource.shadows = LightShadows.Soft;
        }

        // Roughly enable/disable sun and moon with a small margin to avoid flicker
        bool shouldSunBeActive = _currentTime >= 4f + hysteresis && _currentTime < 20f - hysteresis;
        if (_sunSource.gameObject.activeSelf != shouldSunBeActive)
            _sunSource.gameObject.SetActive(shouldSunBeActive);

        bool shouldMoonBeActive = !(_currentTime >= 6f - hysteresis && _currentTime < 17f + hysteresis);
        if (_moonSource.gameObject.activeSelf != shouldMoonBeActive)
            _moonSource.gameObject.SetActive(shouldMoonBeActive);
    }
    #endregion

    #region EVENT METHODS
    void OnMilestoneChanged(Milestone_DataSO mapping)
    {
        // if (mapping != null)
        //     _wantedTime = mapping.WantedTime;
    }
    #endregion
}
