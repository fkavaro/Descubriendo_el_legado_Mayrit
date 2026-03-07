// https://youtu.be/FjJJ_I9zqJo?si=COnrovYRfn-TG86U 
using System;
using UnityEngine;


public class Billboard : MonoBehaviour
{
    #region EDITOR PROPERTIES
    [Tooltip("Wether to fully follow camera or just in Y axis")]
    [SerializeField] bool _freezeXZAxis = false;
    [Tooltip("Wether to scale based on distance to camera")]
    [SerializeField] bool _dynamicScaling = true;

    [Header("Distance scaling")]
    [Tooltip("Min and max uniform scale: x = minimum scale, y = maximum scale")]
    [SerializeField] Vector2 scaleRange = new(1f, 10f);
    [Tooltip("Distance range: x = minimum distance, y = maximum distance. Passed max distance, object will be invisible.")]
    [SerializeField] Vector2 distanceRange = new(50f, 1000f);
    #endregion

    #region INTERNAL PROPERTIES
    protected Camera _mainCamera;
    protected bool _isTooFar;
    #endregion

    #region MONOBEHAVIOUR
    protected virtual void Start()
    {
        _mainCamera = Camera.main;
    }

    protected virtual void Update()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        if (_mainCamera == null) return;
        float distance = Vector3.Distance(transform.position, _mainCamera.transform.position);
        ApplyRotation();
        ApplyScaling(distance);
        CheckIfTooFar(distance);
    }
    #endregion

    #region PRIVATE METHODS
    void ApplyRotation()
    {
        if (_freezeXZAxis)
            transform.rotation = Quaternion.Euler(0f, _mainCamera.transform.rotation.eulerAngles.y, 0f);
        else
            transform.rotation = _mainCamera.transform.rotation;
    }

    void ApplyScaling(float distance)
    {
        if (!_dynamicScaling) return;

        // Ensure to treat x as min and y as max for distances and scales
        float minDistance = Mathf.Min(distanceRange.x, distanceRange.y);
        float maxDistance = Mathf.Max(distanceRange.x, distanceRange.y);
        float minScaleVal = Mathf.Min(scaleRange.x, scaleRange.y);
        float maxScaleVal = Mathf.Max(scaleRange.x, scaleRange.y);

        // Calculate t in [0,1] based on distance within min and max distance
        float t = Mathf.Approximately(maxDistance, minDistance) ? 0f : Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Get scale based on t
        float scaleUniform = Mathf.Lerp(minScaleVal, maxScaleVal, t);

        // Modify local scale uniformly (preserve original z scale if non-uniform)
        transform.localScale = new Vector3(scaleUniform, scaleUniform, transform.localScale.z);
    }

    void CheckIfTooFar(float distance)
    {
        float maxDistance = Mathf.Max(distanceRange.x, distanceRange.y);

        _isTooFar = distance > maxDistance;
    }
    #endregion
}
