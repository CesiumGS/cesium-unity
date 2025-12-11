using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CesiumForUnity
{
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    [DefaultExecutionOrder(-1000000)]
    internal class CesiumGlobeAnchorBackwardCompatibility0dot2dot0 : CesiumGlobeAnchor, IBackwardCompatibilityComponent<CesiumGlobeAnchor>
    {
        public enum CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2
        {
            None,
            LongitudeLatitudeHeight,
            EarthCenteredEarthFixed,
            UnityCoordinates
        }

        [FormerlySerializedAs("_adjustOrientationForGlobeWhenMoving")]
        public bool _adjustOrientationForGlobeWhenMoving0dot2dot0 = false;
        [FormerlySerializedAs("_detectTransformChanges")]
        public bool _detectTransformChanges0dot2dot0 = false;
        [FormerlySerializedAs("_positionAuthority")]
        public CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2 _positionAuthority0dot2dot0 = CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.None;
        [FormerlySerializedAs("_latitude")]
        public double _latitude0dot2dot0 = 0.0;
        [FormerlySerializedAs("_longitude")]
        public double _longitude0dot2dot0 = 0.0;
        [FormerlySerializedAs("_height")]
        public double _height0dot2dot0 = 0.0;
        [FormerlySerializedAs("_ecefX")]
        public double _ecefX0dot2dot0 = 0.0;
        [FormerlySerializedAs("_ecefY")]
        public double _ecefY0dot2dot0 = 0.0;
        [FormerlySerializedAs("_ecefZ")]
        public double _ecefZ0dot2dot0 = 0.0;
        [FormerlySerializedAs("_unityX")]
        public double _unityX0dot2dot0 = 0.0;
        [FormerlySerializedAs("_unityY")]
        public double _unityY0dot2dot0 = 0.0;
        [FormerlySerializedAs("_unityZ")]
        public double _unityZ0dot2dot0 = 0.0;

#if UNITY_EDITOR
        [CustomEditor(typeof(CesiumGlobeAnchorBackwardCompatibility0dot2dot0))]
        internal class CesiumGlobeAnchorBackwardCompatibility0dot2dot0Editor : Editor
        {
            public override void OnInspectorGUI()
            {
                if (GUILayout.Button("Upgrade"))
                {
                    CesiumGlobeAnchorBackwardCompatibility0dot2dot0 o = (CesiumGlobeAnchorBackwardCompatibility0dot2dot0)this.target;
                    CesiumBackwardCompatibility<CesiumGlobeAnchor>.Upgrade(o);
                }
            }
        }

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
            
            switch (this._positionAuthority0dot2dot0)
            {
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.None:
                    // This shouldn't happen, but if it does, just leave the position at the default.
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.LongitudeLatitudeHeight:
                    upgraded.longitudeLatitudeHeight = new double3(this._longitude0dot2dot0, this._latitude0dot2dot0, this._height0dot2dot0);
                    break;
                case CesiumGlobeAnchorPositionAuthorityBackwardCompatibility0dot1dot2.EarthCenteredEarthFixed:
                    upgraded.positionGlobeFixed = new double3(this._ecefX0dot2dot0, this._ecefY0dot2dot0, this._ecefZ0dot2dot0);
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
                        double3 ecef = georeference.TransformUnityPositionToEarthCenteredEarthFixed(new double3(this._unityX0dot2dot0, this._unityY0dot2dot0, this._unityZ0dot2dot0));
                        upgraded.positionGlobeFixed = ecef;
                    }
                    break;
            }

            upgraded.adjustOrientationForGlobeWhenMoving = this._adjustOrientationForGlobeWhenMoving0dot2dot0;
            upgraded.detectTransformChanges = this._detectTransformChanges0dot2dot0;
        }
    }
}
