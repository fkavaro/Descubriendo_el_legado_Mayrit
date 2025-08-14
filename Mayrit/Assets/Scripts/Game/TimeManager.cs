using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    #region PUBLIC PROPERTIES
    [Header("Time Settings")]
    [Tooltip("Whether the time will advance automatically or just to reach a wanted time")]
    public bool _isDynamic = false;
    public bool _isWantedTimeReached;

    [Tooltip("Wanted time in hours to be reached. If dynamic time is enabled, this will be ignored")]
    [Range(0f, 24f)]
    public float _wantedTime = 10f;

    [Tooltip("Current time in hours since the start of the game")]
    [Range(0f, 24f)]
    public float _currentTime = 8f;

    [Tooltip("Time cycle speed multiplier")]
    public float _timeSpeed = 1f;

    [Tooltip("Whether current time is between 6 and 18 hours or not")]
    public bool _isDayTime = true;

    [Tooltip("Normalised time value between 0 and 1, where 0 is midnight and 1 is the next midnight")]
    [Range(0f, 1f)]
    public float _normalisedTime;

    [Header("Sun Light Settings")]
    public Light _sunSource;
    public float _sunAngle;
    [Range(0f, 90f)]
    public float _sunLatitude = 20f;
    [Range(-180f, 180f)]
    public float _sunLongitude = -90;
    public float _sunMaxIntensity;
    public AnimationCurve _sunIntensityCurve;
    public AnimationCurve _sunTemperatureCurve;

    [Header("Moon Light Settings")]
    public Light _moonSource;
    [Range(0f, 90f)]
    public float _moonLatitude = 40f;
    [Range(-180f, 180f)]
    public float _moonLongitude = 90;
    public float _moonMaxIntensity;
    public AnimationCurve _moonIntensityCurve;
    #endregion

    #region PRIVATE PROPERTIES

    #endregion

    #region INHERITED METHODS
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to ProgressManager event to set the wanted time when the game starts
        ProgressManager.Instance.OnTimeSet += (time) => { _wantedTime = time; };
    }

    // Update is called once per frame
    void Update()
    {
        // Difference between current time and wanted time is less than threshold of 0.1f
        _isWantedTimeReached = Mathf.Abs(_currentTime - _wantedTime) < 0.1f;

        // If dynamic time is enabled or if the wanted time has not been reached yet
        if (_isDynamic || !_isWantedTimeReached)
        {
            UpdateTimeOfDay();
            UpdateLighting();
            CheckActiveLightSource();
        }
    }

    // Called when the script is loaded or a value is changed in the inspector
    void OnValidate()
    {
        UpdateLighting();
        CheckActiveLightSource();
    }
    #endregion

    #region PUBLIC METHODS

    #endregion

    #region PRIVATE METHODS
    void UpdateTimeOfDay()
    {
        // Update the current time based on the time speed
        _currentTime += Time.deltaTime * _timeSpeed;

        // Ensure current time wraps around after 24 hours
        if (_currentTime >= 24f)
            _currentTime = 0f;
    }

    void UpdateLighting()
    {
        _sunAngle = _currentTime / 24f * 360f; // Calculate sun angle based on current time

        // Rotate sun light source
        //_sunSource.transform.rotation = Quaternion.Euler(_sunAngle - 90f, _sunPosition, 0f); // -90 so that its down at midnight
        _sunSource.transform.localRotation = Quaternion.Euler(_sunLatitude - 90, _sunLongitude, 0) * Quaternion.Euler(0, _sunAngle, 0);

        // Rotate moon light source (contrary to the sun)
        //_moonSource.transform.rotation = Quaternion.Euler(_sunAngle + 90f, _sunPosition, 0f); // +90 so that its up at midnight
        _moonSource.transform.localRotation = Quaternion.Euler(90 - _moonLatitude, _moonLongitude, 0) * Quaternion.Euler(0, _sunAngle, 0);

        // Normalise current time to a value between 0 and 1
        _normalisedTime = _currentTime / 24f;

        // Set the intensity of the sun light based on the evaluated curve
        _sunSource.intensity = _sunMaxIntensity * _sunIntensityCurve.Evaluate(_normalisedTime);
        // Set the color temperature of the sun light based on the evaluated curve
        _sunSource.colorTemperature = 10000f * _sunTemperatureCurve.Evaluate(_normalisedTime); // In kelvin units

        // Set the intensity of the moon light based on the evaluated curve
        _moonSource.intensity = _moonMaxIntensity * _moonIntensityCurve.Evaluate(_normalisedTime);
    }

    void CheckActiveLightSource()
    {
        // During day time
        if (_currentTime >= 6f && _currentTime < 18f) // Between 6 AM and 6 PM
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

        // Roughly during day time
        if (_currentTime >= 5f && _currentTime < 19f)
            _sunSource.gameObject.SetActive(true); // Enable sun
        else // Roughly during night time
            _sunSource.gameObject.SetActive(false); // Disable sun

        // Roughly during day time
        if (_currentTime >= 6f && _currentTime < 17f)
            _moonSource.gameObject.SetActive(false); // Disable moon
        else // Roughly during night time
            _moonSource.gameObject.SetActive(true); // Enable moon
    }
    #endregion
}
