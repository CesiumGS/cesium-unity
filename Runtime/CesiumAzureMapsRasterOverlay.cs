using Reinterop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// The available styles of the <see cref="CesiumAzureMapsRasterOverlay"/>.
    /// </summary>
    public enum AzureMapsStyle
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
        private string _azureMapsKey = "";

        /// <summary>
        /// The Azure Maps API key to use.
        /// </summary>
        public string azureMapsKey
        {
            get => this._azureMapsKey;
            set
            {
                this._azureMapsKey = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private AzureMapsStyle _mapStyle = AzureMapsStyle.Aerial;

        /// <summary>
        /// The map style to use.
        /// </summary>
        public AzureMapsStyle mapStyle
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