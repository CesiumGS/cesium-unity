using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// The asynchronous result of a call to <see cref="Cesium3DTileset.SampleHeightMostDetailed"/>.
    /// </summary>
    public class CesiumSampleHeightResult
    {
        /// <summary>
        /// The positions and sampled heights. The X component is Longitude (degrees), the
        /// Y component is Latitude (degrees), and the Z component is Height (meters) above
        /// the ellipsoid (usually WGS84).
        /// </summary>
        /// <remarks>
        /// <para>
        /// The longitudes and latitudes will match the values at the same index in the
        /// original input positions. Each height will either be the height sampled
        /// from the tileset at that position, or the original input height if the
        /// height could not be sampled. To determine which, look at the value of
        /// <see cref="CesiumSampleHeightResult.heightSampled"/> at the same index.
        /// </para>
        /// <para>
        /// The returned height is is measured from the ellipsoid, which is usually WGS84.
        /// It should not be confused with a height about Mean Sea Level.
        /// </para>
        /// </remarks>
        public double3[] longitudeLatitudeHeightPositions { get; set; }

        /// <summary>
        /// Specifies whether the height for the position at this index was sampled successfully.
        /// </summary>
        /// <remarks>
        /// If true, <see cref="CesiumSampleHeightResult.longitudeLatitudeHeightPositions"/> has
        /// a valid height sampled from the tileset at this index. If false, the height
        /// could not be sampled for the position at this index, and so the height in
        /// <see cref="CesiumSampleHeightResult.longitudeLatitudeHeightPositions"/> is unchanged
        /// from the original input height.
        /// </remarks>
        public bool[] heightSampled { get; set; }

        /// <summary>
        /// Any warnings that occurred while sampling heights.
        /// </summary>
        public string[] warnings { get; set; }
    }
}
