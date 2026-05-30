// For MonoBehaviours
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
public class LokiEditorForMono : LokiInspector.LokiEditorBase
{
}
