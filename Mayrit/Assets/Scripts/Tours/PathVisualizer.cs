using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathVisualizer
{
    #region PROPERTIES
    readonly LineRenderer _lineRenderer;
    readonly bool _useNavMesh;
    readonly float _lineWidth;
    readonly int _maxCorners;

    Transform _player, _nextPOI;
    Tour _currentTour;
    #endregion

    #region CONSTRUCTOR
    public PathVisualizer(LineRenderer lineRenderer, float lineWidth, bool useNavMesh, int maxCorners)
    {
        _lineRenderer = lineRenderer;
        _lineWidth = lineWidth;
        _useNavMesh = useNavMesh;
        _maxCorners = maxCorners;
    }
    #endregion

    #region PUBLIC METHODS
    public void Initialize()
    {
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;

        ConfigureLineRenderer();
    }

    public void UpdatePath()
    {
        // If no target POI or player, clear the line
        if (_nextPOI == null || _player == null)
        {
            if (_nextPOI == null)
                Debug.LogWarning("PathVisualizer: Clear path - no POI target");
            if (_player == null)
                Debug.LogWarning("PathVisualizer: Clear path - no playable character");

            Clear();
            return;
        }

        DrawPath(_player.position, _nextPOI.position);
    }

    public void Deinitialize()
    {
        ProgressManager.ExistingInstance.OnMilestoneChangedEvent -= OnMilestoneChanged;

        DetachFromTour(_currentTour);
    }

    public void Clear()
    {
        if (_lineRenderer == null) return;
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }
    #endregion

    #region PRIVATE METHODS
    void ConfigureLineRenderer()
    {
        if (_lineRenderer == null) return;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.widthMultiplier = _lineWidth;
    }

    void DrawPath(Vector3 start, Vector3 end)
    {
        List<Vector3> corners = new();

        if (_useNavMesh)
        {
            NavMeshPath path = new();
            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path) && path.corners != null && path.corners.Length > 0)
            {
                corners.AddRange(path.corners);
            }
        }

        if (corners.Count == 0)
        {
            corners.Add(start);
            corners.Add(end);
        }

        // Safety cap
        if (corners.Count > _maxCorners)
        {
            corners.RemoveRange(_maxCorners, corners.Count - _maxCorners);
        }

        if (_lineRenderer == null) return;
        _lineRenderer.positionCount = corners.Count;
        for (int i = 0; i < corners.Count; ++i)
            _lineRenderer.SetPosition(i, corners[i]);

        _lineRenderer.enabled = true;
    }

    void AttachToTour(Tour tour)
    {
        if (_currentTour == tour) return;

        DetachFromTour(_currentTour);

        _currentTour = tour;
        _currentTour.OnNextPOIChangeEvent += OnNextPOIChange;
    }

    void DetachFromTour(Tour tour)
    {
        if (tour == null) return;

        tour.OnNextPOIChangeEvent -= OnNextPOIChange;
        _nextPOI = null;
        _currentTour = null;
    }
    #endregion

    #region EVENT METHODS
    void OnMilestoneChanged(MilestoneMapping milestoneMapping)
    {
        _player = milestoneMapping.PlayableCharacter.transform;
        AttachToTour(milestoneMapping.Tour);
    }

    void OnNextPOIChange(PointOfInterest poi)
    {
        _nextPOI = poi.transform;
    }
    #endregion
}
