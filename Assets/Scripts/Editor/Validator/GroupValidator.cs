#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace LokiInspector
{
    public static class GroupValidator
    {
        public static bool ValidateGroups(System.Action<string, string, int> logError)
        {
            bool allGood = true;

            // 1. Check MonoBehaviours in scene
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue; // skip destroyed refs
                if (behaviour.gameObject == null) continue; // skip broken behaviours

                if (!ValidateObjectGroups(behaviour, behaviour.gameObject.name, logError))
                    allGood = false;
            }

            // 2. Check ScriptableObjects in project
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;

                if (!ValidateObjectGroups(so, path, logError))
                    allGood = false;
            }

            return allGood;
        }

        private static bool ValidateObjectGroups(Object target, string context, System.Action<string, string, int> logError)
        {
            bool allGood = true;
            var type = target.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var foldoutTabTracker = new Dictionary<string, string>();

            foreach (var field in fields)
            {
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
                            allGood = false;

                            if (TryGetFieldLocation(field, out var path, out var line))
                            {
                                logError?.Invoke(
                                    $"FoldoutGroup \"{foldout}\" is used in multiple TabGroups (\"{existingTab}\" vs \"{tab}\") in '{context}'",
                                    path,
                                    line
                                );
                            }
                        }
                    }
                    else
                    {
                        foldoutTabTracker[foldout] = tab;
                    }
                }
            }

            return allGood;
        }

        private static bool TryGetFieldLocation(FieldInfo field, out string path, out int lineNumber)
        {
            path = null;
            lineNumber = 0;

            MonoScript script = null;

            // Try MonoBehaviour
            if (typeof(MonoBehaviour).IsAssignableFrom(field.DeclaringType))
            {
                var tempGO = new GameObject();
                try
                {
                    var comp = tempGO.AddComponent(field.DeclaringType) as MonoBehaviour;
                    script = MonoScript.FromMonoBehaviour(comp);
                }
                finally
                {
                    Object.DestroyImmediate(tempGO);
                }
            }
            // Try ScriptableObject
            else if (typeof(ScriptableObject).IsAssignableFrom(field.DeclaringType))
            {
                var so = ScriptableObject.CreateInstance(field.DeclaringType);
                script = MonoScript.FromScriptableObject(so);
                Object.DestroyImmediate(so);
            }

            if (script == null)
                return false;

            path = AssetDatabase.GetAssetPath(script);
            var lines = script.text.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string l = lines[i].Trim();

                // Match either the attribute line or the field declaration
                if (l.Contains("[" + nameof(TabGroupAttribute)) ||
                    l.Contains("[" + nameof(FoldoutGroupAttribute)) ||
                    l.Contains(field.Name))
                {
                    lineNumber = i + 1;
                    break;
                }
            }

            return !string.IsNullOrEmpty(path) && lineNumber > 0;
        }


    }
}
#endif

