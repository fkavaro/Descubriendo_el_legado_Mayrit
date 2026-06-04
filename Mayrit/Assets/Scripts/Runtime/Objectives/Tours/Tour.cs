using UnityEngine;

public class Tour : ObjectiveTrackerBase<Tour, TourStop, DataSO>
{
    [SerializeField] private DataSO _tourData;
    public DataSO TourData => _tourData;

    public TourStop LastStopInList => _objectives.Count > 0 ? _objectives[^1] : null;

    protected override void Awake()
    {
        base.Awake();

        // Only count stops with valid data for total count
        _totalCount = 0;
        foreach (var stop in _objectives)
        {
            if (stop.Data != null) _totalCount++;
        }
    }
}