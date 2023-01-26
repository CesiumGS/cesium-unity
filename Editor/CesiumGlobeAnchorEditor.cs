using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGlobeAnchor))]
    public class CesiumGlobeAnchorEditor : Editor
    {
        private CesiumGlobeAnchor _globeAnchor;

        private SerializedProperty _adjustOrientationForGlobeWhenMoving;
        private SerializedProperty _detectTransformChanges;
        private SerializedProperty _positionAuthority;

        // Converts the SerializedProperty's value to the CesiumGeoreferenceOriginAuthority
        // enum it corresponds to, for convenience.
        internal CesiumGlobeAnchorPositionAuthority positionAuthority
        {   
            get
            {
                return (CesiumGlobeAnchorPositionAuthority)
                    this._positionAuthority.enumValueIndex;
            }
        }

        private SerializedProperty _latitude;
        private SerializedProperty _longitude;
        private SerializedProperty _height;

        private SerializedProperty _ecefX;
        private SerializedProperty _ecefY;
        private SerializedProperty _ecefZ;

        private SerializedProperty _unityX;
        private SerializedProperty _unityY;
        private SerializedProperty _unityZ;

        private void OnEnable()
        {
            this._globeAnchor = (CesiumGlobeAnchor)this.target;

            this._adjustOrientationForGlobeWhenMoving =
                this.serializedObject.FindProperty("_adjustOrientationForGlobeWhenMoving");
            this._detectTransformChanges =
                this.serializedObject.FindProperty("_detectTransformChanges");
            this._positionAuthority =
                this.serializedObject.FindProperty("_positionAuthority");

            this._latitude = this.serializedObject.FindProperty("_latitude");
            this._longitude = this.serializedObject.FindProperty("_longitude");
            this._height = this.serializedObject.FindProperty("_height");

            this._ecefX = this.serializedObject.FindProperty("_ecefX");
            this._ecefY = this.serializedObject.FindProperty("_ecefY");
            this._ecefZ = this.serializedObject.FindProperty("_ecefZ");

            this._unityX = this.serializedObject.FindProperty("_unityX");
            this._unityY = this.serializedObject.FindProperty("_unityY");
            this._unityZ = this.serializedObject.FindProperty("_unityZ");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            DrawGlobeAnchorProperties();
            DrawPositionAuthorityProperty();
            EditorGUILayout.Space(5);
            DrawLongitudeLatitudeHeightProperties();
            EditorGUILayout.Space(5);
            DrawEarthCenteredEarthFixedProperties();
            EditorGUILayout.Space(5);
            DrawUnityPositionProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private void DrawGlobeAnchorProperties()
        {
            // The labels for this component are particularly long, so use a custom value
            // instead of the editor style's default.
            int labelWidth = 265;
            GUILayout.BeginHorizontal();
            GUIContent adjustOrientationContent = new GUIContent(
                "Adjust Orientation For Globe When Moving",
                "Whether to adjust the game object's orientation based on globe curvature " +
                "as the game object moves." +
                "\n\n" +
                "The Earth is not flat, so as we move across its surface, the direction of " +
                "\"up\" changes. If we ignore this fact and leave an object's orientation " +
                "unchanged as it moves over the globe surface, the object will become " +
                "increasingly tilted and eventually be completely upside-down when we arrive " +
                "at the opposite side of the globe." +
                "\n\n" +
                "When this setting is enabled, this component will automatically apply a " +
                "rotation to the Transform to account for globe curvature any time the game " +
                "object's position on the globe changes." +
                "\n\n" +
                "This property should usually be enabled, but it may be useful to disable it " +
                "when your application already accounts for globe curvature itself when it " +
                "updates a game object's transform, because in that case game object would " +
                "be over-rotated.");
            GUILayout.Label(adjustOrientationContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(
                this._adjustOrientationForGlobeWhenMoving,
                GUIContent.none);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUIContent detectTransformChangesContent = new GUIContent(
                "Detect Transform Changes",
                "Whether this component should detect changes to the Transform component, " +
                "such as from physics, and update the precise coordinates accordingly. " +
                "Disabling this option improves performance for game objects that will not " +
                "move. Transform changes are always detected in Edit mode, no matter the " +
                "state of this flag.");
            GUILayout.Label(detectTransformChangesContent, GUILayout.Width(labelWidth));
            EditorGUILayout.PropertyField(this._detectTransformChanges, GUIContent.none);
            GUILayout.EndHorizontal();
        }

        private void DrawPositionAuthorityProperty()
        {
            GUIContent positionAuthorityContent = new GUIContent(
                "Position Authority",
                "The set of coordinates that authoritatively define the position of this game object."
            );
            EditorGUILayout.PropertyField(this._positionAuthority);
        }

        private void DrawLongitudeLatitudeHeightProperties()
        {
            EditorGUI.BeginDisabledGroup(
                this.positionAuthority != CesiumGlobeAnchorPositionAuthority.LongitudeLatitudeHeight);

            GUILayout.Label("Position (Longitude Latitude Height)", EditorStyles.boldLabel);

            GUIContent latitudeContent = new GUIContent(
               "Latitude",
               "The latitude of this game object in degrees, in the range [-90, 90].");
            CesiumEditorUtility.InspectorGUI.ClampedDoubleField(
                this._latitude,
                -90.0,
                90.0,
                latitudeContent);

            GUIContent longitudeContent = new GUIContent(
                "Longitude",
                "The longitude of this game object in degrees, in the range [-180, 180].");
            CesiumEditorUtility.InspectorGUI.ClampedDoubleField(
                 this._longitude,
                 -180.0,
                 180.0,
                 longitudeContent);

            GUIContent heightContent = new GUIContent(
                "Height",
                "The height of this game object in meters above the ellipsoid (usually WGS84)." +
                "\n\n" +
                "Do not confuse this with a geoid height or height above mean sea level, which " +
                "can be tens of meters higher or lower depending on where in the world the " +
                "object is located.");
            EditorGUILayout.PropertyField(this._height, heightContent);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEarthCenteredEarthFixedProperties()
        {
            EditorGUI.BeginDisabledGroup(
                this.positionAuthority != CesiumGlobeAnchorPositionAuthority.EarthCenteredEarthFixed);

            GUILayout.Label("Position (Earth-Centered, Earth-Fixed)", EditorStyles.boldLabel);

            GUIContent ecefXContent = new GUIContent(
                "ECEF X",
                "The Earth-Centered, Earth-Fixed X-coordinate of the origin of this " +
                "game object in meters." +
                "\n\n" +
                 "In the ECEF coordinate system, the origin is at the center of the Earth " +
                 "and the positive X axis points toward where the Prime Meridian crosses " +
                 "the Equator.");
            EditorGUILayout.PropertyField(this._ecefX, ecefXContent);

            GUIContent ecefYContent = new GUIContent(
                "ECEF Y",
                "The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this " +
                "game object in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Y axis points toward the Equator at 90 degrees longitude.");
            EditorGUILayout.PropertyField(this._ecefY, ecefYContent);

            GUIContent ecefZContent = new GUIContent(
                "ECEF Z",
                "The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this " +
                "game object in meters." +
                "\n\n" +
                "In the ECEF coordinate system, the origin is at the center of the Earth " +
                "and the positive Z axis points toward the North pole.");
            EditorGUILayout.PropertyField(this._ecefZ, ecefZContent);

            EditorGUI.EndDisabledGroup();
        }

        private void DrawUnityPositionProperties()
        {
            EditorGUI.BeginDisabledGroup(
                this.positionAuthority != CesiumGlobeAnchorPositionAuthority.UnityCoordinates);

            GUILayout.Label("Position (Unity Local Coordinates)", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            GUIContent unityXContent = new GUIContent(
                "Unity X",
                "The Unity X coordinate of this game object. This is the same as the Transform's " +
                "X coordinate but expressed in 64-bit (double) precision.");
            EditorGUILayout.PropertyField(this._unityX, unityXContent);

            GUIContent unityYContent = new GUIContent(
                "Unity Y",
                "The Unity Y coordinate of this game object. This is the same as the Transform's " +
                "Y coordinate but expressed in 64-bit (double) precision.");
            EditorGUILayout.PropertyField(this._unityY, unityYContent);

            GUIContent unityZContent = new GUIContent(
                "Unity Z",
                "The Unity Z coordinate of this game object. This is the same as the Transform's " +
                "Z coordinate but expressed in 64-bit (double) precision.");
            EditorGUILayout.PropertyField(this._unityZ, unityZContent);

            EditorGUI.EndDisabledGroup();
        }
    }
}