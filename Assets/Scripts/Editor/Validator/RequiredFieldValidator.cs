#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LokiInspector {

    public static class RequiredFieldValidator
    {
        public static bool ValidateAllRequiredFields(System.Action<string, string, int> logError)
        {
            bool allGood = true;

            // 1. Validate MonoBehaviours in the scene
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue; // skip destroyed refs
                if (behaviour.gameObject == null) continue; // skip broken behaviours

                if (!ValidateObjectFields(behaviour, behaviour.gameObject.name, logError))
                    allGood = false;
            }


            // 2. Validate ScriptableObject assets in the project
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so == null) continue;

                if (!ValidateObjectFields(so, path, logError))
                    allGood = false;
            }

            return allGood;
        }

        private static bool ValidateObjectFields(Object target, string context, System.Action<string, string, int> logError)
        {
            if (target == null)
                return true;

            bool allGood = true;

            var type = target.GetType();
            if (type == null)
                return true;

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<RequiredAttribute>() != null)
                {
                    var value = field.GetValue(target);
                    if (value == null || (value is Object obj && obj == null))
                    {
                        allGood = false;

                        if (TryGetFieldLocation(field, out var path, out var line))
                        {
                            logError?.Invoke(
                                $"[Required] field not assigned: {type.Name}.{field.Name} in '{context}'",
                                path,
                                line
                            );
                        }
                        else
                        {
                            logError?.Invoke(
                                $"[Required] field not assigned: {type.Name}.{field.Name} in '{context}' (location unknown)",
                                null,
                                0
                            );
                        }
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

            // Handle ScriptableObjects
            if (typeof(ScriptableObject).IsAssignableFrom(field.DeclaringType))
            {
                var tempSO = ScriptableObject.CreateInstance(field.DeclaringType) as ScriptableObject;
                if (tempSO != null)
                {
                    script = MonoScript.FromScriptableObject(tempSO);
                    Object.DestroyImmediate(tempSO);
                }
            }
            // Handle MonoBehaviours
            else if (typeof(MonoBehaviour).IsAssignableFrom(field.DeclaringType))
            {
                var tempGO = new GameObject();
                try
                {
                    var comp = tempGO.AddComponent(field.DeclaringType) as MonoBehaviour;
                    if (comp != null)
                        script = MonoScript.FromMonoBehaviour(comp);
                }
                finally
                {
                    Object.DestroyImmediate(tempGO);
                }
            }

            if (script == null)
                return false;

            path = AssetDatabase.GetAssetPath(script);

            var lines = script.text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                // safer matching (avoid comments/attributes picking up the field name)
                if (System.Text.RegularExpressions.Regex.IsMatch(lines[i], $@"\b{field.Name}\b"))
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
