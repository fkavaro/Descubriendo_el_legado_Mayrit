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
    readonly Gradient _colorGradient;

    PlayableCharacter _playableCharacter;
    PointOfInterest _nextPOI;
    Vector3 _playerPos, _nextPOIPos;
    Tour _currentTour;
    #endregion

    #region CONSTRUCTOR
    public PathVisualizer(LineRenderer lineRenderer, Gradient colorGradient, float lineWidth, bool useNavMesh, int maxCorners)
    {
        _lineRenderer = lineRenderer;
        _colorGradient = colorGradient;
        _lineWidth = lineWidth;
        _useNavMesh = useNavMesh;
        _maxCorners = maxCorners;
    }
    #endregion

    #region PUBLIC METHODS
    public void Initialize()
    {
        ProgressManager.Instance.OnMilestoneChangedEvent += OnMilestoneChanged;
        AttachToTour(ProgressManager.Instance.CurrentMilestoneMapping?.Tour);

        ConfigureLineRenderer();
    }

    public void UpdatePath()
    {
        // If no target POI or player, clear the line
        if (_nextPOI == null || _playableCharacter == null)
        {
            Clear();
            return;
        }

        _playerPos = _playableCharacter.transform.position;
        _nextPOIPos = _nextPOI.transform.position;

        DrawPath(_playerPos, _nextPOIPos);
    }

    public void Deinitialize()
    {
        if (ProgressManager.Instance != null)
            ProgressManager.Instance.OnMilestoneChangedEvent -= OnMilestoneChanged;

        DetachFromTour(_currentTour);
    }
    #endregion

    #region PRIVATE METHODS
    void ConfigureLineRenderer()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.useWorldSpace = true;
        _lineRenderer.widthMultiplier = _lineWidth;
        _lineRenderer.colorGradient = _colorGradient;
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

    void Clear()
    {
        if (_lineRenderer == null) return;
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    void AttachToTour(Tour tour)
    {
        if (_currentTour == tour) return;

        DetachFromTour(_currentTour);

        _currentTour = tour;
        if (_currentTour != null)
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
        AttachToTour(milestoneMapping?.Tour);
        _playableCharacter = milestoneMapping?.PlayableCharacter;
    }

    void OnNextPOIChange(PointOfInterest poi)
    {
        _nextPOI = poi;
    }
    #endregion
}
