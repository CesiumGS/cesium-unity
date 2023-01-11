using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumFlyToController))]
    public class CesiumFlyToControllerEditor : Editor
    {
        private SerializedProperty _flyToAltitudeProfileCurve;
        private SerializedProperty _flyToProgressCurve;
        private SerializedProperty _flyToMaximumAltitudeCurve;
        private SerializedProperty _flyToDuration;
        private SerializedProperty _flyToGranularityDegrees;

        private void OnEnable()
        {
            this._flyToAltitudeProfileCurve =
                this.serializedObject.FindProperty("_flyToAltitudeProfileCurve");
            this._flyToProgressCurve =
                this.serializedObject.FindProperty("_flyToProgressCurve");
            this._flyToMaximumAltitudeCurve =
                this.serializedObject.FindProperty("_flyToMaximumAltitudeCurve");
            this._flyToDuration =
                this.serializedObject.FindProperty("_flyToDuration");
            this._flyToGranularityDegrees =
                this.serializedObject.FindProperty("_flyToGranularityDegrees");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.DrawProperties();
            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperties()
        {
            GUILayout.Label("Fly-To Properties", EditorStyles.boldLabel);

            GUIContent flyToAltitudeProfileCurveContent = new GUIContent(
                "Altitude Profile Curve",
                "This curve dictates what percentage of the max altitude the " +
                "camera should take at a given time on the curve." +
                "\n\n" +
                "This curve must be kept in the 0 to 1 range on both axes. The " +
                "\"Maximum Altitude Curve\" dictates the actual max " +
                "altitude at each point along the curve.");
            EditorGUILayout.PropertyField(
                this._flyToAltitudeProfileCurve, flyToAltitudeProfileCurveContent);

            GUIContent flyToProressCurveContent = new GUIContent(
                "Progress Curve",
                "This curve is used to determine the progress percentage for " +
                "all the other curves. This allows us to accelerate and deaccelerate " +
                "as wanted throughout the curve.");
            EditorGUILayout.PropertyField(
                this._flyToProgressCurve, flyToProressCurveContent);

            GUIContent flyToMaximumAltitudeCurveContent = new GUIContent(
                "Maximum Altitude Curve",
                "This curve dictates the maximum altitude at each point along " +
                "the curve." +
                "\n\n" +
                "This can be used in conjunction with the \"Altitude Profile " +
                "Curve\" to allow the camera to take some altitude during the flight.");
            EditorGUILayout.PropertyField(
                this._flyToMaximumAltitudeCurve, flyToMaximumAltitudeCurveContent);

            GUIContent flyToDurationContent = new GUIContent(
                "Duration",
                "The length in seconds that the camera flight should last.");
            EditorGUILayout.PropertyField(
                this._flyToDuration, flyToDurationContent);

            GUIContent flyToGranularityDegreesContent = new GUIContent(
                "Granularity Degrees",
                "The granularity in degrees with which keypoints should be generated " +
                "for the flight interpolation. This value should be greater than 0.0, otherwise " +
                "the controller will not take flight." +
                "\n\n" +
                "This represents the difference in degrees between each keypoint on the flight path. " +
                "The lower the value, the more keypoints are generated, and the smoother the flight " +
                "interpolation will be.");
            EditorGUILayout.PropertyField(
                this._flyToGranularityDegrees, flyToGranularityDegreesContent);
        }
    }
}