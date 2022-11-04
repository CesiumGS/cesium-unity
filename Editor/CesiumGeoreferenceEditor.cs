using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGeoreference))]
    public class CesiumGeoreferenceEditor : Editor
    {
        private CesiumGeoreference _georeference;

        private SerializedProperty _originAuthority;

        // Converts the SerializedProperty's value to CesiumGeoreferenceOriginAuthority
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
            this._georeference = (CesiumGeoreference)this.target;

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

            EditorGUI.BeginChangeCheck();
            this.DrawOriginAuthorityProperty();
            EditorGUILayout.Space(5);
            this.DrawLongitudeLatitudeHeightProperties();
            EditorGUILayout.Space(5);
            this.DrawEarthCenteredEarthFixedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                this._georeference.UpdateOrigin();
            }
        }

        private void DrawOriginAuthorityProperty()
        {
            GUIContent originAuthorityContent = new GUIContent(
                "Origin Authority",
                "The set of coordinates that authoritatively define the origin of " +
                "this georeference.");
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
                "The latitude of the origin in degrees, in the range [-90, 90].");
            CesiumEditorUtility.InspectorGUI.ClampedDoubleField(
                this._latitude,
                -90.0,
                90.0,
                latitudeContent);

            GUIContent longitudeContent = new GUIContent(
                "Longitude",
                "The longitude of the origin in degrees, in the range [-180, 180].");
            CesiumEditorUtility.InspectorGUI.ClampedDoubleField(
                 this._longitude,
                 -180.0,
                 180.0,
                 longitudeContent);

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
            CesiumGeoreferenceOriginAuthority authority = this.originAuthority;
            EditorGUI.BeginDisabledGroup(
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
