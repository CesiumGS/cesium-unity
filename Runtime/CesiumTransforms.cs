using Reinterop;
using System;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumTransformsImpl", "CesiumTransformsImpl.h")]
    public static partial class CesiumTransforms
    {
        /// <summary>
        /// Convert longitude, latitude, and height to Earth-Centered, Earth-Fixed (ECEF) coordinates.
        /// </summary>
        /// <param name="longitudeLatitudeHeight">
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.</param>
        /// <returns>The ECEF coordinates in meters.</returns>
        public static partial CesiumVector3
            LongitudeLatitudeHeightToEarthCenteredEarthFixed(CesiumVector3 longitudeLatitudeHeight);

        /// <summary>
        /// Convert Earth-Centered, Earth-Fixed (ECEF) coordinates to longitude, latitude, and height.
        /// </summary>
        /// <param name="earthCenteredEarthFixed">The ECEF coordinates in meters.</param>
        /// <returns>
        /// The longitude (X) and latitude (Y) are in degrees. The height (Z) is in meters above the ellipsoid,
        /// and should not be confused with a geoid, orthometric, or mean sea level height.
        /// </returns>
        public static partial CesiumVector3
            EarthCenteredEarthFixedToLongitudeLatitudeHeight(CesiumVector3 earthCenteredEarthFixed);

        public static partial CesiumVector3 ScaleCartesianToEllipsoidGeodeticSurface(CesiumVector3 cartesian);
    }
}
