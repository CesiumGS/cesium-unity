using Unity.Mathematics;
using UnityEngine;

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
    public class CesiumGlobeAnchorBackwardCompatibility0dot1dot2 : BackwardCompatibilityComponent
    {
        public bool _adjustOrientationForGlobeWhenMoving = true;
        public bool _detectTransformChanges = true;
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

        protected override string UpgradedComponent => "CesiumGlobeAnchor";
        protected override string UpgradedVersion => "v0.1.2";

        protected override void Upgrade()
        {
            // Try getting the real CesiumGlobeAnchor before adding it, because it may have been
            // created automatically by Unity because of the RequireComponent attribute.
            GameObject go = this.gameObject;
            CesiumGlobeAnchor upgraded = go.GetComponent<CesiumGlobeAnchor>();
            if (upgraded == null)
            {
                upgraded = go.AddComponent<CesiumGlobeAnchor>();
            }

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
                    upgraded.SetPositionLongitudeLatitudeHeight(this._longitude, this._latitude, this._height);
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.EarthCenteredEarthFixed:
                    upgraded.SetPositionEarthCenteredEarthFixed(this._ecefX, this._ecefY, this._ecefZ);
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
                        upgraded.SetPositionEarthCenteredEarthFixed(ecef.x, ecef.y, ecef.z);
                    }
                    break;
            }

            upgraded.adjustOrientationForGlobeWhenMoving = this._adjustOrientationForGlobeWhenMoving;
            upgraded.detectTransformChanges = this._detectTransformChanges;
        }
    }
}
