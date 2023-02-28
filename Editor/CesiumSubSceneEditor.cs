using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumSubScene))]
    public class CesiumSubSceneEditor : Editor
    {
        private CesiumSubScene _subScene;

        private SerializedProperty _activationRadius;
        private SerializedProperty _showActivationRadius;

        private SerializedProperty _originAuthority;

        // Converts the SerializedProperty's value to the CesiumGeoreferenceOriginAuthority
        // enum it corresponds to, for convenience.
        internal CesiumGeoreferenceOriginAuthority originAuthority
        {
            get
            {
                return (CesiumGeoreferenceOriginAuthority)
                    this._originAuthority.enumValueIndex;
            }
        }

        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _height;

        private SerializedProperty _ecefX;
        private SerializedProperty _ecefY;
        private SerializedProperty _ecefZ;

        private void OnEnable()
        {
            this._subScene = (CesiumSubScene)this.target;

            this._activationRadius = this.serializedObject.FindProperty("_activationRadius");
            this._showActivationRadius =
                this.serializedObject.FindProperty("_showActivationRadius");
            this._originAuthority =
                this.serializedObject.FindProperty("_originAuthority");

            this._latitude = this.serializedObject.FindProperty("_latitude");
            this._longitude = this.serializedObject.FindProperty("_longitude");
            this._height = this.serializedObject.FindProperty("_height");

            this._ecefX = this.serializedObject.FindProperty("_ecefX");
            this._ecefY = this.serializedObject.FindProperty("_ecefY");
            this._ecefZ = this.serializedObject.FindProperty("_ecefZ");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            DrawToolbarButton();
            EditorGUILayout.Space(5);
            DrawSubSceneProperties();
            EditorGUILayout.Space(5);
            
            DrawLongitudeLatitudeHeightProperties();
            EditorGUILayout.Space(5);
            DrawEarthCenteredEarthFixedProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawToolbarButton()
        {
            // Don't modify the sub-scene if the editor is in play mode.
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIContent placeOriginHereContent = new GUIContent(
                "Place Origin Here",
                "Places the sub-scene origin at the camera's current location. " +
                "Rotates the globe so the current longitude/latitude/height of the " +
                "camera is at the Unity origin. The camera is also teleported to the " +
                "Unity origin." +
                "\n\n" +
                "Warning: Before clicking, ensure that all non-Cesium objects in the " +
                "persistent scene are georeferenced with the \"CesiumGlobeAnchor\" " +
                "component or are children of a GameObject with that component. " +
                "Ensure that static GameObjects only exist under georeferenced sub-scenes.");
            if (GUILayout.Button(placeOriginHereContent, GUILayout.Width(200)))
            {
                CesiumEditorUtility.PlaceSubSceneAtCameraPosition(this._subScene);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawSubSceneProperties()
        {
            GUIContent activationRadiusContent = new GUIContent(
                "Activation Radius",
                "The radius from the origin at which this sub-scene becomes active. " +
                "The sub-scene may not become active if another sub-scene is closer, " +
                "even when the camera is inside this radius.");
            EditorGUILayout.PropertyField(this._activationRadius, activationRadiusContent);

            GUIContent showActivationRadiusContent = new GUIContent(
                "Show Activation Radius",
                "Whether to visualize the sub-scene loading radius in the editor. " +
                "Helpful for initially positioning the sub-scene and choosing a load radius.");
            EditorGUILayout.PropertyField(this._showActivationRadius, showActivationRadiusContent);

            GUIContent originAuthorityContent = new GUIContent(
                "Origin Authority",
                "The set of coordinates that authoritatively define the origin of " +
                "this sub-scene.");
            EditorGUILayout.PropertyField(this._originAuthority, originAuthorityContent);
        }

        private void DrawLongitudeLatitudeHeightProperties()
        {
            CesiumGeoreferenceOriginAuthority authority = this.originAuthority;
            EditorGUI.BeginDisabledGroup(
                authority != CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight);

            GUILayout.Label(
                "Origin (Longitude Latitude Height)", EditorStyles.boldLabel);

            GUIContent latitudeContent = new GUIContent(
                "Latitude",
                "The latitude of the origin of this sub-scene in degrees, in the range [-90, 90].");
            CesiumInspectorGUI.ClampedDoubleField(
                this._latitude,
                -90.0,
                90.0,
                latitudeContent);

            GUIContent longitudeContent = new GUIContent(
                "Longitude",
                "The longitude of the origin of this sub-scene in degrees, in the range [-180, 180].");
            CesiumInspectorGUI.ClampedDoubleField(
                 this._longitude,
                 -180.0,
                 180.0,
                 longitudeContent);

            GUIContent heightContent = new GUIContent(
                "Height",
                "The height of the origin of this sub-scene in meters above the ellipsoid (usually WGS84)." +
                "\n\n" +
                "Do not confuse this with a geoid height or height above mean sea level, which " +
                "can be tens of meters higher or lower depending on where in the world the " +
                "origin is located.");
            EditorGUILayout.PropertyField(this._height, heightContent);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEarthCenteredEarthFixedProperties()
        {
            CesiumGeoreferenceOriginAuthority authority = this.originAuthority;
            EditorGUI.BeginDisabledGroup(
                 authority != CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed);

            GUILayout.Label(
                "Origin (Earth Centered, Earth Fixed)", EditorStyles.boldLabel);

            GUIContent ecefXContent = new GUIContent(
                "ECEF X",
                "The Earth-Centered, Earth-Fixed X-coordinate of the origin of this " +
                "sub-scene in meters." +
                "\n\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth " +
                 "and the positive X axis points toward where the Prime Meridian crosses " +
                 "the Equator.");
            EditorGUILayout.PropertyField(this._ecefX, ecefXContent);

            GUIContent ecefYContent = new GUIContent(
                "ECEF Y",
                "The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this " +
                "sub-scene in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Y axis points toward the Equator at 90 degrees longitude.");
            EditorGUILayout.PropertyField(this._ecefY, ecefYContent);

            GUIContent ecefZContent = new GUIContent(
                "ECEF Z",
                "The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this " +
                "sub-scene in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Z axis points toward the North pole.");
            EditorGUILayout.PropertyField(this._ecefZ, ecefZContent);

            EditorGUI.EndDisabledGroup();
        }
    }
}