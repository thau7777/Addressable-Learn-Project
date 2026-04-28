using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class LokiToolWindow : EditorWindow
{
    private enum LokiTab { Introduction, Export, AnimationSets }
    private LokiTab _currentTab;

    private Vector2 _animScroll;

    [MenuItem("Tools/Loki Inspector")]
    public static void ShowWindow()
    {
        var window = GetWindow<LokiToolWindow>("Loki Inspector");
        window.minSize = new Vector2(400, 300);
    }


    private void OnGUI()
    {
        _currentTab = (LokiTab)GUILayout.Toolbar((int)_currentTab, new[] { "Introduction", "Export Package" });
        GUILayout.Space(10);

        switch (_currentTab)
        {
            case LokiTab.Introduction:
                DrawIntroductionTab();
                break;
            case LokiTab.Export:
                DrawExportTab();
                break;
        }
    }

    private void DrawIntroductionTab()
    {
        GUILayout.Label("Welcome to Loki Inspector", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("This tool allows you to enhance your Unity inspectors with custom attributes.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);
        GUILayout.Label("Features:", EditorStyles.boldLabel);
        GUILayout.Label("- Conditional display (ShowIf, ShowIfEnumValue)");
        GUILayout.Label("- Grouping (FoldoutGroup, TabGroup)");
        GUILayout.Label("- Field customization (LabelText, MinMaxSlider)");
        GUILayout.Space(10);
        GUILayout.Label("Made by Hậu", EditorStyles.centeredGreyMiniLabel);
    }

    private void DrawExportTab()
    {
        GUILayout.Label("Export Loki Inspector Package", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("This will export everything inside the 'Loki Inspector' folder.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Export Loki Inspector", GUILayout.Height(30)))
        {
            string exportPath = EditorUtility.SaveFilePanel("Export Loki Inspector", "", "LokiInspector.unitypackage", "unitypackage");
            if (!string.IsNullOrEmpty(exportPath))
            {
                AssetDatabase.ExportPackage("Assets/Loki Inspector", exportPath, ExportPackageOptions.Recurse);
                EditorUtility.DisplayDialog("Export Complete", "Loki Inspector package has been exported successfully!", "OK");
            }
        }
    }

    
}
