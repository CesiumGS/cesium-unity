
using System;
using System.Text.RegularExpressions;
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

        public void Dispose()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.postprocessModifications -= OnPostProcessModifications;
        }

        public void Toggle(bool value, Action<bool> set, string label, string tooltip)
        {
            bool newValue = EditorGUILayout.Toggle(new GUIContent(label, FormatTooltip(tooltip)), value);
            if (newValue != value)
                ApplyChange(label, newValue, set);
        }

        public void Double(double value, Action<double> set, string label, string tooltip)
        {
            double newValue = EditorGUILayout.DoubleField(new GUIContent(label, FormatTooltip(tooltip)), value);
            if (newValue != value)
                ApplyChange(label, newValue, set);
        }

        public void Enum<T>(T value, Action<T> set, string label, string tooltip) where T : Enum
        {
            T newValue = (T)EditorGUILayout.EnumPopup(new GUIContent(label, FormatTooltip(tooltip)), (Enum)value);
            if (!value.Equals(newValue))
                ApplyChange(label, newValue, set);
        }

        private void ApplyChange<T>(string label, T newValue, Action<T> set)
        {
            this._ignoreModifications = true;
            try
            {
                Undo.FlushUndoRecordObjects();
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
