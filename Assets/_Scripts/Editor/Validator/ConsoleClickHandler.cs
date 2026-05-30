#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Text.RegularExpressions;

namespace LokiInspector
{

    public static class ConsoleClickHandler
    {
        // Regex matches: "Assets/.../File.cs(12,1): ..."
        private static readonly Regex FileLineRegex =
            new Regex(@"(Assets\/.+\.cs)\((\d+),\d+\):", RegexOptions.Compiled);

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            string consoleMsg = GetSelectedConsoleMessage();
            if (string.IsNullOrEmpty(consoleMsg))
                return false;

            Match match = FileLineRegex.Match(consoleMsg);
            if (match.Success)
            {
                string path = match.Groups[1].Value;
                int lineNumber = int.Parse(match.Groups[2].Value);

                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, lineNumber);
                return true; // ✅ handled
            }

            return false; // let Unity handle other logs normally
        }

        // Extracts the currently selected Console log text
        private static string GetSelectedConsoleMessage()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var field = consoleWindowType.GetField("ms_ConsoleWindow",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var consoleInstance = field?.GetValue(null) as EditorWindow;
            if (consoleInstance == null)
                return null;

            if (EditorWindow.focusedWindow != consoleInstance)
                return null;

            var textField = consoleWindowType.GetField("m_ActiveText",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return textField?.GetValue(consoleInstance)?.ToString();
        }
    }

}
#endif
