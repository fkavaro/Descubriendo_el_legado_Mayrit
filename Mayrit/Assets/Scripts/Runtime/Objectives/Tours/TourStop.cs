using UnityEngine;
using Unity.Cinemachine;

public class TourStop : AObjective<TourStop, DataSO>
{
    public CinemachineCamera Camera => _camera;

    [Header("Tour Stop Settings")]
    [SerializeField] private CinemachineCamera _camera;

    void Update()
    {
        if (_model != null && _model.activeSelf)
            _model.transform.Rotate(Vector3.up, 50f * Time.deltaTime, Space.World);
    }

    #region DEBUG GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.color = _isReached ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, _colliderRadius);

#if UNITY_EDITOR
        if (_data != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_colliderRadius + 0.2f),
            string.IsNullOrEmpty(_data.Header) ? name : _data.Header);
#endif
    }
    #endregion
}

