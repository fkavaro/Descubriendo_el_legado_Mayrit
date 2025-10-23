// https://youtu.be/FjJJ_I9zqJo?si=COnrovYRfn-TG86U 
using System;
using UnityEngine;


public class Billboard : MonoBehaviour
{
    [Tooltip("Wether to fully follow camera or just in Y axis")]
    [SerializeField] bool freezeXZAxis = false;
    [Tooltip("Wether to scale based on distance to camera")]
    [SerializeField] bool dynamicScaling = true;
    [Tooltip("Wether to disappear when too far from camera")]
    [SerializeField] bool disappearWhenFar = false;

    [Header("Distance scaling")]
    [Tooltip("Min and max uniform scale: x = minimum scale, y = maximum scale")]
    [SerializeField] Vector2 scaleRange = new(0.5f, 3f);
    [Tooltip("Distance range: x = minimum distance, y = maximum distance. Passed max distance, object will be invisible.")]
    [SerializeField] Vector2 distanceRange = new(50f, 500f);

    void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;
        ApplyRotation(cam);
        ApplyScaling(cam);
        CheckIfTooFar(cam);
    }

    void ApplyRotation(Camera cam)
    {
        if (freezeXZAxis)
            transform.rotation = Quaternion.Euler(0f, cam.transform.rotation.eulerAngles.y, 0f);
        else
            transform.rotation = cam.transform.rotation;
    }

    void ApplyScaling(Camera cam)
    {
        if (!dynamicScaling) return;

        // Scale based on distance to camera: closer -> smaller, farther -> bigger
        float distance = Vector3.Distance(transform.position, cam.transform.position);

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

    void CheckIfTooFar(Camera cam)
    {
        if (!disappearWhenFar) return;

        float distance = Vector3.Distance(transform.position, cam.transform.position);
        float maxDistance = Mathf.Max(distanceRange.x, distanceRange.y);

        // Enable or disable the GameObject based on distance
        gameObject.SetActive(distance <= maxDistance);
    }
}
