using Reinterop;
using Unity.Mathematics;
using UnityEngine;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;


namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumEllipsoidImpl", "CesiumEllipsoidImpl.h")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [CreateAssetMenu(menuName = "Cesium/Ellipsoid")]
    public partial class CesiumEllipsoid : ScriptableObject
    {
        public static CesiumEllipsoid WGS84
        {
            get
            {
                if (_cachedWgs84 == null)
                {
                    _cachedWgs84 = ScriptableObject.CreateInstance<CesiumEllipsoid>();
                    _cachedWgs84.SetRadii(CesiumWgs84Ellipsoid.GetRadii());
                }
                return _cachedWgs84;
            }
        }

        private static CesiumEllipsoid _cachedWgs84;

        [SerializeField]
        private double3 _radii = new double3(1.0, 1.0, 1.0);

        public double3 radii
        {
            get => _radii;
            set => _radii = value;
        }

        private partial double3 GetRadii();
        private partial void SetRadii(double3 newRadii);
        private partial double3? ScaleToGeodeticSurface(double3 ellipsoidCenteredEllipsoidFixed);
        private partial double3 GeodeticSurfaceNormal(double3 ellipsoidCenteredEllipsoidFixed);
        private partial double3 LongitudeLatitudeHeightToEllipsoidCenteredEllipsoidFixed(double3 longitudeLatitudeHeight);
        private partial double3 EllipsoidCenteredEllipsoidFixedToLongitudeLatitudeHeight(double3 ellipsoidCenteredEllipsoidFixed);
    }
}
