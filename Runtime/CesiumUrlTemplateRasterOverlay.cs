using Reinterop;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A raster overlay that loads tiles from a templated URL.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumUrlTemplateRasterOverlayImpl", "CesiumUrlTemplateRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium URL Template Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumUrlTemplateRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _templateUrl;

        /// <summary>
        /// The URL containing template parameters that will be substituted when
        /// loading tiles.
        /// 
        /// The following template parameters are supported in `url`:
        /// - `{x}` - The tile X coordinate in the tiling scheme, where 0 is the
        /// westernmost tile.
        /// - `{y}` - The tile Y coordinate in the tiling scheme, where 0 is the
        /// nothernmost tile.
        /// - `{z}` - The level of the tile in the tiling scheme, where 0 is the root
        /// of the quadtree pyramid.
        /// - `{reverseX}` - The tile X coordinate in the tiling scheme, where 0 is the
        /// easternmost tile.
        /// - `{reverseY}` - The tile Y coordinate in the tiling scheme, where 0 is the
        /// southernmost tile.
        /// - `{reverseZ}` - The tile Z coordinate in the tiling scheme, where 0 is
        /// equivalent to `urlTemplateOptions.maximumLevel`.
        /// - `{westDegrees}` - The western edge of the tile in geodetic degrees.
        /// - `{southDegrees}` - The southern edge of the tile in geodetic degrees.
        /// - `{eastDegrees}` - The eastern edge of the tile in geodetic degrees.
        /// - `{northDegrees}` - The northern edge of the tile in geodetic degrees.
        /// - `{minimumX}` - The minimum X coordinate of the tile's projected
        /// coordinates.
        /// - `{minimumY}` - The minimum Y coordinate of the tile's projected
        /// coordinates.
        /// - `{maximumX}` - The maximum X coordinate of the tile's projected
        /// coordinates.
        /// - `{maximumY}` - The maximum Y coordinate of the tile's projected
        /// coordinates.
        /// - `{width}` - The width of each tile in pixels.
        /// - `{height}` - The height of each tile in pixels.
        /// </summary>
        public string templateUrl
        {
            get => this._templateUrl;
            set
            {
                this._templateUrl = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private CesiumUrlTemplateRasterOverlayProjection _projection = CesiumUrlTemplateRasterOverlayProjection.WebMercator;

        /// <summary>
        /// The type of projection used to protect the imagery onto the globe.
        /// 
        /// For instance, EPSG:4326 uses geographic projection and EPSG:3857 uses Web Mercator.
        /// </summary>
        public CesiumUrlTemplateRasterOverlayProjection projection
        {
            get => this._projection;
            set
            {
                this._projection = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _specifyTilingScheme = false;

        /// <summary>
        /// Set this to true to specify the quadtree tiling scheme according to the specified root 
        /// tile numbers and projected bounding rectangle. If false, the tiling scheme will be 
        /// deduced from the projection.
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
        /// If specified, this determines the number of tiles at the root of the quadtree tiling scheme in the X direction.
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
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
        /// If specified, this determines the number of tiles at the root of the quadtree tiling scheme in the Y direction.
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
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
        private double _rectangleWest = -180.0;

        /// <summary>
        /// The west boundary of the bounding rectangle used for the quadtree tiling
        /// scheme. Specified in longitude degrees in the range [-180, 180].
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
        /// </summary>
        public double rectangleWest
        {
            get => this._rectangleWest;
            set
            {
                this._rectangleWest = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleSouth = -90.0;

        /// <summary>
        /// The south boundary of the bounding rectangle used for the quadtree tiling
        /// scheme. Specified in latitude degrees in the range [-90, 90].
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
        /// </summary>
        public double rectangleSouth
        {
            get => this._rectangleSouth;
            set
            {
                this._rectangleSouth = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleEast = 180.0;

        /// <summary>
        /// The east boundary of the bounding rectangle used for the quadtree tiling
        /// scheme. Specified in longitude degrees in the range [-180, 180].
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
        /// </summary>
        public double rectangleEast
        {
            get => this._rectangleEast;
            set
            {
                this._rectangleEast = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private double _rectangleNorth = -90.0;

        /// <summary>
        /// The north boundary of the bounding rectangle used for the quadtree tiling
        /// scheme. Specified in latitude degrees in the range [-90, 90].
        ///
        /// Only applicable if "Specify Tiling Scheme" is set to true.
        /// </summary>
        public double rectangleNorth
        {
            get => this._rectangleNorth;
            set
            {
                this._rectangleNorth = value;
                this.Refresh();
            }
        }

        [SerializeField]
        [Min(0)]
        private int _minimumLevel = 0;

        /// <summary>
        /// Minimum zoom level.
        ///
        /// Take care when specifying this that the number of tiles at the minimum
        /// level is small, such as four or less. A larger number is likely to result
        /// in rendering problems.
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
        private int _maximumLevel = 25;

        /// <summary>
        /// Maximum zoom level.
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
        [Min(64)]
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
        [Min(64)]
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

        [SerializeField]
        private List<HeaderEntry> _requestHeaders = new List<HeaderEntry>();

        /// <summary>
        /// HTTP headers to be attached to each request made for this raster overlay.
        /// </summary>
        public List<HeaderEntry> requestHeaders
        {
            get => this._requestHeaders;
            set
            {
                this._requestHeaders = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);

        /// <summary>
        /// An HTTP header name paired with its specified value.
        /// </summary>
        [Serializable]
        public class HeaderEntry
        {
            /// <summary>
            /// The name of the header.
            /// </summary>
            public string Name;
            /// <summary>
            /// The value of the header.
            /// </summary>
            public string Value;
        }

        /// <summary>
        /// Specifies the type of projection used for projecting a URL template raster overlay.
        /// </summary>
        public enum CesiumUrlTemplateRasterOverlayProjection
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
    }
}
