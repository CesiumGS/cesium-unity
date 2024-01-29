using Reinterop;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace CesiumForUnity
{
    /// <summary>
    /// Describes a flight path along the surface of a WGS84 ellipsoid interpolating between two points. 
    /// The path is continuous and can be sampled, for example, to move a camera between two points on the ellipsoid.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumGlobeFlightPathImpl", "CesiumGlobeFlightPathImpl.h")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumGlobeFlightPath
    {
        /// <summary>
        /// The length of this path in meters.
        /// </summary>
        /// <remarks>
        /// The length is measured "as the crow flies" in a straight line from the start point to end point.
        /// </remarks>
        public double Length => GetLength();

        /// <summary>
        /// Creates a new <see cref="CesiumGlobeFlightPath"/> object from a pair of Earth-Centered, Earth-Fixed coordinates
        /// describing the beginning and end points of the path.
        /// </summary>
        /// <param name="sourceEcef">The start point of the path.</param>
        /// <param name="destinationEcef">The end point of the path.</param>
        /// <returns>A <see cref="CesiumGlobeFlightPath"/> if a path can successfully be created between the two points, or null otherwise.</returns>
        public static CesiumGlobeFlightPath FromEarthCenteredEarthFixedCoordinates(double3 sourceEcef, double3 destinationEcef)
        {
            CesiumGlobeFlightPath path = new CesiumGlobeFlightPath();
            if (!path.CreateFromEarthCenteredEarthFixedCoordinates(sourceEcef, destinationEcef))
            {
                return null;
            }

            return path;
        }

        /// <summary>
        /// Creates a new <see cref="CesiumGlobeFlightPath"/> object from a pair of cartographic coordinates 
        /// (Longitude, Latitude, and Height) describing the beginning and end points of the path.
        /// </summary>
        /// <param name="sourceLlh">The start point of the path.</param>
        /// <param name="destinationLlh">The end point of the path.</param>
        /// <returns>A <see cref="CesiumGlobeFlightPath"/> if a path can successfully be created between the two points, or null otherwise.</returns>
        public static CesiumGlobeFlightPath FromLongituteLatitudeHeight(double3 sourceLlh, double3 destinationLlh)
        {
            CesiumGlobeFlightPath path = new CesiumGlobeFlightPath();
            if (!path.CreateFromLongitudeLatitudeHeight(sourceLlh, destinationLlh))
            {
                return null;
            }

            return path;
        }

        #region Implementation
        /// <summary>
        /// Samples the flight path at the given percentage of its length.
        /// </summary>
        /// <param name="percentage">
        /// The percentage of the flight path's length to sample at, where 0 is the beginning and 1 is the end.
        /// This value will be clamped to the range [0..1].
        /// </param>
        /// <param name="additionalHeight">
        /// The height above the earth at this position will be calculated by interpolating between the height
        /// at the beginning and end points of the curve, based on the value of <paramref name="percentage"/>.
        /// This parameter specifies an additional offset to add to the height at this position.
        /// </param>
        /// <returns>The position of the given point on this path in Earth-Centered, Earth-Fixed coordinates.</returns>
        public partial double3 GetPosition(double percentage, double additionalHeight = 0.0);

        private partial bool CreateFromEarthCenteredEarthFixedCoordinates(double3 sourceEcef, double3 destinationEcef);
        private partial bool CreateFromLongitudeLatitudeHeight(double3 sourceLlh, double3 destinationLlh);
        private partial double GetLength();
        #endregion
    }
}
