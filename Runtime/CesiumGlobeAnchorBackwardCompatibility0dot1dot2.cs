using Unity.Mathematics;
using UnityEngine;

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
    public class CesiumGlobeAnchorBackwardCompatibility0dot1dot2 : MonoBehaviour
    {
        [SerializeField]
        public bool _adjustOrientationForGlobeWhenMoving = true;
        [SerializeField]
        public bool _detectTransformChanges = true;
        [SerializeField]
        public CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2 _positionAuthority = CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.None;
        [SerializeField]
        public double _latitude = 0.0;
        [SerializeField]
        public double _longitude = 0.0;
        [SerializeField]
        public double _height = 0.0;
        [SerializeField]
        public double _ecefX = 0.0;
        [SerializeField]
        public double _ecefY = 0.0;
        [SerializeField]
        public double _ecefZ = 0.0;
        [SerializeField]
        public double _unityX = 0.0;
        [SerializeField]
        public double _unityY = 0.0;
        [SerializeField]
        public double _unityZ = 0.0;

        void OnEnable()
        {
            // Try getting the real CesiumGlobeAnchor before adding it, because it may have been
            // created automatically by Unity because of the RequireComponent attribute.
            CesiumGlobeAnchor upgraded = this.gameObject.GetComponent<CesiumGlobeAnchor>();
            if (upgraded == null)
            {
                upgraded = this.gameObject.AddComponent<CesiumGlobeAnchor>();
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

            Helpers.Destroy(this);
        }
    }
}
