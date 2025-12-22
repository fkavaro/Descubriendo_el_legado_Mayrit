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
    readonly float _sampleSpacing;       // Distance between samples along segments
    readonly float _sampleDistance;      // Max distance to snap start/end to NavMesh
    readonly float _projSampleDistance;  // Max distance to project samples to NavMesh
    readonly float _renderYOffset;       // Vertical offset to lift the rendered line above navmesh
    readonly int _maxPoints;             // Maximum number of points allowed on the LineRenderer

    // Runtime state
    Transform _player;
    Transform _nextPOI;

    // Dependency Injection
    readonly ProgressManager _progressManager;
    readonly TourManager _tourManager;
    #endregion

    #region CONSTRUCTOR
    public PathVisualizer(
        LineRenderer lineRenderer,
        float sampleSpacing,
        float sampleDistance,
        float projSampleDistance,
        float renderYOffset,
        int maxPoints)
    {
        _lineRenderer = lineRenderer;
        _sampleSpacing = Mathf.Max(0.01f, sampleSpacing);
        _sampleDistance = Mathf.Max(0.01f, sampleDistance);
        _projSampleDistance = Mathf.Max(0.01f, projSampleDistance);
        _renderYOffset = renderYOffset;
        _maxPoints = Mathf.Max(16, maxPoints);

        // Get dependencies from Service Locator
        _progressManager = ServiceLocator.Instance.Get<ProgressManager>();
        _tourManager = ServiceLocator.Instance.Get<TourManager>();
    }
    #endregion

    #region PUBLIC METHODS
    /// <summary>
    /// Subscribe to milestone events and prepare the LineRenderer.
    /// </summary>
    public void Initialize()
    {
        _progressManager.MilestoneChangedEvent += OnMilestoneChanged;
        _tourManager.OnTourNextPOIChangeEvent += OnNextPOIChange;

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
        _progressManager.MilestoneChangedEvent -= OnMilestoneChanged;
        _tourManager.OnTourNextPOIChangeEvent -= OnNextPOIChange;
    }

    /// <summary>
    /// Clear the LineRenderer and hide it.
    /// </summary>
    public void Clear()
    {
        if (_lineRenderer == null)
            return;

        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    /// <summary>
    /// Update the visualized path between player and next POI (if available).
    /// </summary>
    public void UpdatePath()
    {
        if (_nextPOI == null || _player == null)
        {
            if (_lineRenderer != null && _lineRenderer.enabled)
            {
                if (_nextPOI == null)
                    Debug.LogWarning("PathVisualizer: Clear path - no POI target");
                else
                    Debug.LogWarning("PathVisualizer: Clear path - no playable character");
            }

            Clear();
            return;
        }

        // Draw path from player to next POI
        DrawPath(_player.position, _nextPOI.position);
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
            Clear();
            return;
        }

        // Build sample points along the path
        List<Vector3> points = BuildSamplePoints(navPath.corners);

        if (points == null || points.Count == 0)
        {
            Clear();
            return;
        }

        // Downsample if exceeding max points
        if (points.Count > _maxPoints)
            points = Downsample(points, _maxPoints);

        ApplyToLineRenderer(points);
    }
    #endregion

    #region SAMPLING HELPERS
    // Try to snap the start and end points to the NavMesh within configured distance.
    bool TryGetSnappedEndpoints(Vector3 start, Vector3 end, out Vector3 startPos, out Vector3 endPos)
    {
        startPos = start;
        endPos = end;

        bool hasStart = NavMesh.SamplePosition(start, out NavMeshHit startHit, _sampleDistance, NavMesh.AllAreas);
        bool hasEnd = NavMesh.SamplePosition(end, out NavMeshHit endHit, _sampleDistance, NavMesh.AllAreas);

        if (!hasStart || !hasEnd)
            return false;

        startPos = startHit.position;
        endPos = endHit.position;
        return true;
    }

    // Build dense sample points along the path corners, projecting samples to NavMesh and applying Y offset.
    List<Vector3> BuildSamplePoints(Vector3[] baseCorners)
    {
        var points = new List<Vector3>(baseCorners.Length * 2);

        for (int i = 0; i < baseCorners.Length - 1; i++)
        {
            Vector3 a = baseCorners[i];
            Vector3 b = baseCorners[i + 1];
            float segLen = Vector3.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.CeilToInt(segLen / _sampleSpacing));

            for (int s = 0; s < steps; s++)
            {
                float t = (float)s / steps;
                Vector3 samplePoint = Vector3.Lerp(a, b, t);

                if (NavMesh.SamplePosition(samplePoint, out NavMeshHit hit, _projSampleDistance, NavMesh.AllAreas))
                    samplePoint = hit.position;

                samplePoint.y += _renderYOffset;

                if (points.Count == 0 || (points[^1] - samplePoint).sqrMagnitude > 0.0001f)
                    points.Add(samplePoint);
            }
        }

        // Add last corner (ensure projection)
        Vector3 last = baseCorners[^1];
        if (NavMesh.SamplePosition(last, out NavMeshHit lastHit, _projSampleDistance, NavMesh.AllAreas))
            last = lastHit.position;

        last.y += _renderYOffset;
        if (points.Count == 0 || (points[^1] - last).sqrMagnitude > 0.0001f)
            points.Add(last);

        return points;
    }

    // Uniformly downsample if there are more points than allowed by _maxPoints.
    List<Vector3> Downsample(List<Vector3> points, int maxPoints)
    {
        List<Vector3> sampled = new(maxPoints + 1);
        int step = Mathf.CeilToInt((float)points.Count / maxPoints);
        for (int i = 0; i < points.Count; i += step)
            sampled.Add(points[i]);

        if (sampled[^1] != points[^1])
            sampled.Add(points[^1]);

        return sampled;
    }

    void ApplyToLineRenderer(List<Vector3> points)
    {
        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
        _lineRenderer.enabled = true;
    }
    #endregion

    #region EVENT METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        _player = milestoneMapping.PlayableCharacter.transform;
    }

    void OnNextPOIChange(PointOfInterest poi)
    {
        _nextPOI = poi.transform;
    }
    #endregion
}
