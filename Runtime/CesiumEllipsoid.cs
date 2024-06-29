using Reinterop;
using Unity.Mathematics;
using UnityEngine;


namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumEllipsoidImpl", "CesiumEllipsoidImpl.h")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    [CreateAssetMenu(menuName = "Cesium/Ellipsoid")]
    public partial class CesiumEllipsoid : ScriptableObject
    {
        /// <summary>
        /// Obtain a WGS84 ellipsoid.
        /// </summary>
        public static CesiumEllipsoid WGS84
        {
            get
            {
                if (_cachedWgs84 == null)
                {
                    _cachedWgs84 = CreateInstance<CesiumEllipsoid>();
                    _cachedWgs84.name = "WGS84";
                    _cachedWgs84.SetRadii(CesiumWgs84Ellipsoid.GetRadii());
                }

                return _cachedWgs84;
            }
        }

        private static CesiumEllipsoid _cachedWgs84;

        [SerializeField]
        private double3 _radii = new double3(1.0, 1.0, 1.0);

        internal double3 radii
        {
            get => _radii;
            set => _radii = value;
        }

        /// <summary>
        /// Gets the maximum radius of the ellipsoid in any dimension.
        /// </summary>
        /// <returns>The maximum radius of the ellipsoid.</returns>
        public double GetMaximumRadius()
        {
            return math.cmax(this.GetRadii());
        }

        /// <summary>
        /// Gets the minimum radius of the ellipsoid in any dimension.
        /// </summary>
        /// <returns>The minimum radius of the ellipsoid.</returns>
        public double GetMinimumRadius()
        {
            return math.cmin(this.GetRadii());
        }

        /// <summary>
        /// Returns the radii of this ellipsoid.
        /// </summary>
        public partial double3 GetRadii();

        /// <summary>
        /// Sets the radii of this ellipsoid to the given values.
        /// </summary>
        /// <param name="newRadii">The new (x, y, z) radii of the ellipsoid.</param>
        public partial void SetRadii(double3 newRadii);

        /// <summary>
        /// Scale the given Ellipsoid-Centered, Ellipsoid-Fixed (ECEF) position along the geodetic surface normal 
        /// so that it is on the surface of the ellipsoid. If the position is at the center of the
        /// ellipsoid, the result will be null.
        /// </summary>
        /// <param name="ellipsoidCenteredEllipsoidFixed">The ECEF position in meters.</param>
        /// <returns>The scaled position, or null if the position is at the center of the ellipsoid.</returns>
        public partial double3? ScaleToGeodeticSurface(double3 ellipsoidCenteredEllipsoidFixed);

        /// <summary>
        /// Computes the normal of the plane tangent to the surface of the ellipsoid at the provided 
        /// Ellipsoid-Centered, Ellipsoid-Fixed position.
        /// </summary>
        /// <param name="ellipsoidCenteredEllipsoidFixed">The ECEF position in meters.</param>
        /// <returns>The normal at the ECEF position</returns>
        public partial double3 GeodeticSurfaceNormal(double3 ellipsoidCenteredEllipsoidFixed);

        /// <summary>
        /// Convert longitude, latitude, and height to Ellipsoid-Centered, Ellipsoid-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="longitudeLatitudeHeight">
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public partial double3 LongitudeLatitudeHeightToCenteredFixed(double3 longitudeLatitudeHeight);

        /// <summary>
        /// Convert Ellipsoid-Centered, Ellipsoid-Fixed (ECEF) coordinates to longitude, latitude, and height.
        /// </summary>
        /// <param name="ellipsoidCenteredEllipsoidFixed">The ECEF coordinates in meters.</param>
        /// <returns>
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.
        /// </returns>
        public partial double3 CenteredFixedToLongitudeLatitudeHeight(double3 ellipsoidCenteredEllipsoidFixed);
    }
}
