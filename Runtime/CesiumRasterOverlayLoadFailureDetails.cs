namespace CesiumForUnity
{
    /// <summary>
    /// The type of <see cref="CesiumRasterOverlay"/> load that encountered an error.
    /// </summary>
    public enum CesiumRasterOverlayLoadType
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
        /// An initial load needed to create the overlay's tile provider.
        /// </summary>
        TileProvider
    }

    /// <summary>
    /// Holds details of a <see cref="CesiumRasterOverlay"/> load failure.
    /// </summary>
    public struct CesiumRasterOverlayLoadFailureDetails
    {
        /// <summary>
        /// The overlay that encountered the load failure.
        /// </summary>
        public CesiumRasterOverlay overlay;

        /// <summary>
        /// The type of request that failed to load.
        /// </summary>
        public CesiumRasterOverlayLoadType type;

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

        public CesiumRasterOverlayLoadFailureDetails(
            CesiumRasterOverlay overlay,
            CesiumRasterOverlayLoadType type,
            long httpStatusCode,
            string message)
        {
            this.overlay = overlay;
            this.type = type;
            this.httpStatusCode = httpStatusCode;
            this.message = message;
        }
    }
}
