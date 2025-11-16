using System;
using UnityEngine;
using UnityEditor.EditorTools;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spot : MonoBehaviour
{
    [SerializeField] bool _isOccupied = false;
    [SerializeField] bool _isRotationFixed = false;

    [Header("Debug direction gizmo")]
    [SerializeField] float _gizmoLength = 0.5f;
    [SerializeField] float _gizmoThickness = 20f;
    [SerializeField] Color _gizmoColor = Color.yellow;

    [HideInInspector] public Quaternion LocalDirection => Quaternion.Euler(0f, transform.rotation.y, 0f);
    [HideInInspector] public Quaternion WorldDirection => transform.rotation * LocalDirection;

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
        Quaternion rot = transform.rotation * Quaternion.Euler(0f, transform.rotation.y, 0f);
        Vector3 dir = rot * Vector3.forward;

        // Use Handles to draw a thicker anti-aliased line in the Scene view
        Handles.color = _gizmoColor;
        Vector3 tip = pos + dir * _gizmoLength;

        // Draw a small arrow head using Handles (computed in the same local->world space)
        float headSize = _gizmoLength;
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
    UnityEditor.SerializedProperty isOccupiedProp;
    UnityEditor.SerializedProperty debugArrowLengthProp;
    UnityEditor.SerializedProperty debugArrowThicknessProp;
    UnityEditor.SerializedProperty debugArrowColorProp;

    void OnEnable()
    {
        isRotationFixedProp = serializedObject.FindProperty("_isRotationFixed");
        isOccupiedProp = serializedObject.FindProperty("_isOccupied");
        debugArrowLengthProp = serializedObject.FindProperty("_gizmoLength");
        debugArrowThicknessProp = serializedObject.FindProperty("_gizmoThickness");
        debugArrowColorProp = serializedObject.FindProperty("_gizmoColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(isOccupiedProp);
        UnityEditor.EditorGUILayout.PropertyField(isRotationFixedProp);

        if (isRotationFixedProp.boolValue)
        {
            UnityEditor.EditorGUILayout.PropertyField(debugArrowLengthProp);
            UnityEditor.EditorGUILayout.PropertyField(debugArrowThicknessProp);
            UnityEditor.EditorGUILayout.PropertyField(debugArrowColorProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
