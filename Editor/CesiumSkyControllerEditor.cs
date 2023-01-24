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

        private SerializedProperty _sunLight;
        private SerializedProperty _updateOnTick;
        private SerializedProperty _updateInEditor;

        private SerializedProperty _timeOfDay;

        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _northOffset;
        private SerializedProperty _day;
        private SerializedProperty _month;
        private SerializedProperty _year;
        private SerializedProperty _timeZone;

        private SerializedProperty _groundBlendHeight;
        private SerializedProperty _spaceBlendHeight;

        bool enableDebugControls = false;


        private void OnEnable()
        {
            this.skyController = (CesiumSkyController)target;
            this._sunLight = this.serializedObject.FindProperty("_sunLight");
            this._timeOfDay = this.serializedObject.FindProperty("_timeOfDay");
            this._updateOnTick = this.serializedObject.FindProperty("_updateOnTick");
            this._updateInEditor = this.serializedObject.FindProperty("_updateInEditor");

            this._latitude = this.serializedObject.FindProperty("_latitude");
            this._longitude = this.serializedObject.FindProperty("_longitude");
            this._northOffset = this.serializedObject.FindProperty("_northOffset");
            this._day = this.serializedObject.FindProperty("_day");
            this._month = this.serializedObject.FindProperty("_month");
            this._year = this.serializedObject.FindProperty("_year");
            this._timeZone = this.serializedObject.FindProperty("_timeZone");
            this._groundBlendHeight = this.serializedObject.FindProperty("_groundBlendHeight");
            this._spaceBlendHeight = this.serializedObject.FindProperty("_spaceBlendHeight");
        }

        public override void OnInspectorGUI()
        {
            DrawSkyProperties();
            DrawTimeProperties();
            DrawSkyShadingProperties();
            DrawGeoreferenceProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawSkyProperties()
        {
            GUIContent updateSky = new GUIContent(
                "Update Sky",
                "Updates light angle and shader blending manually. You do not need to call this if UpdateOnTick is enabled.");
            if (GUILayout.Button(updateSky))
            {
                this.skyController.UpdateSky();
            }

            GUIContent sunLightContent = new GUIContent("Sun Directional Light", "The Directional Light object to use with this script. If this is not set, it will default to the prefab's Directional Light.");
            EditorGUILayout.PropertyField(this._sunLight, sunLightContent);

            GUIContent updateOnTickContent = new GUIContent("Update on Tick", "Whether or not to update every tick. If this is false, UpdateSky must be called manually.");
            EditorGUILayout.PropertyField(this._updateOnTick, updateOnTickContent);

            GUIContent updateInEditorContent = new GUIContent("Update in Editor", "Whether or not to update the sky in editor.");
            EditorGUILayout.PropertyField(this._updateInEditor, updateInEditorContent);

            EditorGUILayout.Space(10);

        }

        private void DrawTimeProperties()
        {
            GUILayout.Label("Time and Date", EditorStyles.boldLabel);

            GUIContent timeOfDayContent = new GUIContent("Time of Day", "The current time, from 0 to 24.");
            EditorGUILayout.PropertyField(this._timeOfDay, timeOfDayContent);

            GUIContent dayContent = new GUIContent("Date", "The current day of the month, from 0 to 31.");
            EditorGUILayout.PropertyField(this._day, dayContent);

            GUIContent monthContent = new GUIContent("Month", "The current month of the year, from 1 (January) to 12 (December).");
            EditorGUILayout.PropertyField(this._month, monthContent);

            GUIContent yearContent = new GUIContent("Year", "The current year.");
            EditorGUILayout.PropertyField(this._year, yearContent);

            GUIContent timeZoneContent = new GUIContent("Time Zone", "Time zone, as an offset from GMT.");
            EditorGUILayout.PropertyField(this._timeZone, timeZoneContent);

            EditorGUILayout.Space(10);

        }

        private void DrawSkyShadingProperties()
        {
            GUILayout.Label("Sky Shading", EditorStyles.boldLabel);

            GUIContent groundBlendHeightContent = new GUIContent("Ground blend height", "Height at which to begin blending the atmosphere to space.");
            EditorGUILayout.PropertyField(this._groundBlendHeight, groundBlendHeightContent);

            GUIContent spaceBlendHeightContent = new GUIContent("Space blend height", "Height at which the atmosphere is completely replaced with the space color.");
            EditorGUILayout.PropertyField(this._spaceBlendHeight, spaceBlendHeightContent);

            EditorGUILayout.Space(10);
        }

        private void DrawGeoreferenceProperties()
        {
            GUILayout.Label("Georeferencing (Debug Only)", EditorStyles.boldLabel);

            enableDebugControls = EditorGUILayout.Toggle("Enable Debug Georeferencing Controls", enableDebugControls);

            using (new EditorGUI.DisabledScope(enableDebugControls == false))
            {
                GUIContent latitudeContent = new GUIContent("Latitude", "Origin Latitude");
                EditorGUILayout.PropertyField(this._latitude, latitudeContent);

                GUIContent longitudeContent = new GUIContent("Longitude", "Origin Longitude");
                EditorGUILayout.PropertyField(this._longitude, longitudeContent);

                GUIContent northOffsetContent = new GUIContent("North Offset", "Adjusted sunrise to sunset direction.");
                EditorGUILayout.PropertyField(this._northOffset, northOffsetContent);
            }

        }
    }
}

