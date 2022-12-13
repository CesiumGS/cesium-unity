using Reinterop;
using UnityEngine;
using Unity.Mathematics;

namespace CesiumForUnity
{   /// <summary>
    /// Holds static methods for ellipsoid math using the World Geodetic System (WGS84) standard.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumEllipsoidImpl", "CesiumEllipsoidImpl.h")]
    public static partial class CesiumEllipsoid
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
            return math.cmax(CesiumEllipsoid.GetRadii());
        }

        /// <summary>
        /// Gets the minimum radius of the ellipsoid in any dimension.
        /// </summary>
        /// <returns>The minimum radius of the ellipsoid.</returns>
        public static double GetMinimumRadius()
        {
            return math.cmin(CesiumEllipsoid.GetRadii());
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
    }
}
