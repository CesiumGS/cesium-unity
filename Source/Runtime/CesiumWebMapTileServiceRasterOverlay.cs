using Reinterop;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Specifies the type of projection used for projecting a Web Map Tile Service raster overlay.
    /// </summary>
    public enum CesiumWebMapTileServiceRasterOverlayProjection
    {
        /// <summary>
        /// The raster overlay is projected using Web Mercator.
        /// </summary>
        WebMercator,

        /// <summary>
        /// The raster overlay is projected using a geographic projection.
        /// </summary>
        Geographic
    }

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
        #region Fields

        [SerializeField]
        private string _baseUrl = "";

        /// <summary>
        /// The base URL of the Web Map Tile Service (WMTS).
        /// </summary>
        /// <remarks>
        /// This URL should not include query parameters. For example:
        /// https://tile.openstreetmap.org/{TileMatrix}/{TileCol}/{TileRow}.png
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
        private string _layer = "";

        /// <summary>
        /// The layer name to use for WMTS requests.
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

        [SerializeField]
        private string _style = "";

        /// <summary>
        /// The style name to use for WMTS requests.
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

        [SerializeField]
        private string _format = "image/jpeg";

        /// <summary>
        /// The MIME type for images to retrieve from the server. The default value is "image/jpeg".
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

        [SerializeField]
        private string _tileMatrixSetID = "";

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

        [SerializeField]
        private string _tileMatrixSetLabelPrefix;

        /// <summary>
        /// The prefix to use for the tile matrix set labels. For instance, setting 
        /// "EPSG:4326:" as the prefix generates the list ["EPSG:4326:0", "EPSG:4326:1",
        /// "EPSG:4326:2", ...].
        /// Only applicable when <see cref="specifyTileMatrixSetLabels"/> is false.
        /// </summary>
        public string tileMatrixSetLabelPrefix
        {
            get => this._tileMatrixSetLabelPrefix;
            set
            {
                this._tileMatrixSetLabelPrefix = value;
                this.Refresh();
            }
        }


        [SerializeField]
        private bool _specifyTileMatrixSetLabels = false;

        /// <summary>
        /// Set this to true to manually specify the tile matrix set labels. If false, 
        /// the labels will be constructed from the specified levels and prefix (if one is specified).
        /// </summary>
        public bool specifyTileMatrixSetLabels
        {
            get => this._specifyTileMatrixSetLabels;
            set
            {
                this._specifyTileMatrixSetLabels = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private List<string> _tileMatrixSetLabels;

        /**
         * The manually specified tile matrix set labels. Only applicable when 
         * <see cref="specifyTileMatrixSetLabels"/> is true.
         */
        public List<string> tileMatrixSetLabels
        {
            get => this._tileMatrixSetLabels;
            set
            {
                this._tileMatrixSetLabels = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private CesiumWebMapTileServiceRasterOverlayProjection _projection = CesiumWebMapTileServiceRasterOverlayProjection.WebMercator;

        /// <summary>
        /// The type of projection used to project the WMTS imagery onto the globe. 
        /// For instance, EPSG:4326 uses geographic projection and EPSG:3857 uses Web Mercator.
        /// </summary>
        public CesiumWebMapTileServiceRasterOverlayProjection projection
        {
            get => this._projection;
            set
            {
                this._projection = value;
                this.Refresh();
            }
        }

        [SerializeField]
        bool _specifyTilingScheme = false;

        /// <summary>
        /// Set this to true to specify the quadtree tiling scheme according to the 
        /// specified projection, root tile numbers, and bounding rectangle.
        /// If false, the tiling scheme will be deduced from the projection.
        /// </summary>
        public bool specifyTilingScheme
        {
            get => this._specifyTilingScheme;
            set
            {
                this._specifyTilingScheme = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Min(1)]
        private int _rootTilesX = 1;

        /// <summary>
        /// The number of tiles corresponding to TileCol, also known as TileMatrixWidth. If specified,
        /// this determines the number of tiles at the root of the quadtree tiling scheme in the X 
        /// direction.
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public int rootTilesX
        {
            get => this._rootTilesX;
            set
            {
                this._rootTilesX = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Min(1)]
        private int _rootTilesY = 1;

        /// <summary>
        /// The number of tiles corresponding to TileRow, also known as TileMatrixHeight. If specified,
        /// this determines the number of tiles at the root of the quadtree tiling scheme in the Y 
        /// direction.
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public int rootTilesY
        {
            get => this._rootTilesY;
            set
            {
                this._rootTilesY = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleWest = -180;

        /// <summary>
        /// The west boundary of the bounding rectangle used for the quadtree tiling scheme. 
        /// Specified in longitude degrees in the range [-180, 180].
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public double rectangleWest
        {
            get => this._rectangleWest;
            set
            {
                this._rectangleWest = Math.Clamp(value, -180, 180);
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleSouth = -90;

        /// <summary>
        /// The south boundary of the bounding rectangle used for the quadtree tiling scheme. 
        /// Specified in latitude degrees in the range [-90, 90]. 
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public double rectangleSouth
        {
            get => this._rectangleSouth;
            set
            {
                this._rectangleSouth = Math.Clamp(value, -90, 90);
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleEast = 180;

        /// <summary>
        /// The east boundary of the bounding rectangle used for the quadtree tiling scheme. 
        /// Specified in longitude degrees in the range [-180, 180].
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public double rectangleEast
        {
            get => this._rectangleEast;
            set
            {
                this._rectangleEast = Math.Clamp(value, -180, 180);
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleNorth = 90;

        /// <summary>
        /// The north boundary of the bounding rectangle used for the quadtree tiling scheme. 
        /// Specified in latitude degrees in the range [-90, 90].
        /// Only applicable if <see cref="specifyTilingScheme"/> is set to true.
        /// </summary>
        public double rectangleNorth
        {
            get => this._rectangleNorth;
            set
            {
                this._rectangleNorth = Math.Clamp(value, -90, 90);
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _specifyZoomLevels = false;

        /// <summary>
        /// Set this to true to directly specify the minimum and maximum zoom levels available
        /// from the server. If false, the minimum and maximum zoom levels will be retrieved
        /// from the server's tilemapresource.xml file.
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
        private int _minimumLevel = 0;

        /// <summary>
        /// The minimum level-of-detail supported by the imagery provider.
        /// Only applicable if <see cref="specifyZoomLevels"/> is set to true.
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
        private int _maximumLevel = 25;

        /// <summary>
        /// The maximum level-of-detail supported by the imagery provider.
        /// Only applicable if <see cref="specifyZoomLevels"/> is set to true.
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

        [SerializeField]
        private int _tileWidth = 256;

        /// <summary>
        /// The pixel width of the image tiles.
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
        /// The pixel height of the image tiles.
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

        #endregion

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}