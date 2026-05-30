#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LokiInspector {

    [InitializeOnLoad]
    public static class ValidationPipeline
    {
        static ValidationPipeline()
        {
            // After scripts recompile / domain reload
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            // Before entering Play Mode
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        // === After Compile ===
        private static void OnAfterAssemblyReload()
        {
            RunValidators(logOnly: true);
        }

        // === Before Play ===
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!RunValidators(logOnly: false))
                {
                    Debug.LogError("❌ Play mode prevented due to validation errors.");
                    EditorApplication.isPlaying = false;
                }
            }
        }

        // === Core Validator Runner ===
        private static bool RunValidators(bool logOnly)
        {
            bool requiredOk = RequiredFieldValidator.ValidateAllRequiredFields(LogError);
            bool groupsOk = GroupValidator.ValidateGroups(LogError);

            return requiredOk && groupsOk;
        }

        // === Custom Error Logger with clickable line numbers ===
        private static void LogError(string message, string filePath, int lineNumber)
        {
            if (!string.IsNullOrEmpty(filePath) && lineNumber > 0)
            {
                // ✅ Compiler-style: "Assets/Path/File.cs(42,1): error Validation: ..."
                string formatted = $"{filePath}({lineNumber},1): error Validation: {message}";
                Debug.LogError(formatted);
            }
            else
            {
                Debug.LogError($"Validation Error: {message}");
            }
        }




    }
}

#endif
