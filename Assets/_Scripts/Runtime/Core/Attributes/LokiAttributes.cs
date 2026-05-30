using System;
using UnityEngine;
namespace LokiInspector {

    #region Conditional Attributes

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string Condition;
        public bool Invert;

        public ShowIfAttribute(string condition, bool invert = false)
        {
            Condition = condition;
            Invert = invert;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfEnumValueAttribute : PropertyAttribute
    {
        public string EnumFieldName;
        public object[] TargetValues;

        public ShowIfEnumValueAttribute(string enumFieldName, params object[] targetValues)
        {
            EnumFieldName = enumFieldName;
            TargetValues = targetValues;
        }
    }

    #endregion

    #region Appearance Attributes

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : PropertyAttribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class FoldoutGroupAttribute : Attribute
    {
        public string GroupName;
        public FoldoutGroupAttribute(string name) => GroupName = name;
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class TabGroupAttribute : Attribute
    {
        public string GroupName;
        public TabGroupAttribute(string groupName) => GroupName = groupName;
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : PropertyAttribute { }
    /// <summary>
    /// Changes the label text of a field in the inspector and optionally colors it.
    /// You can use the following colors (Viet hoa hay thuong deu duoc):
    /// - red
    /// - green
    /// - blue
    /// - yellow
    /// - cyan
    /// - magenta
    /// - white
    /// - black
    /// - gray
    /// - grey
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelTextAttribute : PropertyAttribute  // ← Changed from Attribute to PropertyAttribute
    {
        public enum LabelColor
        {
            None,
            red,
            green,
            blue,
            yellow,
            cyan,
            magenta,
            white,
            black,
            gray,
            orange,
        }
        public string Label { get; }
        public string ColorName { get; }
        public int FontSize { get; }
        public FontStyle FontStyle { get; }
        public LabelTextAttribute(string label = null, LabelColor color = LabelColor.None, int fontSize = 0, FontStyle fontStyle = FontStyle.Normal)
        {
            Label = label;
            ColorName = color.ToString();
            FontSize = fontSize;
            FontStyle = fontStyle;
        }
    }


    #endregion

    #region Drawer Attributes
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float Min { get; }
        public float Max { get; }
        public float EnforceMinMaxDistance { get; }

        public MinMaxSliderAttribute(float min, float max, float enforceMinMaxDistance = 0f)
        {
            Min = min;
            Max = max;
            EnforceMinMaxDistance = enforceMinMaxDistance;
        }
    }



    #endregion
}


