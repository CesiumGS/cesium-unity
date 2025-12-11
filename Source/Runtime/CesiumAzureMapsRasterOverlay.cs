using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The available styles of the <see cref="CesiumAzureMapsRasterOverlay"/>.
    /// </summary>
    public enum AzureMapsTilesetId
    {
        /// <summary>
        /// All roadmap layers with Azure Maps' main style.
        /// </summary>
        BaseRoad,
        /// <summary>
        /// All roadmap layers with Azure Maps' dark grey style.
        /// </summary>
        BaseDarkGrey,
        /// <summary>
        /// Label data in Azure Maps' main style.
        /// </summary>
        BaseLabelsRoad, 
        /// <summary>
        /// Label data in Azure Maps' dark grey style.
        /// </summary>
        BaseLabelsDarkGrey,
        /// <summary>
        /// Road, boundary, and label data in Azure Maps' main style.
        /// </summary>
        BaseHybridRoad, 
        /// <summary>
        /// Road, boundary, and label data in Azure Maps' dark grey style.
        /// </summary>
        BaseHybridDarkGrey, 
        /// <summary>
        /// A combination of satellite or aerial imagery. Only available for accounts
        /// under S1 and G2 pricing SKU.
        /// </summary>
        Imagery,
        /// <summary>
        /// Shaded relief and terra layers.
        /// </summary>
        Terra,
        /// <summary>
        /// Weather radar tiles. Latest weather radar images including areas of rain,
        /// snow, ice and mixed conditions.
        /// </summary>
        WeatherRadar,
        /// <summary>
        /// Weather infrared tiles. Latest infrared satellite images showing clouds by
        /// their temperature.
        /// </summary>
        WeatherInfrared,
        /// <summary>
        /// Absolute traffic tiles in Azure Maps' main style.
        /// </summary>
        TrafficAbsolute,
        /// <summary>
        /// Relative traffic tiles in Azure Maps' main style. This filters out traffic
        /// data from smaller streets that are otherwise included in TrafficAbsolute.
        /// </summary>
        TrafficRelativeMain,
        /// <summary>
        /// Relative traffic tiles in Azure Maps' dark style. This filters out traffic
        /// data from smaller streets that are otherwise included in TrafficAbsolute.
        /// </summary>
        TrafficRelativeDark,
        /// <summary>
        /// Delay traffic tiles in Azure Maps' dark style. This only shows the points
        /// of delay along traffic routes that are otherwise included in
        /// TrafficAbsolute.
        /// </summary>
        TrafficDelay,
        /// <summary>
        /// Reduced traffic tiles in Azure Maps' dark style. This shows the traffic
        /// routes and major delay points, but filters out some data that is otherwise
        /// included in TrafficAbsolute.
        /// </summary>
        TrafficReduced,
    }

    /// <summary>
    /// A raster overlay that directly accesses Azure Maps. If you're using Azure Maps
    /// via Cesium ion, use the <see cref="CesiumIonRasterOverlay"/> component instead.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumAzureMapsRasterOverlayImpl",
        "CesiumAzureMapsRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Azure Maps Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumAzureMapsRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _key = "";

        /// <summary>
        /// The Azure Maps subscription key to use.
        /// </summary>
        public string key
        {
            get => this._key;
            set
            {
                this._key = value;
                this.Refresh();
            }
        }

        [SerializeField] private string _apiVersion = "2024-04-01";
        /// <summary>
        /// The version number of Azure Maps API.
        /// </summary>
        public string apiVersion
        {
            get => this._apiVersion;
            set
            {
                this._apiVersion = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private AzureMapsTilesetId _tilesetId = AzureMapsTilesetId.BaseRoad;

        /// <summary>
        /// The tileset ID to use.
        /// </summary>
        public AzureMapsTilesetId tilesetId
        {
            get => this._tilesetId;
            set
            {
                this._tilesetId = value;
                this.Refresh();
            }
        }

        [SerializeField] 
        private string _language = "en-us";

        /// <summary>
        /// The language in which search results should be returned. This should be one
        /// of the supported IETF language tags, case-insensitive. When data in the
        /// specified language is not available for a specific field, default language
        /// is used.
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
        private string _view = "US";

        /// <summary>
        /// The View parameter (also called the "user region" parameter) allows
        /// you to show the correct maps for a certain country/region for
        /// geopolitically disputed regions.
        /// </summary>
        /// <remarks>
        /// Different countries/regions have different views of such regions, and the
        /// View parameter allows your application to comply with the view required by
        /// the country/region your application will be serving. By default, the View
        /// parameter is set to "Unified" even if you haven't defined it in the
        /// request. It is your responsibility to determine the location of your users,
        /// and then set the View parameter correctly for that location. Alternatively,
        /// you have the option to set 'View=Auto', which will return the map data
        /// based on the IP address of the request. The View parameter in Azure Maps
        /// must be used in compliance with applicable laws, including those regarding
        /// mapping, of the country/region where maps, images and other data and third
        /// party content that you are authorized to access via Azure Maps is made
        /// available. Example: view=IN.
        /// </remarks>
        public string view
        {
            get => this._view;
            set
            {
                this._view = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}