using Reinterop;
using System;
using Unity.Mathematics;

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
