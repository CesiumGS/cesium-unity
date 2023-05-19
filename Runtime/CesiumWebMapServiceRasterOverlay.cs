using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that directly accesses a Web Map Service (WMS) server.
    /// https://www.ogc.org/standards/wms
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumWebMapServiceRasterOverlayImpl",
        "CesiumWebMapServiceRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Web Map Service Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumWebMapServiceRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _baseUrl = "";

        /// <summary>
        /// The base URL of the Web Map Service (WMS).
        /// </summary>
        /// <remarks>
        /// This URL should not include query parameters. For example:
        /// https://services.ga.gov.au/gis/services/NM_Culture_and_Infrastructure/MapServer/WMSServer
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

        [SerializeField]
        private string _layers = "";

        /// <summary>
        /// Comma-separated layer names to request from the server.
        /// </summary>
        public string layers
        {
            get => this._layers;
            set
            {
                this._layers = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private int _tileWidth = 256;

        /// <summary>
        /// Image width
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

        [SerializeField]
        private int _tileHeight = 256;

        /// <summary>
        /// Image height
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

        [SerializeField]
        [Min(0)]
        private int _minimumLevel = 0;

        /// <summary>
        /// The minimum zoom level.
        /// </summary>
        /// <remarks>
        /// Take care when specifying this that the number of tiles at the minimum
        /// level is small, such as four or less. A larger number is likely to
        /// result in rendering problems.
        /// </remarks>
        public int minimumLevel
        {
            get => this._minimumLevel;
            set
            {
                this._minimumLevel = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Min(0)]
        private int _maximumLevel = 14;

        /// <summary>
        /// The maximum zoom level.
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

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}