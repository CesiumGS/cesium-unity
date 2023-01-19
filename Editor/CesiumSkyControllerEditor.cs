using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumSkyController))]
    public class CesiumSkyControllerEditor : Editor
    {
        private CesiumSkyController skyController;
        private SerializedProperty timeOfDay;
        private SerializedProperty updateOnTick;
        private SerializedProperty updateInEditor;


        private void OnEnable()
        {
            this.skyController = (CesiumSkyController)target;
            this.timeOfDay = this.serializedObject.FindProperty("timeOfDay");
            this.updateOnTick = this.serializedObject.FindProperty("updateOnTick");
            this.updateInEditor = this.serializedObject.FindProperty("updateInEditor");
        }

        public override void OnInspectorGUI()
        {
            DrawSkySettings();
            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawSkySettings()
        {
            GUIContent updateSky = new GUIContent(
                "Update Sky",
                "Updates light angle and shader blending manually. You do not need to call " +
                "this if UpdateOnTick is enabled.");
            if (GUILayout.Button(updateSky))
            {
                this.skyController.UpdateSky();
            }

            // GUILayout.Label("Level of Detail", EditorStyles.boldLabel);

            GUIContent updateOnTickContent = new GUIContent("Update on Tick", "day hour");
            EditorGUILayout.PropertyField(this.updateOnTick, updateOnTickContent);

            GUIContent updateInEditorContent = new GUIContent("Update in Editor", "day hour");
            EditorGUILayout.PropertyField(this.updateInEditor, updateInEditorContent);

            GUIContent timeOfDayContent = new GUIContent("Time of Day", "day hour");
            EditorGUILayout.PropertyField(this.timeOfDay, timeOfDayContent);

        }
    }
}

