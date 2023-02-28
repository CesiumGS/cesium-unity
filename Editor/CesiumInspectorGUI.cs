
using System;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    public class CesiumInspectorGUI : IDisposable
    {
        public UnityEngine.Object[] targets { get; private set; }
        public int tooltipLabelWidth { get; set; } = 265;

        private bool _ignoreModifications = false;

        public CesiumInspectorGUI(params UnityEngine.Object[] targets)
        {
            this.targets = targets;

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Undo.postprocessModifications += OnPostProcessModifications;
        }

        ~CesiumInspectorGUI()
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

            GUIContent mainLabel = new GUIContent(label, tooltip);
            bool mainLabelTooLong = EditorStyles.label.CalcSize(mainLabel).x + 4.0f > originalLabelWidth;
            if (mainLabelTooLong)
                GUILayout.Label(mainLabel);

            using (new EditorGUILayout.HorizontalScope())
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                if (!mainLabelTooLong)
                    EditorGUILayout.PrefixLabel(mainLabel);

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

        public static void ClampedIntField(
            SerializedProperty property, int min, int max, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int value = EditorGUILayout.IntField(label, property.intValue);
                property.intValue = Math.Clamp(value, min, max);
            }
            else
            {
                EditorGUILayout.LabelField(
                    label.text, "Use ClampedIntField for int only.");
            }
        }

        public static void ClampedFloatField(
            SerializedProperty property, float min, float max, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                float value = EditorGUILayout.FloatField(label, property.floatValue);
                property.floatValue = Math.Clamp(value, min, max);
            }
            else
            {
                EditorGUILayout.LabelField(
                    label.text, "Use ClampedFloatField for float only.");
            }
        }

        public static void ClampedDoubleField(
            SerializedProperty property, double min, double max, GUIContent label)
        {
            // SerializedPropertyType.Float is used for both float and double;
            // SerializedPropertyType.Double does not exist.
            if (property.propertyType == SerializedPropertyType.Float)
            {
                double value = EditorGUILayout.DoubleField(label, property.doubleValue);
                property.doubleValue = Math.Clamp(value, min, max);
            }
            else
            {
                EditorGUILayout.LabelField(
                    label.text, "Use ClampedDoubleField for double only.");
            }
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
