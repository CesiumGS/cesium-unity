using Reinterop;
using Unity.Mathematics;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Describes a curve that's a section of an ellipse that lies on a plane intersecting the center of the earth and 
    /// both the source and destination points on a WGS84 ellipsoid. This curve can be sampled at any point along its length.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumSimplePlanarEllipsoidCurveImpl", "CesiumSimplePlanarEllipsoidCurveImpl.h")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumSimplePlanarEllipsoidCurve
    {
        /// <summary>
        /// Creates a new <see cref="CesiumSimplePlanarEllipsoidCurve"/> object from a pair of 
        /// Earth-Centered, Earth-Fixed coordinates describing the beginning and end points of the curve.
        /// </summary>
        /// <param name="sourceEcef">The start point of the curve.</param>
        /// <param name="destinationEcef">The end point of the curve.</param>
        /// <returns>
        /// A <see cref="CesiumSimplePlanarEllipsoidCurve"/> if a curve can successfully be 
        /// created between the two points, or null otherwise.
        /// </returns>
        public static CesiumSimplePlanarEllipsoidCurve FromEarthCenteredEarthFixedCoordinates(double3 sourceEcef, double3 destinationEcef)
        {
            CesiumSimplePlanarEllipsoidCurve curve = new CesiumSimplePlanarEllipsoidCurve();
            if (!curve.CreateFromEarthCenteredEarthFixedCoordinates(sourceEcef, destinationEcef))
            {
                return null;
            }

            return curve;
        }

        /// <summary>
        /// Creates a new <see cref="CesiumSimplePlanarEllipsoidCurve"/> object from a pair of cartographic 
        /// coordinates (Longitude, Latitude, and Height) describing the beginning and end points of the curve.
        /// </summary>
        /// <param name="sourceLlh">The start point of the curve.</param>
        /// <param name="destinationLlh">The end point of the curve.</param>
        /// <returns>
        /// A <see cref="CesiumSimplePlanarEllipsoidCurve"/> if a curve can successfully be 
        /// created between the two points, or null otherwise.
        /// </returns>
        public static CesiumSimplePlanarEllipsoidCurve FromLongituteLatitudeHeight(double3 sourceLlh, double3 destinationLlh)
        {
            CesiumSimplePlanarEllipsoidCurve curve = new CesiumSimplePlanarEllipsoidCurve();
            if (!curve.CreateFromLongitudeLatitudeHeight(sourceLlh, destinationLlh))
            {
                return null;
            }

            return curve;
        }

        #region Implementation
        /// <summary>
        /// Samples the curve at the given percentage of its length.
        /// </summary>
        /// <param name="percentage">
        /// The percentage of the curve's length to sample at, where 0 is the beginning and 1 is the end.
        /// This value will be clamped to the range [0..1].
        /// </param>
        /// <param name="additionalHeight">
        /// The height above the earth at this position will be calculated by interpolating between the height
        /// at the beginning and end points of the curve, based on the value of <paramref name="percentage"/>.
        /// This parameter specifies an additional offset to add to the height at this position.
        /// </param>
        /// <returns>The position of the given point on this curve in Earth-Centered, Earth-Fixed coordinates.</returns>
        public partial double3 GetPosition(double percentage, double additionalHeight = 0.0);

        private partial bool CreateFromEarthCenteredEarthFixedCoordinates(double3 sourceEcef, double3 destinationEcef);
        private partial bool CreateFromLongitudeLatitudeHeight(double3 sourceLlh, double3 destinationLlh);

        private CesiumSimplePlanarEllipsoidCurve()
        {
            CreateImplementation();
        }
        #endregion
    }
}
