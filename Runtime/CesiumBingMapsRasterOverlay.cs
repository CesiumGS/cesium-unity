using Reinterop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The available styles of the <see cref="CesiumBingMapsRasterOverlay"/>.
    /// </summary>
    public enum BingMapsStyle
    {
        Aerial,
        AerialWithLabelsOnDemand,
        RoadOnDemand,
        CanvasDark,
        CanvasLight,
        CanvasGray,
        OrdnanceSurvey,
        CollinsBart
    }

    /// <summary>
    /// A raster overlay that directly accesses Bing Maps. If you're using Bing Maps
    /// via Cesium ion, use the <see cref="CesiumIonRasterOverlay"/> component instead.
    /// </summary>
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumBingMapsRasterOverlayImpl",
        "CesiumBingMapsRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Bing Maps Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumBingMapsRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _bingMapsKey = "";

        /// <summary>
        /// The Bing Maps API key to use.
        /// </summary>
        public string bingMapsKey
        {
            get => this._bingMapsKey;
            set
            {
                this._bingMapsKey = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private BingMapsStyle _mapStyle = BingMapsStyle.Aerial;

        /// <summary>
        /// The map style to use.
        /// </summary>
        public BingMapsStyle mapStyle
        {
            get => this._mapStyle;
            set
            {
                this._mapStyle = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);

        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}