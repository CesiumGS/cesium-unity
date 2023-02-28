using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(CesiumGlobeAnchor))]
    public class CesiumGlobeAnchorEditor : Editor
    {
        private CesiumGlobeAnchor _globeAnchor;
        private CesiumInspectorGUI _gui;

        private void OnEnable()
        {
            this._globeAnchor = (CesiumGlobeAnchor)this.target;
            this._gui = new CesiumInspectorGUI(this._globeAnchor);
        }

        private void OnDisable()
        {
            if (this._gui != null)
            {
                this._gui.Dispose();
                this._gui = null;
            }
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            DrawGlobeAnchorProperties();
            EditorGUILayout.Space(5);
            DrawLongitudeLatitudeHeightProperties();
            EditorGUILayout.Space(5);
            DrawEarthCenteredEarthFixedProperties();
            EditorGUILayout.Space(5);
            DrawEastUpNorthRotationProperties();
            EditorGUILayout.Space(5);
            DrawScaleProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        private static readonly string adjustOrientationForGlobeWhenMovingTooltip = CesiumEditorUtility.FormatTooltip(@"
            Whether to adjust the game object's orientation based on globe curvature
            as the game object moves.

            The Earth is not flat, so as we move across its surface, the direction of
            ""up"" changes. If we ignore this fact and leave an object's orientation
            unchanged as it moves over the globe surface, the object will become
            increasingly tilted and eventually be completely upside-down when we arrive
            at the opposite side of the globe.

            When this setting is enabled, this component will automatically apply a
            rotation to the Transform to account for globe curvature any time the game
            object's position on the globe changes.

            This property should usually be enabled, but it may be useful to disable it
            when your application already accounts for globe curvature itself when it
            updates a game object's transform, because in that case game object would
            be over-rotated.");

        private static readonly string detectTransformChangesTooltip = CesiumEditorUtility.FormatTooltip(@"
            Whether this component should detect changes to the Transform component,
            such as from physics, and update the precise coordinates accordingly.
            Disabling this option improves performance for game objects that will not
            move. Transform changes are always detected in Edit mode, no matter the
            state of this flag.");

        private void DrawGlobeAnchorProperties()
        {
            this._gui.Toggle(
                "Adjust Orientation For Globe When Moving",
                this._globeAnchor.adjustOrientationForGlobeWhenMoving,
                (value) => this._globeAnchor.adjustOrientationForGlobeWhenMoving = value,
                adjustOrientationForGlobeWhenMovingTooltip);

            this._gui.Toggle(
                "Detect Transform Changes",
                this._globeAnchor.detectTransformChanges,
                (value) => this._globeAnchor.detectTransformChanges = value,
                detectTransformChangesTooltip);
        }

        private static readonly string longitudeLatitudeHeightTooltip = CesiumEditorUtility.FormatTooltip(@"
            The Latitude, Longitude, and Height of this game object.

            The Latitude (Lat) is expressed in degrees, in the range [-90, 90].

            The Longitude (Lon) is expressed in degrees, in the range [-180, 180].

            The Height (H) is expressed in meters above the ellipsoid (usually WGS84). Do not
            confuse this with a geoid height or height above mean sea level, which
            can be tens of meters higher or lower depending on where in the world the
            object is located.");

        private static readonly GUIContent latitudeContent = new GUIContent("Lat");
        private static readonly GUIContent longitudeContent = new GUIContent("Lon");
        private static readonly GUIContent heightContent = new GUIContent("H");

        private void DrawLongitudeLatitudeHeightProperties()
        {
            double3 longitudeLatitudeHeight = this._globeAnchor.longitudeLatitudeHeight;
            longitudeLatitudeHeight.x = Math.Round(longitudeLatitudeHeight.x, 8);
            longitudeLatitudeHeight.y = Math.Round(longitudeLatitudeHeight.y, 8);
            longitudeLatitudeHeight.z = Math.Round(longitudeLatitudeHeight.z, 4);

            // Swap X and Y so that latitude is first, to match most users' intuitions.
            longitudeLatitudeHeight = longitudeLatitudeHeight.yxz;

            this._gui.Double3(
                "Position (Latitude, Longitude, Height)",
                longitudeLatitudeHeight,
                (value) =>
                {
                    this._globeAnchor.longitudeLatitudeHeight = value.yxz;
                },
                longitudeLatitudeHeightTooltip,
                latitudeContent,
                longitudeContent,
                heightContent);
        }

        private static readonly string ecefTooltip = CesiumEditorUtility.FormatTooltip(@"
            The Earth-Centered, Earth-Fixed (ECEF) coordinates of the origin of this
            game object, in meters.

            ECEF is a right-handed coordinate system with its origin at the
            center of the Earth, the positive X axis pointing toward where the Prime
            Meridian crosses the Equator, the positive Y axis pointing toward the
            Equator at 90 degrees longitude, and the positive Z axis pointing toward
            the North pole.");

        private void DrawEarthCenteredEarthFixedProperties()
        {
            double3 ecefPosition = this._globeAnchor.positionGlobeFixed;
            ecefPosition.x = Math.Round(ecefPosition.x, 4);
            ecefPosition.y = Math.Round(ecefPosition.y, 4);
            ecefPosition.z = Math.Round(ecefPosition.z, 4);

            this._gui.Double3(
                "Position (Earth-Centered, Earth-Fixed)",
                ecefPosition,
                (value) =>
                {
                    this._globeAnchor.positionGlobeFixed = value;
                },
                ecefTooltip);
        }

        private static readonly string rotationTooltip = CesiumEditorUtility.FormatTooltip(@"
            The rotation of the object relative to the local East-Up-North
            (EUN) frame centered at the object's position. In the EUN frame +X points East, +Y
            points Up, and +Z points North.

            These Euler angles are expressed in degrees and use Unity's normal ZXY convention.");

        private void DrawEastUpNorthRotationProperties()
        {
            Quaternion rotation = this._globeAnchor.rotationEastUpNorth;
            double3 eulerAngles = (float3)rotation.eulerAngles;

            eulerAngles.x = Math.Round(Math.Clamp(Helpers.Negative180To180(eulerAngles.x), -180.0f, 180.0f), 4);
            eulerAngles.y = Math.Round(Math.Clamp(Helpers.Negative180To180(eulerAngles.y), -180.0f, 180.0f), 4);
            eulerAngles.z = Math.Round(Math.Clamp(Helpers.Negative180To180(eulerAngles.z), -180.0f, 180.0f), 4);

            this._gui.Double3(
                "Rotation (East-Up-North)",
                eulerAngles,
                (value) =>
                {
                    rotation.eulerAngles = (float3)value;
                    this._globeAnchor.rotationEastUpNorth = rotation;
                },
                rotationTooltip);
        }

        private static readonly string scaleTooltip = CesiumEditorUtility.FormatTooltip(@"The local scaling of the object.");

        private void DrawScaleProperties()
        {
            double3 scale = this._globeAnchor.scaleEastUpNorth;
            scale.x = Math.Round(scale.x, 4);
            scale.y = Math.Round(scale.y, 4);
            scale.z = Math.Round(scale.z, 4);

            this._gui.Double3(
                "Scale",
                scale,
                (value) =>
                {
                    // Don't try to set a scale that is too close to zero.
                    if (math.cmin(math.abs(value)) < 1.0e-15)
                        return;
                    this._globeAnchor.scaleEastUpNorth = value;
                },
                scaleTooltip);
        }
    }
}