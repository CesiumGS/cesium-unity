namespace CesiumForUnity
{
    /// <summary>
    /// The type of <see cref="Cesium3DTileset"/> load that encountered an error.
    /// </summary>
    public enum Cesium3DTilesetLoadType
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
        /// A tileset.json.
        /// </summary>
        TilesetJson
    }

    /// <summary>
    /// Holds details of a <see cref="Cesium3DTileset"/> load failure.
    /// </summary>
    public struct Cesium3DTilesetLoadFailureDetails
    {
        /// <summary>
        /// The tileset that encountered the load failure.
        /// </summary>
        public Cesium3DTileset tileset;

        /// <summary>
        /// The type of request that failed to load.
        /// </summary>
        public Cesium3DTilesetLoadType type;

        /// <summary>
        /// The HTTP status code of the response that led to the failure.
        /// </summary>
        /// <remarks>
        /// If there was no response or the failure did not follow from a request, then
        /// the value of this property will be 0.
        /// </remarks>
        public long httpStatusCode;

        /// <summary>
        /// A human-readable explanation of what failed.
        /// </summary>
        public string message;

        public Cesium3DTilesetLoadFailureDetails(
            Cesium3DTileset tileset,
            Cesium3DTilesetLoadType type,
            long httpStatusCode,
            string message)
        {
            this.tileset = tileset;
            this.type = type;
            this.httpStatusCode = httpStatusCode;
            this.message = message;
        }
    }
}
