using Reinterop;
using System;

namespace CesiumForUnity
{
    /// <summary>
    /// Represents a Polygon in a GeoJSON document.
    /// </summary>
    /// <remarks>
    /// A GeoJSON Polygon consists of one or more linear rings. The first ring
    /// is the exterior ring, and any subsequent rings are holes. Use
    /// <see cref="GetPolygonRings"/> to access the rings as an array of
    /// <see cref="CesiumGeoJsonLineString"/>.
    /// </remarks>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGeoJsonPolygonImpl",
        "CesiumGeoJsonPolygonImpl.h")]
    public partial class CesiumGeoJsonPolygon
    {
        /// <summary>
        /// Internal constructor used by native code.
        /// </summary>
        internal CesiumGeoJsonPolygon()
        {
            CreateImplementation();
        }

        /// <summary>
        /// Gets the rings of this polygon as an array of line strings.
        /// </summary>
        /// <remarks>
        /// The first element is the exterior ring, and any subsequent elements
        /// are interior rings (holes).
        /// </remarks>
        /// <returns>An array of <see cref="CesiumGeoJsonLineString"/> representing
        /// the polygon's rings.</returns>
        public partial CesiumGeoJsonLineString[] GetPolygonRings();

        /// <summary>
        /// Releases native resources associated with this polygon.
        /// </summary>
        internal partial void DisposeNative();

        /// <summary>
        /// Finalizer to release native resources.
        /// </summary>
        ~CesiumGeoJsonPolygon()
        {
            DisposeNative();
        }
    }
}
