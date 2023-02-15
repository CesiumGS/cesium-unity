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
        private CesiumGUI _gui;

        private void OnEnable()
        {
            this._globeAnchor = (CesiumGlobeAnchor)this.target;
            this._gui = new CesiumGUI(this._globeAnchor);
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

        private void DrawGlobeAnchorProperties()
        {
            this._gui.Toggle(
                this._globeAnchor.adjustOrientationForGlobeWhenMoving,
                (value) => this._globeAnchor.adjustOrientationForGlobeWhenMoving = value,
                "Adjust Orientation For Globe When Moving",
                @"Whether to adjust the game object's orientation based on globe curvature
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

            this._gui.Toggle(
                this._globeAnchor.detectTransformChanges,
                (value) => this._globeAnchor.detectTransformChanges = value,
                "Detect Transform Changes",
                @"Whether this component should detect changes to the Transform component,
                such as from physics, and update the precise coordinates accordingly.
                Disabling this option improves performance for game objects that will not
                move. Transform changes are always detected in Edit mode, no matter the
                state of this flag.");
        }

        private void DrawLongitudeLatitudeHeightProperties()
        {
            GUILayout.Label("Position (Longitude Latitude Height)", EditorStyles.boldLabel);

            // Note that Y is intentionally first here so that latitude is listed first.

            this._gui.Double(
                this._globeAnchor.longitudeLatitudeHeight.y,
                (value) =>
                {
                    var llh = this._globeAnchor.longitudeLatitudeHeight;
                    llh.y = value;
                    this._globeAnchor.longitudeLatitudeHeight = llh;
                },
                "Latitude",
                "The latitude of this game object in degrees, in the range [-90, 90].");

            this._gui.Double(
                this._globeAnchor.longitudeLatitudeHeight.x,
                (value) =>
                {
                    var llh = this._globeAnchor.longitudeLatitudeHeight;
                    llh.x = value;
                    this._globeAnchor.longitudeLatitudeHeight = llh;
                },
                "Longitude",
                "The longitude of this game object in degrees, in the range [-180, 180].");

            this._gui.Double(
                this._globeAnchor.longitudeLatitudeHeight.z,
                (value) =>
                {
                    var llh = this._globeAnchor.longitudeLatitudeHeight;
                    llh.z = value;
                    this._globeAnchor.longitudeLatitudeHeight = llh;
                },
                "Height",
                @"The height of this game object in meters above the ellipsoid (usually WGS84).

                Do not confuse this with a geoid height or height above mean sea level, which
                can be tens of meters higher or lower depending on where in the world the
                object is located.");
        }

        private void DrawEarthCenteredEarthFixedProperties()
        {
            GUILayout.Label("Position (Earth-Centered, Earth-Fixed)", EditorStyles.boldLabel);

            this._gui.Double(
                this._globeAnchor.ecefPosition.x,
                (value) =>
                {
                    var xyz = this._globeAnchor.ecefPosition;
                    xyz.x = value;
                    this._globeAnchor.ecefPosition = xyz;
                },
                "ECEF X",
                @"The Earth-Centered, Earth-Fixed X-coordinate of the origin of this
                game object in meters.

                In the ECEF coordinate system, the origin is at the center of the Earth
                and the positive X axis points toward where the Prime Meridian crosses
                the Equator.");

            this._gui.Double(
                this._globeAnchor.ecefPosition.y,
                (value) =>
                {
                    var xyz = this._globeAnchor.ecefPosition;
                    xyz.y = value;
                    this._globeAnchor.ecefPosition = xyz;
                },
                "ECEF Y",
                @"The Earth-Centered, Earth-Fixed Y-coordinate of the origin of this
                game object in meters.

                In the ECEF coordinate system, the origin is at the center of the Earth
                and the positive Y axis points toward the Equator at 90 degrees longitude.");

            this._gui.Double(
                this._globeAnchor.ecefPosition.z,
                (value) =>
                {
                    var xyz = this._globeAnchor.ecefPosition;
                    xyz.z = value;
                    this._globeAnchor.ecefPosition = xyz;
                },
                "ECEF Z",
                @"The Earth-Centered, Earth-Fixed Z-coordinate of the origin of this
                game object in meters.

                In the ECEF coordinate system, the origin is at the center of the Earth
                and the positive Z axis points toward the North pole.");
        }

        private void DrawEastUpNorthRotationProperties()
        {
            GUILayout.Label("Rotation to Local East-Up-North", EditorStyles.boldLabel);

            Quaternion rotation = this._globeAnchor.localToEastUpNorthRotation;
            Vector3 eulerAngles = rotation.eulerAngles;

            this._gui.Double(
                eulerAngles.x,
                (value) =>
                {
                    eulerAngles.x = (float)value;
                    this._globeAnchor.localToEastUpNorthRotation = quaternion.EulerZXY(eulerAngles);
                },
                "X",
                @"");

            this._gui.Double(
                eulerAngles.y,
                (value) =>
                {
                    eulerAngles.y = (float)value;
                    this._globeAnchor.localToEastUpNorthRotation = quaternion.EulerZXY(eulerAngles);
                },
                "Y",
                @"");

            this._gui.Double(
                eulerAngles.z,
                (value) =>
                {
                    eulerAngles.z = (float)value;
                    this._globeAnchor.localToEastUpNorthRotation = quaternion.EulerZXY(eulerAngles);
                },
                "Z",
                @"");
        }

        private void DrawScaleProperties()
        {
            GUILayout.Label("Scale", EditorStyles.boldLabel);

            double3 positiveScale = -this._globeAnchor.localToEcefScale;

            this._gui.Double(
                positiveScale.x,
                (value) =>
                {
                    positiveScale.x = value;
                    this._globeAnchor.localToEcefScale = -positiveScale;
                },
                "Scale X",
                @"");

            this._gui.Double(
                positiveScale.y,
                (value) =>
                {
                    positiveScale.y = value;
                    this._globeAnchor.localToEcefScale = -positiveScale;
                },
                "Scale Y",
                @"");

            this._gui.Double(
                positiveScale.z,
                (value) =>
                {
                    positiveScale.z = value;
                    this._globeAnchor.localToEcefScale = -positiveScale;
                },
                "Scale Z",
                @"");
        }
    }
}