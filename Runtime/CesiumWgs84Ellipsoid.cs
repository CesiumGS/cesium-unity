using Reinterop;
using UnityEngine;
using Unity.Mathematics;

namespace CesiumForUnity
{   /// <summary>
    /// Holds static methods for ellipsoid math and transforming between geospatial coordinate systems
    /// using the World Geodetic System (WGS84) standard.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumWgs84EllipsoidImpl", "CesiumWgs84EllipsoidImpl.h", staticOnly: true)]
    public static partial class CesiumWgs84Ellipsoid
    {
        /// <summary>
        /// Gets the radii of the ellipsoid in its x-, y-, and z-directions.
        /// </summary>
        /// <returns>The radii of the ellipsoid in its x-, y-, and z-directions.</returns>
        public static partial double3 GetRadii();

        /// <summary>
        /// Gets the maximum radius of the ellipsoid in any dimension.
        /// </summary>
        /// <returns>The maximum radius of the ellipsoid.</returns>
        public static double GetMaximumRadius()
        {
            return math.cmax(CesiumWgs84Ellipsoid.GetRadii());
        }

        /// <summary>
        /// Gets the minimum radius of the ellipsoid in any dimension.
        /// </summary>
        /// <returns>The minimum radius of the ellipsoid.</returns>
        public static double GetMinimumRadius()
        {
            return math.cmin(CesiumWgs84Ellipsoid.GetRadii());
        }

        /// <summary>
        /// Scale the given Earth-Centered, Earth-Fixed position along the geodetic surface normal 
        /// so that it is on the surface of the ellipsoid. If the position is at the center of the
        /// ellipsoid, the result will be null.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF position in meters.</param>
        /// <returns>The scaled position, or null if the position is at the center of the ellipsoid.</returns>
        public static partial double3? ScaleToGeodeticSurface(double3 earthCenteredEarthFixed);

        /// <summary>
        /// Computes the normal of the plane tangent to the surface of the ellipsoid at the provided 
        /// Earth-Centered, Earth-Fixed position.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF position in meters.</param>
        /// <returns>The normal at the ECEF position</returns>
        public static partial double3 GeodeticSurfaceNormal(double3 earthCenteredEarthFixed);

        /// <summary>
        /// Convert longitude, latitude, and height to Earth-Centered, Earth-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="longitudeLatitudeHeight">
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public static partial double3
            LongitudeLatitudeHeightToEarthCenteredEarthFixed(double3 longitudeLatitudeHeight);

        /// <summary>
        /// Convert Earth-Centered, Earth-Fixed (ECEF) coordinates to longitude, latitude, and height.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters.</param>
        /// <returns>
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.
        /// </returns>
        public static partial double3
            EarthCenteredEarthFixedToLongitudeLatitudeHeight(double3 earthCenteredEarthFixed);
    }
}
