//! XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//! DISCARDED XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//! XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Draws a navigation path between the player and the next Point Of Interest (POI) of the tour.
/// </summary>
public class PathVisualizer
{
    #region FIELDS
    // Configuration
    readonly LineRenderer _lineRenderer;
    readonly float _pointSpacing;           // Distance between sample points along path segments
    readonly float _endpointSnapDistance;   // Max distance to snap player/POI to NavMesh
    readonly float _terrainProjectionDistance; // Max distance to project points down to NavMesh terrain
    readonly float _heightOffset;           // Vertical offset to lift line above ground (prevents z-fighting)
    readonly int _maxPointCount;            // Hard limit on total points for performance
    readonly float _maxTrailLength;         // Maximum distance from start to render (trail cutoff)

    // Runtime state
    Transform _nextPOI;

    // Dependency Injection
    readonly TourManager _tourManager;
    #endregion

    #region CONSTRUCTOR
    public PathVisualizer(
        LineRenderer lineRenderer,
        float pointSpacing,
        float endpointSnapDistance,
        float terrainProjectionDistance,
        float heightOffset,
        int maxPointCount,
        float maxTrailLength)
    {
        _lineRenderer = lineRenderer;
        _pointSpacing = Mathf.Max(0.01f, pointSpacing);
        _endpointSnapDistance = Mathf.Max(0.01f, endpointSnapDistance);
        _terrainProjectionDistance = Mathf.Max(0.01f, terrainProjectionDistance);
        _heightOffset = heightOffset;
        _maxPointCount = Mathf.Max(16, maxPointCount);
        _maxTrailLength = Mathf.Max(1f, maxTrailLength);

        // Get dependencies from Service Locator
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Subscribe to milestone events and prepare the LineRenderer.
    /// </summary>
    public void Initialize()
    {
        _tourManager.NextPOIChangeEvent += OnNextPOIChange;

        if (_lineRenderer == null)
            return;

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    /// <summary>
    /// Unsubscribe and detach from current tour.
    /// </summary>
    public void Deinitialize()
    {
        _tourManager.NextPOIChangeEvent -= OnNextPOIChange;
    }

    /// <summary>
    /// Clear the LineRenderer and hide it.
    /// </summary>
    public void Clear()
    {
        if (_lineRenderer == null || !_lineRenderer.enabled)
            return;

        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    /// <summary>
    /// Update the visualized path between player and next POI (if available).
    /// </summary>
    public void UpdatePath(Transform player)
    {
        if (_nextPOI == null || player == null)
        {
            if (_lineRenderer != null && _lineRenderer.enabled)
            {
                if (_nextPOI == null)
                    Debug.LogWarning("PathVisualizer: Clear path - no POI target");
                else
                    Debug.LogWarning("PathVisualizer: Clear path - no playable character");
            }

            Debug.LogWarning("  [PathVisualizer] No next POI or player transform available, clearing path.");

            Clear();
            return;
        }

        // Draw path from player to next POI
        DrawPath(player.position, _nextPOI.position);
    }
    #endregion

    #region PATH SAMPLING
    // Draw a path between start and end points using NavMesh.
    void DrawPath(Vector3 start, Vector3 end)
    {
        if (_lineRenderer == null)
            return;

        // Try to snap start and end to NavMesh
        // No path if either cannot be snapped
        if (!TryGetSnappedEndpoints(start, end, out Vector3 startPos, out Vector3 endPos))
        {
            Debug.LogWarning("PathVisualizer: Start or end not on NavMesh (or too far). Clearing path.");
            Clear();
            return;
        }

        NavMeshPath navPath = new();
        bool pathFound = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, navPath);

        // No path found
        if (!pathFound || navPath.corners == null || navPath.corners.Length == 0)
        {
            Debug.LogWarning("PathVisualizer: No path found between start and end. Clearing path.");
            Clear();
            return;
        }

        // Build sample points along the path, starting from player position
        List<Vector3> points = BuildSamplePoints(navPath.corners, start);

        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("PathVisualizer: No sample points generated for path. Clearing path.");
            Clear();
            return;
        }

        // Reduce point count if exceeding performance limit
        if (points.Count > _maxPointCount)
            points = Downsample(points, _maxPointCount);

        ApplyToLineRenderer(points);
    }
    #endregion

    #region SAMPLING HELPERS
    /// <summary>
    /// Attempts to snap both player and POI positions to the nearest NavMesh surface.
    /// </summary>
    bool TryGetSnappedEndpoints(Vector3 playerPos, Vector3 poiPos, out Vector3 snappedStart, out Vector3 snappedEnd)
    {
        snappedStart = playerPos;
        snappedEnd = poiPos;

        bool playerOnNavMesh = NavMesh.SamplePosition(playerPos, out NavMeshHit startHit, _endpointSnapDistance, NavMesh.AllAreas);
        bool poiOnNavMesh = NavMesh.SamplePosition(poiPos, out NavMeshHit endHit, _endpointSnapDistance, NavMesh.AllAreas);

        if (!playerOnNavMesh || !poiOnNavMesh)
            return false;

        snappedStart = startHit.position;
        snappedEnd = endHit.position;
        return true;
    }

    /// <summary>
    /// Generates densely sampled points along the path, projected to terrain.
    /// Trail always starts from player position and extends maxTrailLength forward.
    /// </summary>
    List<Vector3> BuildSamplePoints(Vector3[] pathCorners, Vector3 playerPosition)
    {
        List<Vector3> samplePoints = new(pathCorners.Length * 2);
        Vector3 trailOrigin = playerPosition;  // Use actual player position as origin
        float accumulatedDistance = 0f;  // Track distance along path from player

        // Sample points along each path segment
        for (int cornerIndex = 0; cornerIndex < pathCorners.Length - 1; cornerIndex++)
        {
            Vector3 segmentStart = pathCorners[cornerIndex];
            Vector3 segmentEnd = pathCorners[cornerIndex + 1];
            float segmentLength = Vector3.Distance(segmentStart, segmentEnd);
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(segmentLength / _pointSpacing));

            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                float segmentProgress = (float)sampleIndex / sampleCount;
                Vector3 interpolatedPoint = Vector3.Lerp(segmentStart, segmentEnd, segmentProgress);

                // Calculate distance along path from player
                if (samplePoints.Count > 0)
                    accumulatedDistance += Vector3.Distance(samplePoints[^1], interpolatedPoint);
                else
                    accumulatedDistance = Vector3.Distance(trailOrigin, interpolatedPoint);

                // Stop if trail length limit is exceeded
                if (accumulatedDistance > _maxTrailLength)
                    return samplePoints;

                // Project point down to terrain surface
                if (NavMesh.SamplePosition(interpolatedPoint, out NavMeshHit terrainHit, _terrainProjectionDistance, NavMesh.AllAreas))
                    interpolatedPoint = terrainHit.position;

                // Lift above terrain to prevent z-fighting
                interpolatedPoint.y += _heightOffset;

                // Add if not duplicate (avoid redundant points)
                if (samplePoints.Count == 0 || (samplePoints[^1] - interpolatedPoint).sqrMagnitude > 0.0001f)
                    samplePoints.Add(interpolatedPoint);
            }
        }

        // Try to add final corner if within trail length
        Vector3 finalCorner = pathCorners[^1];
        if (samplePoints.Count > 0)
            accumulatedDistance += Vector3.Distance(samplePoints[^1], finalCorner);

        if (accumulatedDistance <= _maxTrailLength)
        {
            if (NavMesh.SamplePosition(finalCorner, out NavMeshHit terrainHit, _terrainProjectionDistance, NavMesh.AllAreas))
                finalCorner = terrainHit.position;

            finalCorner.y += _heightOffset;
            if (samplePoints.Count == 0 || (samplePoints[^1] - finalCorner).sqrMagnitude > 0.0001f)
                samplePoints.Add(finalCorner);
        }

        return samplePoints;
    }

    /// <summary>
    /// Reduces point count by uniform sampling when exceeding performance limit.
    /// Always preserves the final point to maintain trail endpoint.
    /// </summary>
    List<Vector3> Downsample(List<Vector3> fullPoints, int targetPointCount)
    {
        List<Vector3> reducedPoints = new(targetPointCount + 1);
        int skipInterval = Mathf.CeilToInt((float)fullPoints.Count / targetPointCount);

        for (int i = 0; i < fullPoints.Count; i += skipInterval)
            reducedPoints.Add(fullPoints[i]);

        // Ensure final point is included
        if (reducedPoints[^1] != fullPoints[^1])
            reducedPoints.Add(fullPoints[^1]);

        return reducedPoints;
    }

    void ApplyToLineRenderer(List<Vector3> points)
    {
        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
        _lineRenderer.enabled = true;
    }
    #endregion

    #region EVENT METHODS
    void OnNextPOIChange(PointOfInterest poi)
    {
        _nextPOI = poi.transform;
    }
    #endregion
}

//! XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//! DISCARDED XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
//! XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
