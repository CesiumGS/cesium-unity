using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that directly accesses a Web Map Tile Service (WMTS) server.
    /// https://www.ogc.org/standards/wmts
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumWebMapTileServiceRasterOverlayImpl",
        "CesiumWebMapTileServiceRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Web Map Tile Service Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumWebMapTileServiceRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField] private string _baseUrl = "";

        /// <summary>
        /// The base URL of the Web Map Tile Service (WMTS).
        /// </summary>
        /// <remarks>
        /// This URL should not include query parameters. For example:
        /// https://services.ga.gov.au/gis/services/NM_Culture_and_Infrastructure/MapServer/WMTSServer
        /// </remarks>
        public string baseUrl
        {
            get => this._baseUrl;
            set
            {
                this._baseUrl = value;
                this.Refresh();
            }
        }

        [SerializeField] private string _layer = "";

        /// <summary>
        /// The layer name for WMTS requests.
        /// </summary>
        public string layer
        {
            get => this._layer;
            set
            {
                this._layer = value;
                this.Refresh();
            }
        }

        [SerializeField] private string _style = "default";

        /// <summary>
        /// The style name for WMTS requests, default value is "default".
        /// </summary>
        public string style
        {
            get => this._style;
            set
            {
                this._style = value;
                this.Refresh();
            }
        }

        [SerializeField] private string _format = "image/jpeg";

        /// <summary>
        /// The MIME type for images to retrieve from the server, default value is "image/jpeg".
        /// </summary>
        public string format
        {
            get => this._format;
            set
            {
                this._format = value;
                this.Refresh();
            }
        }

        [SerializeField] private string _tileMatrixSetID = "";

        /// <summary>
        /// The identifier of the TileMatrixSet to use for WMTS requests.
        /// </summary>
        public string tileMatrixSetID
        {
            get => this._tileMatrixSetID;
            set
            {
                this._tileMatrixSetID = value;
                this.Refresh();
            }
        }

        [SerializeField] private int _minimumLevel = 0;

        /// <summary>
        /// The minimum level-of-detail supported by the imagery provider, default value is 0.
        /// </summary>
        public int minimumLevel
        {
            get => this._minimumLevel;
            set
            {
                this._minimumLevel = value;
                this.Refresh();
            }
        }

        [SerializeField] private int _maximumLevel = 25;

        /// <summary>
        /// The maximum level-of-detail supported by the imagery provider, default value is 25.
        /// </summary>
        public int maximumLevel
        {
            get => this._maximumLevel;
            set
            {
                this._maximumLevel = value;
                this.Refresh();
            }
        }

        [SerializeField] private int _tileWidth = 256;

        /// <summary>
        /// Pixel width of image tiles, default value is 256.
        /// </summary>
        public int tileWidth
        {
            get => this._tileWidth;
            set
            {
                this._tileWidth = value;
                this.Refresh();
            }
        }

        [SerializeField] private int _tileHeight = 256;

        /// <summary>
        /// Pixel height of image tiles, default value is 256.
        /// </summary>
        public int tileHeight
        {
            get => this._tileHeight;
            set
            {
                this._tileHeight = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}