namespace CesiumForUnity
{
    /// <summary>
    /// The type of ID on a GeoJSON feature.
    /// </summary>
    public enum CesiumGeoJsonFeatureIdType
    {
        /// <summary>
        /// The feature has no ID.
        /// </summary>
        None = 0,

        /// <summary>
        /// The feature has an integer ID.
        /// </summary>
        Integer = 1,

        /// <summary>
        /// The feature has a string ID.
        /// </summary>
        String = 2
    }
}
