using Reinterop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The possible values of <see cref="CesiumGoogleMapTilesRasterOverlay.mapType"/>.
    /// </summary>
    public enum GoogleMapTilesMapType
    {
        /// <summary>
        /// Satellite imagery.
        /// </summary>
        Satellite,

        /// <summary>
        /// The standard Google Maps painted map tiles.
        /// </summary>
        Roadmap,

        /// <summary>
        /// Terrain imagery.
        /// </summary>
        /// <remarks>
        /// When selecting terrain as the map type, you must also add
        /// <see cref="GoogleMapTilesLayerType.Roadmap"/> to
        /// <see cref="CesiumGoogleMapTilesRasterOverlay.layerTypes"/>.
        /// </remarks>
        Terrain
    };

    /// <summary>
    /// The possible values of <see cref="CesiumGoogleMapTilesRasterOverlay.scale"/>.
    /// </summary>
    public enum GoogleMapTilesScale
    {
        /// <summary>
        /// The default.
        /// </summary>
        ScaleFactor1x,

        /// <summary>
        /// Doubles label size and removes minor feature labels.
        /// </summary>
        ScaleFactor2x,

        /// <summary>
        /// Quadruples label size and removes minor feature labels.
        /// </summary>
        ScaleFactor4x,
    }

    ///<summary>
    ///The possible values of <see cref="CesiumGoogleMapTilesRasterOverlay.layerTypes"/>.
    ///</summary>
    public enum GoogleMapTilesLayerType
    {
        /// <summary>
        /// Required if you specify <see cref="GoogleMapTilesMapType.Terrain"/>
        /// as the map type. Can also be optionally overlaid on the satellite map type.
        /// Has no effect on roadmap tiles.
        /// </summary>
        Roadmap,

        /// <summary>
        /// Shows Street View-enabled streets and locations using blue outlines
        /// on the map.
        /// </summary>
        Streetview,

        /// <summary>
        /// Displays current traffic conditions.
        /// </summary>
        Traffic
    };

    /// <summary>
    /// A raster overlay that directly accesses Google Map Tiles (2D).
    /// If you're using Google Map Tiles via Cesium ion, use <see cref="CesiumIonRasterOverlay"/> instead.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumGoogleMapTilesRasterOverlayImpl",
        "CesiumGoogleMapTilesRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Google Map Tiles Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumGoogleMapTilesRasterOverlay : CesiumRasterOverlay
    {

        [SerializeField]
        private string _apiKey = "";

        /// <summary>
        /// The Google Map Tiles API key to use.
        /// </summary>
        public string apiKey
        {
            get => this._apiKey;
            set
            {
                this._apiKey = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private GoogleMapTilesMapType _mapType = GoogleMapTilesMapType.Satellite;

        /// <summary>
        /// The type of base map.
        /// </summary>
        public GoogleMapTilesMapType mapType
        {
            get => this._mapType;
            set
            {
                this._mapType = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private string _language = "en-US";

        /// <summary>
        /// An IETF language tag that specifies the language used to display
        /// information on the tiles. For example, <c>en-US</c> specifies the English
        /// language as spoken in the United States.
        /// </summary>
        public string language
        {
            get => this._language;
            set
            {
                this._language = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private string _region = "US";

        /// <summary>
        /// A Common Locale Data Repository region identifier (two uppercase letters)
        /// that represents the physical location of the user. For example, <c>US</c>.
        /// </summary>
        public string region
        {
            get => this._region;
            set
            {
                this._region = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private GoogleMapTilesScale _scale = GoogleMapTilesScale.ScaleFactor1x;

        /// <summary>
        /// Scales-up the size of map elements (such as road labels), while
        /// retaining the tile size and coverage area of the default tile.
        /// </summary>
        /// <remarks>
        /// Increasing the scale also reduces the number of labels on the map, which
        /// reduces clutter.
        /// </remarks>
        public GoogleMapTilesScale scale
        {
            get => this._scale;
            set
            {
                this._scale = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _highDpi;

        /// <summary>
        /// Specifies whether to return high-resolution tiles.
        /// </summary>
        /// <remarks>
        /// <para>If the scale-factor is increased, <c>highDpi</c> is used to increase
        /// the size of the tile. Normally, increasing the scale factor enlarges the
        /// resulting tile into an image of the same size, which lowers quality. With
        /// <c>highDpi</c>, the resulting size is also increased, preserving quality.
        /// DPI stands for Dots per Inch, and High DPI means the tile renders using
        /// more dots per inch than normal.
        /// </para>
        /// <para>
        /// If <c>true</c>, then the number of pixels in each of the x and y
        /// dimensions is multiplied by the scale factor (that is, 2x or 4x). The
        /// coverage area of the tile remains unchanged. This parameter works only
        /// with <see cref="scale"/> values of <c>2x</c> or <c>4x</c>. It has no 
        /// effect on <c>1x</c> scale tiles.
        /// </para>
        /// </remarks>
        public bool highDpi
        {
            get => this._highDpi; set
            {
                this._highDpi = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private List<GoogleMapTilesLayerType> _layerTypes;

        /// <summary>
        /// The layer types to be added to the map.
        /// </summary>
        public List<GoogleMapTilesLayerType> layerTypes
        {
            get => this._layerTypes;
            set
            {
                this._layerTypes = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private List<string> _styles;

        /// <summary>
        /// An array of JSON style objects that specify the appearance and
        /// detail level of map features such as roads, parks, and built-up areas.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Styling is used to customize the standard Google base map. The <c>styles</c>
        /// parameter is valid only if the <see cref="mapType"/> is
        /// <see cref="GoogleMapTilesMapType.Roadmap"/>.
        /// </para>
        /// <para>
        /// For the complete style syntax, see the
        /// <see href="https://developers.google.com/maps/documentation/tile/style-reference">
        /// Style Reference.</see>
        /// </para>
        /// </remarks>
        public List<string> styles
        {
            get => this._styles;
            set
            {
                this._styles = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private bool _overlay = false;

        /// <summary>
        /// Specifies whether <see cref="layerTypes"/> are rendered as a separate overlay,
        /// or combined with the base imagery.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, the base map isn't displayed. If you haven't defined any
        /// <c>layerTypes</c>, then this value is ignored.
        /// </remarks>
        public bool overlay
        {
            get => this._overlay;
            set
            {
                this._overlay = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    };
}