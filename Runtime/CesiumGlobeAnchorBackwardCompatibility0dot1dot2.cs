using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using System;
using UnityEditorInternal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CesiumForUnity
{
    public enum CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2
    {
        None,
        LongitudeLatitudeHeight,
        EarthCenteredEarthFixed,
        UnityCoordinates
    }

    [ExecuteInEditMode]
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-1000000)]
    internal class CesiumGlobeAnchorBackwardCompatibility0dot1dot2 : CesiumGlobeAnchor, IBackwardCompatibilityComponent<CesiumGlobeAnchor>
    {
        public new bool _adjustOrientationForGlobeWhenMoving = true;
        public new bool _detectTransformChanges = true;
        public CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2 _positionAuthority = CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.None;
        public double _latitude = 0.0;
        public double _longitude = 0.0;
        public double _height = 0.0;
        public double _ecefX = 0.0;
        public double _ecefY = 0.0;
        public double _ecefZ = 0.0;
        public double _unityX = 0.0;
        public double _unityY = 0.0;
        public double _unityZ = 0.0;

#if UNITY_EDITOR
        void OnEnable()
        {
            CesiumBackwardCompatibility<CesiumGlobeAnchor>.Upgrade(this);
        }
#endif

        public string VersionToBeUpgraded => "v0.2.0";

        public void Upgrade(GameObject gameObject, CesiumGlobeAnchor upgraded)
        {
            // Temporarily disable orientation adjustment so that we can set the position without
            // risking rotating the object.
            upgraded.adjustOrientationForGlobeWhenMoving = false;
            upgraded.detectTransformChanges = false;
            
            switch (this._positionAuthority)
            {
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.None:
                    // This shouldn't happen, but if it does, just leave the position at the default.
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.LongitudeLatitudeHeight:
                    upgraded.longitudeLatitudeHeight = new double3(this._longitude, this._latitude, this._height);
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.EarthCenteredEarthFixed:
                    upgraded.positionGlobeFixed = new double3(this._ecefX, this._ecefY, this._ecefZ);
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.UnityCoordinates:
                    // Any backward compatibility for CesiumGeoreference must have a more negative
                    // DefaultExecutionOrder so that the real CesiumGeoreference is created first.
                    // If this component is not nested inside a CesiumGeoreference, converting Unity coordinates
                    // to ECEF is impossible, so just keep the default position.
                    CesiumGeoreference georeference = this.GetComponentInParent<CesiumGeoreference>();
                    if (georeference != null)
                    {
                        georeference.Initialize();
                        double3 ecef = georeference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(this._unityX, this._unityY, this._unityZ));
                        upgraded.positionGlobeFixed = ecef;
                    }
                    break;
            }

            upgraded.adjustOrientationForGlobeWhenMoving = this._adjustOrientationForGlobeWhenMoving;
            upgraded.detectTransformChanges = this._detectTransformChanges;
        }
    }
}
