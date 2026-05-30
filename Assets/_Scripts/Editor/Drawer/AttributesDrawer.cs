// File: Assets/MiniOdin/Editor/AttributeDrawers.cs
// Place this in an Editor folder - handles ALL your custom attributes

using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LokiInspector {

    // ============================================
    // MinMaxSlider Drawer
    // ============================================
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Vector2)
            {
                EditorGUI.LabelField(position, label.text, "Use MinMaxSlider with Vector2.");
                return;
            }

            MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;
            Vector2 range = property.vector2Value;

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float fieldWidth = 50f;
            float spacing = 5f;
            float sliderWidth = position.width - (fieldWidth * 2) - (spacing * 2);

            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + spacing, position.y, sliderWidth, position.height);
            Rect maxRect = new Rect(position.x + fieldWidth + spacing + sliderWidth + spacing, position.y, fieldWidth, position.height);

            range.x = EditorGUI.FloatField(minRect, range.x);
            EditorGUI.MinMaxSlider(sliderRect, ref range.x, ref range.y, attr.Min, attr.Max);
            range.y = EditorGUI.FloatField(maxRect, range.y);

            if (attr.EnforceMinMaxDistance > 0f && (range.y - range.x) < attr.EnforceMinMaxDistance)
                range.y = range.x + attr.EnforceMinMaxDistance;

            range.x = Mathf.Clamp(range.x, attr.Min, attr.Max);
            range.y = Mathf.Clamp(range.y, attr.Min, attr.Max);

            property.vector2Value = range;

            EditorGUI.EndProperty();
        }
    }

    // ============================================
    // ReadOnly Drawer
    // ============================================
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    // ============================================
    // LabelText Drawer
    // ============================================
    [CustomPropertyDrawer(typeof(LabelTextAttribute))]
    public class LabelTextDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelTextAttribute attr = (LabelTextAttribute)attribute;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);

            if (!string.IsNullOrEmpty(attr.ColorName) && TryParseColor(attr.ColorName, out var color))
            {
                labelStyle.normal.textColor = color;
            }

            if (attr.FontSize > 0)
                labelStyle.fontSize = attr.FontSize;

            labelStyle.fontStyle = attr.FontStyle;

            // Use custom label text or fall back to original
            string labelText = string.IsNullOrEmpty(attr.Label) ? label.text : attr.Label;
            GUIContent customLabel = new GUIContent(labelText, label.tooltip);

            // Calculate proper label width for the custom text
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            Vector2 labelSize = labelStyle.CalcSize(customLabel);
            float neededLabelWidth = Mathf.Max(labelSize.x + 4f, originalLabelWidth);

            EditorGUIUtility.labelWidth = neededLabelWidth;

            // Store original color
            Color originalColor = labelStyle.normal.textColor;

            // Temporarily override label style
            var tempStyle = EditorStyles.label;
            if (!string.IsNullOrEmpty(attr.ColorName))
            {
                GUI.contentColor = originalColor;
            }

            // Draw the property with custom label
            EditorGUI.PropertyField(position, property, customLabel, true);

            // Restore
            GUI.contentColor = Color.white;
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool TryParseColor(string name, out Color color)
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
                case "gray": color = Color.gray; return true;
                case "orange": color = Color.orange; return true;
                default: color = default; return false;
            }
        }
    }

    // ============================================
    // Required Drawer
    // ============================================
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isAssigned = property.propertyType switch
            {
                SerializedPropertyType.ObjectReference => property.objectReferenceValue != null,
                SerializedPropertyType.String => !string.IsNullOrEmpty(property.stringValue),
                _ => true
            };

            if (!isAssigned)
            {
                GUIStyle errorStyle = new GUIStyle(EditorStyles.label);
                errorStyle.normal.textColor = Color.red;
                label.text = label.text + " *Required*";

                // Draw label in red
                var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                EditorGUI.LabelField(labelRect, label, errorStyle);

                // Draw property
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    // ============================================
    // ShowIf Drawer (Conditional Visibility)
    // ============================================
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attr = (ShowIfAttribute)attribute;

            if (ShouldShow(property, attr))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attr = (ShowIfAttribute)attribute;

            if (ShouldShow(property, attr))
                return EditorGUI.GetPropertyHeight(property, label, true);

            return -EditorGUIUtility.standardVerticalSpacing; // Hide completely
        }

        private bool ShouldShow(SerializedProperty property, ShowIfAttribute attr)
        {
            // Find the condition property
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, attr.Condition);
            SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

            if (conditionProp == null || conditionProp.propertyType != SerializedPropertyType.Boolean)
                return true; // Show by default if condition not found

            bool show = conditionProp.boolValue;
            return attr.Invert ? !show : show;
        }
    }

    // ============================================
    // ShowIfEnumValue Drawer
    // ============================================
    [CustomPropertyDrawer(typeof(ShowIfEnumValueAttribute))]
    public class ShowIfEnumValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfEnumValueAttribute attr = (ShowIfEnumValueAttribute)attribute;

            if (ShouldShow(property, attr))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfEnumValueAttribute attr = (ShowIfEnumValueAttribute)attribute;

            if (ShouldShow(property, attr))
                return EditorGUI.GetPropertyHeight(property, label, true);

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private bool ShouldShow(SerializedProperty property, ShowIfEnumValueAttribute attr)
        {
            string propertyPath = property.propertyPath;
            string enumPath = propertyPath.Replace(property.name, attr.EnumFieldName);
            SerializedProperty enumProp = property.serializedObject.FindProperty(enumPath);

            if (enumProp == null || enumProp.propertyType != SerializedPropertyType.Enum)
                return true;

            // Check if current enum value matches any target values
            var currentEnumValue = enumProp.enumValueIndex;
            var enumType = fieldInfo.DeclaringType.GetField(attr.EnumFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.FieldType;

            if (enumType == null) return true;

            var currentValue = System.Enum.GetValues(enumType).GetValue(currentEnumValue);
            return attr.TargetValues.Contains(currentValue);
        }
    }
}
