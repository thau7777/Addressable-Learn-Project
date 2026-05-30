using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LokiInspector {

    [CanEditMultipleObjects]
    public abstract class LokiEditorBase : Editor
    {
        private struct GroupKey : IEquatable<GroupKey>
        {
            public string Foldout;
            public string Tab;

            public GroupKey(string foldout, string tab)
            {
                Foldout = foldout ?? "";
                Tab = tab ?? "";
            }

            public bool Equals(GroupKey other) => Foldout == other.Foldout && Tab == other.Tab;
            public override int GetHashCode() => Foldout.GetHashCode() ^ Tab.GetHashCode();
        }


        public override void OnInspectorGUI()
        {
            // --- Edit Script Button (works for MonoBehaviour + ScriptableObject) ---
            MonoScript script = null;

            if (target is MonoBehaviour mb)
                script = MonoScript.FromMonoBehaviour(mb);
            else if (target is ScriptableObject so)
                script = MonoScript.FromScriptableObject(so);

            if (script != null)
            {
                if (GUILayout.Button("Edit Script", GUILayout.Height(22)))
                {
                    UnityEditor.AssetDatabase.OpenAsset(script);
                }
            }

            EditorGUILayout.Space();

            serializedObject.Update();

            DrawGroupedProperties(serializedObject, target.GetType(), "");

            DrawButtons();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGroupedProperties(SerializedObject serializedObj, Type type, string pathPrefix)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var normalProps = new List<SerializedProperty>();
            var foldGroups = new Dictionary<string, List<SerializedProperty>>();
            var tabGroups = new Dictionary<string, Dictionary<string, List<SerializedProperty>>>();
            var foldoutTabTracker = new Dictionary<string, string>();
            var foldoutErrors = new Dictionary<string, string>();

            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(HideInInspector))) continue;
                var prop = serializedObj.FindProperty(field.Name);
                if (prop == null || !ShouldShowField(field, serializedObj.targetObject)) continue;

                var tabAttr = field.GetCustomAttribute<TabGroupAttribute>();
                var foldAttr = field.GetCustomAttribute<FoldoutGroupAttribute>();

                string tab = tabAttr?.GroupName;
                string foldout = foldAttr?.GroupName ?? "";

                if (!string.IsNullOrEmpty(foldout))
                {
                    if (foldoutTabTracker.TryGetValue(foldout, out var existingTab))
                    {
                        if (existingTab != tab)
                        {
                            string fieldKey = pathPrefix + prop.name;
                            foldoutErrors[fieldKey] = $"FoldoutGroup \"{foldout}\" is used in multiple TabGroups (\"{existingTab}\" vs \"{tab}\"). This is not allowed.";
                            continue;
                        }
                    }
                    else
                    {
                        foldoutTabTracker[foldout] = tab;
                    }
                }

                if (tab != null)
                {
                    if (!tabGroups.ContainsKey(tab))
                        tabGroups[tab] = new Dictionary<string, List<SerializedProperty>>();

                    if (!tabGroups[tab].ContainsKey(foldout))
                        tabGroups[tab][foldout] = new List<SerializedProperty>();

                    tabGroups[tab][foldout].Add(prop);
                }
                else if (!string.IsNullOrEmpty(foldout))
                {
                    if (!foldGroups.ContainsKey(foldout))
                        foldGroups[foldout] = new List<SerializedProperty>();
                    foldGroups[foldout].Add(prop);
                }
                else
                {
                    normalProps.Add(prop);
                }
            }

            foreach (var kvp in foldoutErrors)
                EditorGUILayout.HelpBox(kvp.Value, MessageType.Error);

            foreach (var p in normalProps)
                DrawProperty(p, pathPrefix);

            foreach (var kvp in foldGroups)
            {
                string foldoutKey = GetFoldoutStateKey(serializedObj.targetObject, pathPrefix + kvp.Key);
                bool foldoutState = SessionState.GetBool(foldoutKey, true);

                foldoutState = EditorGUILayout.Foldout(foldoutState, kvp.Key, true);
                SessionState.SetBool(foldoutKey, foldoutState);

                if (foldoutState)
                {
                    EditorGUI.indentLevel++;
                    foreach (var p in kvp.Value)
                        DrawProperty(p, pathPrefix);
                    EditorGUI.indentLevel--;
                }
            }

            if (tabGroups.Count > 0)
            {
                var groupNames = tabGroups.Keys.ToArray();
                string tabKey = GetTabStateKey(serializedObj.targetObject, pathPrefix);

                int tabSelection = SessionState.GetInt(tabKey, 0);
                tabSelection = DrawWrappingToolbar(tabSelection, groupNames);
                tabSelection = Mathf.Clamp(tabSelection, 0, groupNames.Length - 1);
                SessionState.SetInt(tabKey, tabSelection);

                var activeGroup = groupNames[tabSelection];
                var foldoutMap = tabGroups[activeGroup];

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (var foldout in foldoutMap.Keys)
                {
                    if (string.IsNullOrEmpty(foldout))
                    {
                        foreach (var p in foldoutMap[foldout])
                            DrawProperty(p, pathPrefix);
                    }
                    else
                    {
                        string foldoutKey = GetFoldoutStateKey(serializedObj.targetObject, pathPrefix + foldout);
                        bool foldoutState = SessionState.GetBool(foldoutKey, true);

                        EditorGUILayout.BeginHorizontal(LokiSpecialGUIStyle.GetFoldoutStyle());
                        foldoutState = EditorGUILayout.Foldout(foldoutState, foldout, true);
                        SessionState.SetBool(foldoutKey, foldoutState);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        if (foldoutState)
                        {
                            EditorGUI.indentLevel++;
                            foreach (var p in foldoutMap[foldout])
                                DrawProperty(p, pathPrefix);
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private string GetFoldoutStateKey(UnityEngine.Object target, string foldoutName)
        {
            return $"LokiEditor_Foldout_{target.GetEntityId()}_{foldoutName}";
        }


        private string GetTabStateKey(UnityEngine.Object target, string pathPrefix)
        {
            return $"LokiEditor_Tab_{target.GetEntityId()}_{pathPrefix}";
        }


        private bool ShouldShowField(FieldInfo field, object targetObj)
        {
            var sif = field.GetCustomAttribute<ShowIfAttribute>();
            if (sif != null)
            {
                var cond = field.DeclaringType.GetField(sif.Condition, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (cond == null || cond.FieldType != typeof(bool)) return false;
                bool val = (bool)cond.GetValue(targetObj);
                if (sif.Invert) val = !val;
                if (!val) return false;
            }

            var sien = field.GetCustomAttribute<ShowIfEnumValueAttribute>();
            if (sien != null)
            {
                var ef = field.DeclaringType.GetField(sien.EnumFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ef == null || !ef.FieldType.IsEnum) return false;
                var cur = ef.GetValue(targetObj);
                if (!sien.TargetValues.Contains(cur)) return false;
            }

            return true;
        }

        private void DrawProperty(SerializedProperty prop, string pathPrefix)
        {
            var field = target.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                // For nested properties, try to find the field in the property path
                EditorGUILayout.PropertyField(prop, true);
                return;
            }

            // NEVER customize List / Array – let Unity handle it
            if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
            {
                EditorGUILayout.PropertyField(prop, true);
                return;
            }

            DrawSingleProperty(prop, field);

        }

        private void DrawSingleProperty(SerializedProperty prop, FieldInfo field)
        {
            bool prevEnabled = GUI.enabled;
            GUI.enabled = field.GetCustomAttribute<ReadOnlyAttribute>() == null;

            var labelAttr = field.GetCustomAttribute<LabelTextAttribute>();
            GUIContent labelContent = new(labelAttr?.Label ?? ObjectNames.NicifyVariableName(prop.name));

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            Color originalColor = GUI.color;
            bool hasCustomColor = false;
            if (field.GetCustomAttribute<RequiredAttribute>() != null)
            {
                bool isAssigned = prop.propertyType switch
                {
                    SerializedPropertyType.ObjectReference => prop.objectReferenceValue != null,
                    _ => prop.propertyType != SerializedPropertyType.Generic
                };

                if (!isAssigned)
                {
                    var errorStyle = new GUIStyle(EditorStyles.label);
                    errorStyle.normal.textColor = Color.red;
                    labelContent.text = labelContent.text + " *Required*";
                }
            }

            if (labelAttr != null)
            {
                if (!string.IsNullOrEmpty(labelAttr.ColorName) && TryParseColor(labelAttr.ColorName, out var labelColor))
                {
                    labelStyle.normal.textColor = labelColor;
                    GUI.color = labelColor;
                    hasCustomColor = true;
                }

                if (labelAttr.FontSize > 0)
                    labelStyle.fontSize = labelAttr.FontSize;

                labelStyle.fontStyle = labelAttr.FontStyle;
            }

            EditorGUILayout.PropertyField(prop, labelContent, true);

            if (hasCustomColor)
                GUI.color = originalColor;

            GUI.enabled = prevEnabled;
        }

        private void DrawButtons()
        {
            var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var m in methods)
            {
                if (m.GetCustomAttribute<ButtonAttribute>() != null && m.GetParameters().Length == 0)
                {
                    if (GUILayout.Button(ObjectNames.NicifyVariableName(m.Name)))
                        m.Invoke(target, null);
                }
            }
        }

        private int DrawWrappingToolbar(int selected, string[] options)
        {
            if (options == null || options.Length == 0) return selected;

            int newSelected = selected;
            float maxWidth = EditorGUIUtility.currentViewWidth - 40; // Account for margins
            float currentLineWidth = 0;
            int buttonIndex = 0;

            EditorGUILayout.BeginVertical();

            while (buttonIndex < options.Length)
            {
                EditorGUILayout.BeginHorizontal();

                while (buttonIndex < options.Length)
                {
                    // Calculate button width
                    GUIContent content = new GUIContent(options[buttonIndex]);
                    Vector2 size = GUI.skin.button.CalcSize(content);
                    float buttonWidth = Mathf.Max(size.x + 20, 80); // Min width of 80

                    // Check if button fits on current line
                    if (currentLineWidth + buttonWidth > maxWidth && currentLineWidth > 0)
                    {
                        // Start new line
                        break;
                    }

                    // Draw button with clear visual distinction for selected tab
                    bool isSelected = buttonIndex == selected;

                    Color originalBgColor = GUI.backgroundColor;
                    Color originalContentColor = GUI.contentColor;

                    if (isSelected)
                    {
                        // Make selected tab very obvious with bright blue background
                        GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 1f); // Bright blue
                        GUI.contentColor = Color.white;
                    }

                    GUIStyle style = new GUIStyle(GUI.skin.button);
                    if (isSelected)
                    {
                        style.fontStyle = FontStyle.Bold;
                        style.normal.textColor = Color.white;
                        style.hover.textColor = Color.white;
                        style.active.textColor = Color.white;
                    }

                    if (GUILayout.Button(options[buttonIndex], style, GUILayout.MinWidth(buttonWidth)))
                    {
                        newSelected = buttonIndex;
                    }

                    // Restore colors
                    GUI.backgroundColor = originalBgColor;
                    GUI.contentColor = originalContentColor;

                    currentLineWidth += buttonWidth + 2; // +2 for spacing
                    buttonIndex++;
                }

                EditorGUILayout.EndHorizontal();
                currentLineWidth = 0;
            }

            EditorGUILayout.EndVertical();

            return newSelected;
        }

        protected bool TryParseColor(string name, out Color color)
        {
            switch (name.ToLower())
            {
                case "red": color = Color.red; return true;
                case "green": color = Color.green; return true;
                case "blue": color = Color.blue; return true;
                case "yellow": color = Color.yellow; return true;
                case "cyan": color = Color.cyan; return true;
                case "magenta": color = Color.magenta; return true;
                case "white": color = Color.white; return true;
                case "black": color = Color.black; return true;
                case "gray":
                case "grey": color = Color.gray; return true;
                default:
                    color = default;
                    return false;
            }
        }
    }
}
