using Unity.Mathematics;

namespace CesiumForUnity
{
    /// <summary>
    /// The asynchronous result of a call to <see cref="Cesium3DTileset.SampleHeightMostDetailed"/>.
    /// </summary>
    public class CesiumSampleHeightResult
    {
        /// <summary>
        /// The positions and their sampled heights. The X component is Longitude (degrees),
        /// the Y component is Latitude (degrees), and the Z component is Height (meters) 
        /// above the ellipsoid (usually WGS84).
        /// </summary>
        /// <remarks>
        /// <para>
        /// For each resulting position, its longitude and latitude values will match
        /// values from its input. Its height will either be the height sampled from
        /// the tileset at that position, or the original input height if the sample
        /// was unsuccessful. To determine which, look at the value of
        /// <see cref="CesiumSampleHeightResult.heightSampled"/> at the same index.
        /// </para>
        /// <para>
        /// The returned height is measured from the ellipsoid (usually WGS84) and
        /// should not be confused with a height above Mean Sea Level.
        /// </para>
        /// </remarks>
        public double3[] longitudeLatitudeHeightPositions { get; set; }

        /// <summary>
        /// Indicates whether the height for the position at the corresponding index was sampled
        /// successfully.
        /// </summary>
        /// <remarks>
        /// If true, then the corresponding position in
        /// <see cref="CesiumSampleHeightResult.longitudeLatitudeHeightPositions"/> uses
        /// the height sampled from the tileset. If false, the height could not be sampled for 
        /// the position, so its height is the same as the original input height.
        /// </remarks>
        public bool[] heightSampled { get; set; }

        /// <summary>
        /// Any warnings that occurred while sampling heights.
        /// </summary>
        public string[] warnings { get; set; }
    }
}
