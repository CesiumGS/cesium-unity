using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that directly accesses a Tile Map Service (TMS) server. If you're using TMS
    /// via Cesium ion, use the <see cref="CesiumIonRasterOverlay"/> component instead.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumTileMapServiceRasterOverlayImpl",
        "CesiumTileMapServiceRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Tile Map Service Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumTileMapServiceRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _url = "";

        /// <summary>
        /// The base URL of the Tile Map Service (TMS).
        /// </summary>
        /// <remarks>
        /// This will often, but not always, end in <code>tilemapresource.xml</code>.
        /// </remarks>
        public string url
        {
            get => this._url;
            set
            {
                this._url = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _specifyZoomLevels = false;

        /// <summary>
        /// True to directly specify minum and maximum zoom levels available from the
        /// server, or false to automatically determine the minimum and maximum zoom
        /// levels from the server's <code>tilemapresource.xml</code> file.
        /// </summary>
        public bool specifyZoomLevels
        {
            get => this._specifyZoomLevels;
            set
            {
                this._specifyZoomLevels = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Min(0)]
        private int _minimumLevel = 0;

        /// <summary>
        /// The minimum zoom level.
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

        [SerializeField]
        [Min(0)]
        private int _maximumLevel = 10;

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