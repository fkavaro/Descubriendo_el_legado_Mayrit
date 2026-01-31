using System;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Manages the time in game and data. Singleton.
/// </summary>
public class TimeManager : MonoBehaviour
{
    #region PROPERTIES HELPERS
    public float SimulationSpeed => _gameSimulationSpeed;
    public float CurrentTime => _currentTime;
    #endregion

    #region EDITOR PROPERTIES
    [Header("Time Settings")]
    [Tooltip("Game simulation speed multiplier. Set by Camera states.")]
    [Range(0.1f, 10f)]
    [SerializeField] float _gameSimulationSpeed = 1f;
    [Tooltip("Whether the time will advance automatically or just to reach a wanted time")]
    [SerializeField] bool _isDynamic = false;
    [Tooltip("Wanted time in hours to be reached. If dynamic time is enabled, this will be ignored")]
    [Range(0f, 24f)]
    [SerializeField] float _wantedTime = 10f;
    [Tooltip("Current time in hours since the start of the game")]
    [Range(0f, 24f)]
    [SerializeField] float _currentTime;
    [Tooltip("Time cycle speed multiplier")]
    [SerializeField] float _timeSpeed = 0.2f;

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

    // Dependency Injection
    ProgressManager _progressManager;
    GameManager _gameManager;
    #endregion

    #region LIFE CYCLE
    void Awake()
    {
        ServiceLocator.Instance.Register(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get dependencies from ServiceLocator
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _gameManager = ServiceLocator.Instance.Get<GameManager>();
        _currentTime = _progressManager.CurrentMilestoneMapping.WantedTime;

        // Subscribe to ProgressManager event to set the wanted time when the game starts
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;

        UpdateLighting();
    }

    // Update is called once per frame
    void Update()
    {
        // Difference between current time and wanted time is less than threshold of 0.1f
        _isWantedTimeReached = Mathf.Abs(_currentTime - _wantedTime) < 0.1f;

        // If dynamic time is enabled or if the wanted time has not been reached yet
        if (!_gameManager.IsInPauseState && (_isDynamic || !_isWantedTimeReached))
        {
            UpdateTimeOfDay();
            UpdateLighting();
            CheckActiveLightSource();
        }

        if (_gameSimulationSpeed != Time.timeScale && Time.timeScale != 0f)
        {
            SetSimulationSpeed(_gameSimulationSpeed);
        }
    }

    // Called when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        UpdateLighting();
        CheckActiveLightSource();
    }

    void OnDisable()
    {
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
    }
    #endregion

    #region PUBLIC METHODS
    public void SetSimulationSpeed(float speed)
    {
        // Validate input
        if (float.IsNaN(speed) || float.IsInfinity(speed))
        {
            Debug.LogWarning("TimeManager.SetSimulationSpeed: invalid speed (NaN or Infinity). Change ignored.");
            return;
        }

        // Keep inside sensible bounds (aligns with inspector limits but slightly more permissive for safety)
        const float minSpeed = 0.01f;
        const float maxSpeed = 10f;
        float clamped = Mathf.Clamp(speed, minSpeed, maxSpeed);
        if (!Mathf.Approximately(clamped, speed))
            Debug.LogWarning($"TimeManager: requested simulation speed {speed} was clamped to {clamped}.");

        _gameSimulationSpeed = clamped;

        // Apply time scale for gameplay
        Time.timeScale = _gameSimulationSpeed;

        // Keep physics timestep consistent with timeScale (default fixedDeltaTime is 0.02)
        Time.fixedDeltaTime = 0.02f * Mathf.Max(Time.timeScale, minSpeed);
    }
    #endregion

    #region PRIVATE METHODS
    void UpdateTimeOfDay()
    {
        // Smoothly interpolate towards wanted time with damping for smooth transitions
        const float smoothTime = 1f; // Time to reach wanted time
        _currentTime = Mathf.SmoothDamp(_currentTime, _wantedTime, ref _timeVelocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime * _timeSpeed);

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
        if (mapping != null)
            _wantedTime = mapping.WantedTime;
    }
    #endregion
}
