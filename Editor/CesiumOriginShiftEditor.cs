using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumOriginShift))]
    public class CesiumOriginShiftEditor : Editor
    {
        private SerializedProperty _activationDistance;
        private SerializedProperty _useActivationDistance;

        private void OnEnable()
        {
            _activationDistance = this.serializedObject.FindProperty("_activationDistance");
            _useActivationDistance = this.serializedObject.FindProperty("_useActivationDistance");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.DrawProperties();
            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperties()
        {
            // Specify a label width so that both properties' values are aligned
            // This is the same value as in CesiumCameraControllerEditor so that everything lines up
            int labelWidth = 215;

            GUILayout.BeginHorizontal();
            GUIContent useActivationDistanceContent = new GUIContent(
                "Use Activation Distance",
                "Whether the Activation Distance property will be used. " +
                "If unchecked, the origin will be shifted on every frame.");
            GUILayout.Label(useActivationDistanceContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._useActivationDistance, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!this._useActivationDistance.boolValue);

            GUILayout.BeginHorizontal();
            GUIContent activationDistanceContent = new GUIContent(
                "Activation Distance",
                "When Use Activation Distance is checked, the origin will only be shifted " +
                "when the distance in meters from the parent georeference is greater than this value.");
            GUILayout.Label(activationDistanceContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._activationDistance, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }
    }
}
