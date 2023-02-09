
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public static class CesiumGUI
    {
        const int labelWidth = 265;

        public static void Toggle(UnityEngine.Object target, bool value, Action<bool> set, string label, string tooltip)
        {
            bool newValue = EditorGUILayout.Toggle(new GUIContent(label, FormatTooltip(tooltip)), value);
            if (newValue != value)
            {
                if (target != null)
                    Undo.RecordObject(target, "Changed " + label);
                set(newValue);
            }
        }

        public static void Double(UnityEngine.Object target, double value, Action<double> set, string label, string tooltip)
        {
            double newValue = EditorGUILayout.DelayedDoubleField(new GUIContent(label, FormatTooltip(tooltip)), value);
            if (newValue != value)
            {
                if (target != null)
                    Undo.RecordObject(target, "Changed " + label);
                set(newValue);
            }
        }

        public static void Enum<T>(UnityEngine.Object target, T value, Action<T> set, string label, string tooltip) where T : Enum
        {
            T newValue = (T)EditorGUILayout.EnumPopup(new GUIContent(label, FormatTooltip(tooltip)), (Enum)value);
            if (!value.Equals(newValue))
            {
                if (target != null)
                    Undo.RecordObject(target, "Changed " + label);
                set(newValue);
            }
        }

        private static Regex _findLineBreakSets = new Regex("(\r?\n)+[ \t]*");

        /// <summary>
        /// Apply some very basic formatting to the tooltip:
        /// - Trim whitespace from the beginning of the string and from the beginning of each new line
        /// - Remove single newlines, replace them with a space.
        /// - Leave double newlines, these start a new paragraph.
        /// </summary>
        /// <param name="tooltip">The original tooltip.</param>
        /// <returns>The formatted tooltip.</returns>
        private static string FormatTooltip(string tooltip)
        {
            return _findLineBreakSets.Replace(tooltip.Trim(), (match) =>
            {
                int newlineCount = 0;
                string matchString = match.Value;
                for (int i = 0; i < matchString.Length; ++i)
                {
                    if (matchString[i] == '\n')
                        ++newlineCount;
                }

                if (newlineCount == 1)
                    return " ";
                else // two or more newlines
                    return Environment.NewLine + Environment.NewLine;
            });
        }
    }
}
