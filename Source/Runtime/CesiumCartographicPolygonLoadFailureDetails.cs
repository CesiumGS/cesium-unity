namespace CesiumForUnity
{
    /// <summary>
    /// The type of <see cref="CesiumCartographicPolygon"/> load that encountered an error.
    /// </summary>
    public enum CesiumCartographicPolygonLoadType
    {
        /// <summary>
        /// An unknown load error.
        /// </summary>
        Unknown,

        /// <summary>
        /// A Cesium ion asset endpoint.
        /// </summary>
        CesiumIon,

        /// <summary>
        /// A URL endpoint.
        /// </summary>
        Url
    }

    /// <summary>
    /// Holds details of a <see cref="CesiumCartographicPolygon"/> load failure.
    /// </summary>
    public struct CesiumCartographicPolygonLoadFailureDetails
    {
        /// <summary>
        /// The polygon that encountered the load failure.
        /// </summary>
        public CesiumCartographicPolygon polygon;

        public CesiumCartographicPolygonLoadType type;

        public string message;

        public CesiumCartographicPolygonLoadFailureDetails(
            CesiumCartographicPolygon polygon,
            CesiumCartographicPolygonLoadType type,
            string message)
        {
            this.polygon = polygon;
            this.type = type;
            this.message = message;
        }
    }
}