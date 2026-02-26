using System;
using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a LineString in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A LineString is a sequence of two or more positions forming a line.
    /// Each point is a <see cref="double3"/> containing longitude, latitude,
    /// and optional height (LLH) coordinates.
    /// </remarks>
    public class CesiumGeoJsonLineString
    {
        /// <summary>
        /// Gets the points in longitude, latitude, height that make up this line string.
        /// </summary>
        public double3[] points { get; internal set; }

        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonLineString()
        {
            points = Array.Empty<double3>();
        }
    }
}
