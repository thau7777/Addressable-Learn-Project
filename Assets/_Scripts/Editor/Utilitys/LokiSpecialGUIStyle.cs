using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace LokiInspector {

    public static class LokiSpecialGUIStyle
    {
        public static GUIStyle GetFoldoutStyle()
        {
            var style = new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 26,
                padding = new RectOffset(20, 0, 4, 3)
            };

            return style;
        }


    }
}


