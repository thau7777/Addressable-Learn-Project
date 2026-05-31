using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LokiInspector
{
    // Renders a concrete-type selector for [SerializeReference] managed-reference fields/list elements.
    // Honored even when LokiEditorBase draws the parent via PropertyField(includeChildren), and applied
    // per-element when the attribute sits on a List<T>.
    [CustomPropertyDrawer(typeof(SerializeReferenceDropdownAttribute))]
    public class SerializeReferenceDropdownDrawer : PropertyDrawer
    {
        // Keyed by managedReferenceFieldTypename; cleared automatically on domain reload.
        private static readonly Dictionary<string, List<Type>> _typeCache = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return EditorGUI.GetPropertyHeight(property, label, true);

            float height = EditorGUIUtility.singleLineHeight;
            if (property.managedReferenceValue != null && property.isExpanded)
            {
                foreach (var child in GetChildren(property))
                    height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            Rect labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            if (property.managedReferenceValue != null)
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            else
                EditorGUI.LabelField(labelRect, label);

            Rect buttonRect = new Rect(line.x + EditorGUIUtility.labelWidth, line.y,
                Mathf.Max(40f, line.width - EditorGUIUtility.labelWidth), line.height);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(NiceTypeName(property.managedReferenceFullTypename)), FocusType.Keyboard))
                ShowTypeMenu(property);

            if (property.managedReferenceValue != null && property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = line.yMax + EditorGUIUtility.standardVerticalSpacing;
                foreach (var child in GetChildren(property))
                {
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);
                    y += h + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private static IEnumerable<SerializedProperty> GetChildren(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enter = true;
            while (iterator.NextVisible(enter) && !SerializedProperty.EqualContents(iterator, end))
            {
                enter = false;
                yield return iterator.Copy();
            }
        }

        private void ShowTypeMenu(SerializedProperty property)
        {
            // The menu callback runs deferred — capture the path and re-resolve to avoid a stale property.
            SerializedObject so = property.serializedObject;
            string path = property.propertyPath;
            Type currentType = property.managedReferenceValue?.GetType();

            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("(null)"), currentType == null, () =>
            {
                SerializedProperty p = so.FindProperty(path);
                p.managedReferenceValue = null;
                so.ApplyModifiedProperties();
            });

            foreach (Type t in GetAssignableTypes(property.managedReferenceFieldTypename))
            {
                Type captured = t;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(t.Name)), t == currentType, () =>
                {
                    SerializedProperty p = so.FindProperty(path);
                    p.managedReferenceValue = Activator.CreateInstance(captured);
                    p.isExpanded = true;
                    so.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        private static List<Type> GetAssignableTypes(string fieldTypename)
        {
            if (_typeCache.TryGetValue(fieldTypename, out var cached)) return cached;

            Type baseType = ResolveType(fieldTypename);
            List<Type> result = new();
            if (baseType != null)
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); }
                    catch { continue; }
                    foreach (Type t in types)
                    {
                        if (t.IsAbstract || t.IsInterface || t.IsGenericTypeDefinition) continue;
                        if (!baseType.IsAssignableFrom(t)) continue;
                        if (typeof(UnityEngine.Object).IsAssignableFrom(t)) continue;
                        if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                        result.Add(t);
                    }
                }
                result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            }
            _typeCache[fieldTypename] = result;
            return result;
        }

        // managedReference*Typename format is "AssemblyName Namespace.TypeName".
        private static Type ResolveType(string managedReferenceTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceTypename)) return null;
            string[] parts = managedReferenceTypename.Split(' ');
            if (parts.Length != 2) return null;
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }

        private static string NiceTypeName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return "(Select type)";
            string[] parts = managedReferenceFullTypename.Split(' ');
            string full = parts.Length == 2 ? parts[1] : managedReferenceFullTypename;
            int dot = full.LastIndexOf('.');
            return ObjectNames.NicifyVariableName(dot >= 0 ? full.Substring(dot + 1) : full);
        }
    }
}
