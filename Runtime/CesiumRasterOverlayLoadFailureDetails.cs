namespace CesiumForUnity
{
    public enum CesiumRasterOverlayLoadType
    {
        /**
         * An unknown load error.
         */
        Unknown,
        /**
         * A Cesium ion asset endpoint.
         */
        CesiumIon,
        /**
         * An initial load needed to create the overlay's tile provider.
         */
        TileProvider
    }

    public struct CesiumRasterOverlayLoadFailureDetails
    {
        /**
         * The overlay that encountered the load failure.
         */
        public CesiumRasterOverlay overlay;

        /**
         * The type of request that failed to load.
         */
        public CesiumRasterOverlayLoadType type;

        /**
         * The HTTP status code of the response that led to the failure.
         *
         * If there was no response or the failure did not follow from a request, then
         * the value of this property will be 0.
         */
        public long httpStatusCode;

        /**
         * A human-readable explanation of what failed.
         */
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
