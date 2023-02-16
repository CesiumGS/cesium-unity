
using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumGUI : IDisposable
    {
        public UnityEngine.Object[] targets { get; private set; }
        public int tooltipLabelWidth { get; set; } = 265;

        private bool _ignoreModifications = false;

        public CesiumGUI(params UnityEngine.Object[] targets)
        {
            this.targets = targets;

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Undo.postprocessModifications += OnPostProcessModifications;
        }

        ~CesiumGUI()
        {
            Debug.Log("CesiumGUI was not disposed. Be sure to call Dispose in OnDisable.");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.postprocessModifications -= OnPostProcessModifications;
        }

        public void Toggle(string label, bool value, Action<bool> set, string tooltip)
        {
            bool newValue = EditorGUILayout.Toggle(new GUIContent(label, tooltip), value);
            if (newValue != value)
                ApplyChange(label, newValue, set);
        }

        public void Double(string label, double value, Action<double> set, string tooltip)
        {
            double newValue = EditorGUILayout.DoubleField(new GUIContent(label, tooltip), value);
            if (newValue != value)
                ApplyChange(label, newValue, set);
        }

        private static GUIContent defaultXContent = new GUIContent("X");
        private static GUIContent defaultYContent = new GUIContent("Y");
        private static GUIContent defaultZContent = new GUIContent("Z");

        public void Double3(string label, double3 value, Action<double3> set, string tooltip, GUIContent xContent = null, GUIContent yContent = null, GUIContent zContent = null)
        {
            xContent = xContent ?? defaultXContent;
            yContent = yContent ?? defaultYContent;
            zContent = zContent ?? defaultZContent;

            float originalLabelWidth = EditorGUIUtility.labelWidth;

            using (new EditorGUILayout.HorizontalScope())
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(label, tooltip));
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(xContent).x + 4.0f;
                value.x = EditorGUILayout.DoubleField(xContent, value.x);
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(yContent).x + 4.0f;
                value.y = EditorGUILayout.DoubleField(yContent, value.y);
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(zContent).x + 4.0f;
                value.z = EditorGUILayout.DoubleField(zContent, value.z);

                if (changeScope.changed)
                    ApplyChange(label, value, set);
            }

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        public void Enum<T>(string label, T value, Action<T> set, string tooltip) where T : Enum
        {
            T newValue = (T)EditorGUILayout.EnumPopup(new GUIContent(label, tooltip), (Enum)value);
            if (!value.Equals(newValue))
                ApplyChange(label, newValue, set);
        }

        private void ApplyChange<T>(string label, T newValue, Action<T> set)
        {
            Undo.FlushUndoRecordObjects();
            this._ignoreModifications = true;
            try
            {
                Undo.RecordObjects(this.targets, "Changed " + label);
                set(newValue);
                Undo.FlushUndoRecordObjects();
            }
            finally
            {
                this._ignoreModifications = false;
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
        public static string FormatTooltip(string tooltip)
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

        private UndoPropertyModification[] OnPostProcessModifications(UndoPropertyModification[] modifications)
        {
            if (!this._ignoreModifications)
                this.RestartTargets();

            return modifications;
        }

        private void OnUndoRedoPerformed()
        {
            this.RestartTargets();
        }

        private void RestartTargets()
        {
            foreach (UnityEngine.Object target in this.targets)
            {
                if (target is ICesiumRestartable restartable)
                    restartable.Restart();
            }
        }
    }
}
