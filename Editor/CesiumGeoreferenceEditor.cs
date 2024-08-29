using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGeoreference))]
    public class CesiumGeoreferenceEditor : Editor
    {
        private CesiumGeoreference _georeference;

        private SerializedProperty _originPlacement;

        // Converts the SerializedProperty's value to the CesiumGeoreferenceOriginPlacement
        // enum it corresponds to, for convenience.
        internal CesiumGeoreferenceOriginPlacement originPlacement
        {
            get
            {
                return (CesiumGeoreferenceOriginPlacement)
                    this._originPlacement.enumValueIndex;
            }
        }

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

        private SerializedProperty _ellipsoidOverride;
        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _height;

        private SerializedProperty _ecefX;
        private SerializedProperty _ecefY;
        private SerializedProperty _ecefZ;

        private SerializedProperty _scale;

        private void OnEnable()
        {
            this._georeference = (CesiumGeoreference)this.target;

            this._ellipsoidOverride = this.serializedObject.FindProperty("_ellipsoidOverride");
            this._originPlacement =
                this.serializedObject.FindProperty("_originPlacement");
            this._originAuthority =
                this.serializedObject.FindProperty("_originAuthority");

            this._latitude = this.serializedObject.FindProperty("_latitude");
            this._longitude = this.serializedObject.FindProperty("_longitude");
            this._height = this.serializedObject.FindProperty("_height");

            this._ecefX = this.serializedObject.FindProperty("_ecefX");
            this._ecefY = this.serializedObject.FindProperty("_ecefY");
            this._ecefZ = this.serializedObject.FindProperty("_ecefZ");

            this._scale = this.serializedObject.FindProperty("_scale");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            
            this.DrawOriginModeProperties();
            EditorGUILayout.Space(5);
            this.DrawLongitudeLatitudeHeightProperties();
            EditorGUILayout.Space(5);
            this.DrawEarthCenteredEarthFixedProperties();
            EditorGUILayout.Space(5);
            
            this.DrawScaleProperty();
            this.DrawEllipsoidOverrideProperty();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawInspectorButtons()
        {
            // Don't modify the georeference if the editor is in play mode.
            EditorGUI.BeginDisabledGroup(
                EditorApplication.isPlaying
                || this.originPlacement != CesiumGeoreferenceOriginPlacement.CartographicOrigin);

            GUILayout.BeginHorizontal();
            GUIContent placeOriginHereContent = new GUIContent(
                "Place Origin Here",
                "Places the georeference origin at the camera's current location. " +
                "Rotates the globe so the current longitude/latitude/height of the " +
                "camera is at the Unity origin. The camera is also teleported to the " +
                "Unity origin." +
                "\n\n" +
                "Warning: Before clicking, ensure that all non-Cesium objects in the " +
                "persistent level are georeferenced with the \"CesiumGeoreference\" component " +
                "or are children of a GameObject with that component. Ensure that static " +
                "GameObjects only exist in georeferenced sub-scenes.");
            if (GUILayout.Button(placeOriginHereContent))
            {
                CesiumEditorUtility.PlaceGeoreferenceAtCameraPosition(this._georeference);
            }

            GUIContent createSubSceneContent = new GUIContent(
                "Create Sub-Scene Here",
                "Creates a child GameObject with a \"CesiumSubScene\" component whose origin " +
                "is set to the camera's current location. A \"CesiumSubScene\" describes a " +
                "corresponding world location that can be jumped to, and only one sub-scene " +
                "can be worked on in the editor at a time." +
                "\n\n" +
                "This is disabled when \"Origin Placement\" is set to \"True Origin\".");
            if (GUILayout.Button("Create Sub-Scene Here"))
            {
                CesiumEditorUtility.CreateSubScene(this._georeference);
            }

            GUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void DrawOriginModeProperties()
        {
            GUILayout.Label("Origin Mode", EditorStyles.boldLabel);

            GUIContent originPlacementContent = new GUIContent(
                "Placement",
                "The placement of this GameObject's origin (0,0,0) within the tileset." +
                "\n\n" +
                "3D Tiles tilesets often use Earth-centered, Earth-fixed coordinates, such that " +
                "the tileset content is in a small bounding volume 6-7 million meters " +
                "(the radius of the Earth) away from the coordinate system origin. This property " +
                "allows an alternative position, other than the tileset's true origin, to be " +
                "treated as the origin for the purpose of this GameObject. Using this property will " +
                "preserve vertex precision (and thus avoid jittering) much better than setting the " +
                "GameObject's Transform property.");
            EditorGUILayout.PropertyField(this._originPlacement, originPlacementContent);
            
            EditorGUI.BeginDisabledGroup(
                this.originPlacement != CesiumGeoreferenceOriginPlacement.CartographicOrigin);

            GUIContent originAuthorityContent = new GUIContent(
                "Authority",
                "The set of coordinates that authoritatively define the origin of " +
                "this georeference.");
            EditorGUILayout.PropertyField(this._originAuthority, originAuthorityContent);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEllipsoidOverrideProperty()
        {
            EditorGUI.BeginDisabledGroup(
                this.originPlacement != CesiumGeoreferenceOriginPlacement.CartographicOrigin);

            GUIContent ellipsoidOverrideContent = new GUIContent(
                "Ellipsoid Override",
                "The ellipsoid definition to use for this tileset. If this is left blank, " +
                "the ellipsoid specified by the tileset is used, or WGS84 if the tileset " +
                "doesn't list an ellipsoid to use.");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._ellipsoidOverride, ellipsoidOverrideContent);
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                this._georeference.ReloadEllipsoid();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawScaleProperty()
        {
            GUIContent scaleContent = new GUIContent(
                "Scale",
                "The scale of the globe in the Unity world. If this value is 0.5, for " +
                "example, one meter on the globe occupies half a meter in the Unity world. " +
                "The globe can also be scaled by modifying the georeference's Transform, " +
                "but setting this property instead will do a better job of preserving precision.");
            EditorGUILayout.PropertyField(this._scale, scaleContent);
        }

        private void DrawLongitudeLatitudeHeightProperties()
        {
            CesiumGeoreferenceOriginPlacement placement = this.originPlacement;
            CesiumGeoreferenceOriginAuthority authority = this.originAuthority;
            EditorGUI.BeginDisabledGroup(
                placement != CesiumGeoreferenceOriginPlacement.CartographicOrigin ||
                authority != CesiumGeoreferenceOriginAuthority.LongitudeLatitudeHeight);

            GUILayout.Label(
                "Origin (Longitude Latitude Height)", EditorStyles.boldLabel);

            GUIContent latitudeContent = new GUIContent(
                "Latitude",
                "The latitude of the origin in degrees, in the range [-90, 90].");
            CesiumInspectorGUI.ClampedDoubleField(
                this._latitude,
                -90.0,
                90.0,
                latitudeContent,
                delayed: true);

            GUIContent longitudeContent = new GUIContent(
                "Longitude",
                "The longitude of the origin in degrees, in the range [-180, 180].");
            CesiumInspectorGUI.ClampedDoubleField(
                 this._longitude,
                 -180.0,
                 180.0,
                 longitudeContent,
                 delayed: true);

            GUIContent heightContent = new GUIContent(
                "Height",
                "The height of the origin in meters above the ellipsoid (usually WGS84)." +
                "\n\n" +
                "Do not confuse this with a geoid height or height above mean sea level, which " +
                "can be tens of meters higher or lower depending on where in the world the " +
                "origin is located.");
            EditorGUILayout.PropertyField(this._height, heightContent);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEarthCenteredEarthFixedProperties()
        {
            CesiumGeoreferenceOriginPlacement placement = this.originPlacement;
            CesiumGeoreferenceOriginAuthority authority = this.originAuthority;
            EditorGUI.BeginDisabledGroup(
                placement != CesiumGeoreferenceOriginPlacement.CartographicOrigin ||
                 authority != CesiumGeoreferenceOriginAuthority.EarthCenteredEarthFixed);

            GUILayout.Label(
                "Origin (Earth Centered, Earth Fixed)", EditorStyles.boldLabel);

            GUIContent ecefXContent = new GUIContent(
                "ECEF X",
                "The Earth-Centered, Earth-Fixed X-coordinate of the origin of this " +
                "georeference in meters." +
                "\n\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth " +
                 "and the positive X axis points toward where the Prime Meridian crosses " +
                 "the Equator.");
            EditorGUILayout.PropertyField(this._ecefX, ecefXContent);

            GUIContent ecefYContent = new GUIContent(
                "ECEF Y",
                "The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this " +
                "georeference in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Y axis points toward the Equator at 90 degrees longitude.");
            EditorGUILayout.PropertyField(this._ecefY, ecefYContent);

            GUIContent ecefZContent = new GUIContent(
                "ECEF Z",
                "The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this " +
                "georeference in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Z axis points toward the North pole.");
            EditorGUILayout.PropertyField(this._ecefZ, ecefZContent);

            EditorGUI.EndDisabledGroup();
        }
    }
}
