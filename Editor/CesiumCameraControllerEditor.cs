using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumCameraController))]
    public class CesiumCameraControllerEditor : Editor
    {
        private CesiumCameraController _cameraController;

        private SerializedProperty _defaultMaximumSpeed;

        private SerializedProperty _enableDynamicSpeed;
        private SerializedProperty _dynamicSpeedMinHeight;

        private SerializedProperty _enableDynamicClippingPlanes;
        private SerializedProperty _dynamicClippingPlanesMinHeight;

        private SerializedProperty _flyToAltitudeProfileCurve;
        private SerializedProperty _flyToProgressCurve;
        private SerializedProperty _flyToMaximumAltitudeCurve;
        private SerializedProperty _flyToDuration;
        private SerializedProperty _flyToGranularityDegrees;

        private void OnEnable()
        {
            this._cameraController = (CesiumCameraController)this.target;

            this._defaultMaximumSpeed =
                this.serializedObject.FindProperty("_defaultMaximumSpeed");

            this._enableDynamicSpeed =
                this.serializedObject.FindProperty("_enableDynamicSpeed");
            this._dynamicSpeedMinHeight =
                this.serializedObject.FindProperty("_dynamicSpeedMinHeight");

            this._enableDynamicClippingPlanes =
                this.serializedObject.FindProperty("_enableDynamicClippingPlanes");
            this._dynamicClippingPlanesMinHeight =
                this.serializedObject.FindProperty("_dynamicClippingPlanesMinHeight");

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

            this.DrawDynamicProperties();
            EditorGUILayout.Space(5);
            this.DrawFlyToProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawDynamicProperties()
        {
            // The labels for this component are particularly long, so use a custom value
            // instead of the editor style's default.
            int labelWidth = 215;

            GUILayout.BeginHorizontal();
            GUIContent defaultMaximumSpeedContent = new GUIContent(
                "Default Maximum Speed",
                "The controller's maximum speed when dynamic speed is disabled. " +
                "If dynamic speed is enabled, this value will not be used.");
            GUILayout.Label(defaultMaximumSpeedContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._defaultMaximumSpeed, GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUIContent enableDynamicSpeedContent = new GUIContent(
                "Enable Dynamic Speed",
                "If enabled, the controller's speed will change dynamically based on " +
                "elevation and other factors.");
            GUILayout.Label(enableDynamicSpeedContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._enableDynamicSpeed, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!this._enableDynamicSpeed.boolValue);

            GUILayout.BeginHorizontal();
            GUIContent dynamicSpeedMinHeightContent = new GUIContent(
                "Dynamic Speed Min Height",
                "The minimum height where dynamic speed starts to take effect." +
                "\n\n" +
                "Below this height, the current speed will be set as the height of " +
                "the camera above tilesets in the scene. This forces the camera to move " +
                "more slowly when it is right above a tileset.");
            GUILayout.Label(dynamicSpeedMinHeightContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._dynamicSpeedMinHeight, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUIContent enableDynamicClippingPlanesContent = new GUIContent(
                "Enable Dynamic Clipping Planes",
                "If enabled, the camera will dynamically reposition its clipping " +
                "planes so that the globe will not get clipped from far away. " +
                "If this option is disabled, the globe and other tilesets may not render " +
                "at large distances because they will be clipped by the camera." +
                "\n\n" +
                "This setting may not work well for rendering objects that are far " +
                "above the Earth but still close to the camera as it zooms out.");
            GUILayout.Label(enableDynamicClippingPlanesContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._enableDynamicClippingPlanes, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!this._enableDynamicClippingPlanes.boolValue);

            GUILayout.BeginHorizontal();
            GUIContent dynamicClippingPlanesMinHeightContent = new GUIContent(
                "Dynamic Clipping Planes Min Height",
                "The height to start dynamically adjust the camera's clipping " + 
                "planes. Below this height, the clipping planes will be set to their " +
                "initial values.");
            GUILayout.Label(dynamicClippingPlanesMinHeightContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._dynamicClippingPlanesMinHeight, GUIContent.none);
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawFlyToProperties()
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
