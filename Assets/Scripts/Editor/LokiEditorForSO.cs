// For ScriptableObjects
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true), CanEditMultipleObjects]
public class LokiEditorForSO : LokiInspector.LokiEditorBase
{
}
