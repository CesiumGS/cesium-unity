using System;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a Polygon in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON Polygon consists of one or more linear rings. The first ring
    /// is the exterior ring, and any subsequent rings are holes.
    /// </remarks>
    public class CesiumGeoJsonPolygon
    {
        /// <summary>
        /// Gets the rings of this polygon as an array of line strings.
        /// </summary>
        /// <remarks>
        /// The first element is the exterior ring, and any subsequent elements
        /// are interior rings (holes).
        /// </remarks>
        public CesiumGeoJsonLineString[] Rings { get; internal set; }

        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonPolygon()
        {
            Rings = Array.Empty<CesiumGeoJsonLineString>();
        }
    }
}
