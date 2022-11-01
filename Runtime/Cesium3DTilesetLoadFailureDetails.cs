namespace CesiumForUnity
{
    public enum Cesium3DTilesetLoadType
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
         * A tileset.json.
         */
        TilesetJson
    }

    public struct Cesium3DTilesetLoadFailureDetails
    {
        /**
         * The tileset that encountered the load failure.
         */
        public Cesium3DTileset tileset;

        /**
         * The type of request that failed to load.
         */
        public Cesium3DTilesetLoadType type;

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
