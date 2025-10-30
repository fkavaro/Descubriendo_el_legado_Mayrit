using System;
using UnityEngine;
using UnityEditor.EditorTools;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spot : MonoBehaviour
{
    public bool _isOccupied = false;
    public bool _isRotationFixed = false;
    [Tooltip("Direction angle in degrees (Y axis rotation)")]
    public int _directionAngle = 0;

    [Header("Debug direction gizmo")]
    [SerializeField] float _gizmoLength = 2f;
    [SerializeField] float _gizmoThickness = 10f;

    // Direction in local coordinates (unit vector in the object's local space)
    [HideInInspector] public Vector3 DirectionVector => Quaternion.Euler(0f, _directionAngle, 0f) * Vector3.forward;

    // Optionally expose the world-space direction computed from the local direction
    //[HideInInspector] public Vector3 DirectionWorldVector => transform.rotation * DirectionVector;

    readonly object posLock = new();

    public void SetOccupied(bool occupied)
    {
        lock (posLock)
        {
            _isOccupied = occupied;
        }
    }

    public bool IsOccupied()
    {
        lock (posLock)
        {
            return _isOccupied;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!_isRotationFixed)
            return;

        Vector3 pos = transform.position;

        // Compute world-space rotation by applying the local Y rotation on top of the object's transform rotation.
        Quaternion rot = transform.rotation * Quaternion.Euler(0f, _directionAngle, 0f);
        Vector3 dir = rot * Vector3.forward;

        // Use Handles to draw a thicker anti-aliased line in the Scene view
        Handles.color = Color.yellow;
        Vector3 tip = pos + dir * _gizmoLength;
        Handles.DrawAAPolyLine(_gizmoThickness, pos, tip);

        // Draw a small arrow head using Handles (computed in the same local->world space)
        float headSize = Mathf.Max(0.05f, _gizmoLength * 0.3f);
        Vector3 right = rot * Vector3.right;
        Handles.DrawAAPolyLine(_gizmoThickness, tip, tip - dir * headSize + right * headSize);
        Handles.DrawAAPolyLine(_gizmoThickness, tip, tip - dir * headSize - right * headSize);
    }
#endif
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Spot))]
[UnityEditor.CanEditMultipleObjects]
class SpotEditor : UnityEditor.Editor
{
    UnityEditor.SerializedProperty isRotationFixedProp;
    UnityEditor.SerializedProperty directionAngleProp;
    UnityEditor.SerializedProperty isOccupiedProp;
    UnityEditor.SerializedProperty debugArrowLengthProp;
    UnityEditor.SerializedProperty debugArrowThicknessProp;

    void OnEnable()
    {
        isRotationFixedProp = serializedObject.FindProperty("_isRotationFixed");
        directionAngleProp = serializedObject.FindProperty("_directionAngle");
        isOccupiedProp = serializedObject.FindProperty("_isOccupied");
        debugArrowLengthProp = serializedObject.FindProperty("_gizmoLength");
        debugArrowThicknessProp = serializedObject.FindProperty("_gizmoThickness");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(isOccupiedProp);
        UnityEditor.EditorGUILayout.PropertyField(isRotationFixedProp);

        if (isRotationFixedProp.boolValue)
        {
            UnityEditor.EditorGUILayout.PropertyField(directionAngleProp);
            UnityEditor.EditorGUILayout.PropertyField(debugArrowLengthProp);
            UnityEditor.EditorGUILayout.PropertyField(debugArrowThicknessProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
