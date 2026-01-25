namespace CesiumForUnity
{
    /// <summary>
    /// The type of a GeoJSON object.
    /// </summary>
    public enum CesiumGeoJsonObjectType
    {
        /// <summary>
        /// A Point geometry - a single position.
        /// </summary>
        Point = 0,

        /// <summary>
        /// A MultiPoint geometry - multiple positions.
        /// </summary>
        MultiPoint = 1,

        /// <summary>
        /// A LineString geometry - a sequence of positions forming a line.
        /// </summary>
        LineString = 2,

        /// <summary>
        /// A MultiLineString geometry - multiple line strings.
        /// </summary>
        MultiLineString = 3,

        /// <summary>
        /// A Polygon geometry - a closed shape with optional holes.
        /// </summary>
        Polygon = 4,

        /// <summary>
        /// A MultiPolygon geometry - multiple polygons.
        /// </summary>
        MultiPolygon = 5,

        /// <summary>
        /// A GeometryCollection - a collection of geometry objects.
        /// </summary>
        GeometryCollection = 6,

        /// <summary>
        /// A Feature - a geometry with associated properties.
        /// </summary>
        Feature = 7,

        /// <summary>
        /// A FeatureCollection - a collection of features.
        /// </summary>
        FeatureCollection = 8
    }
}
