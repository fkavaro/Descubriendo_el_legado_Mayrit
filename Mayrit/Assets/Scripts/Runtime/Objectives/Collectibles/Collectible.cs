using UnityEngine;

public class Collectible : AObjective<Collectible, CollectibleSO>
{
    #region DEBUG GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, _colliderRadius);

#if UNITY_EDITOR
        if (_data != null)
            UnityEditor.Handles.Label(transform.position + Vector3.up * (_colliderRadius + 1f),
            string.IsNullOrEmpty(_data.Data.Header) ? name : _data.Data.Header);
#endif
    }
    #endregion
}