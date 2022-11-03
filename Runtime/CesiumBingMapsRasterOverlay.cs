using Reinterop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CesiumForUnity
{
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

    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumBingMapsRasterOverlayImpl",
        "CesiumBingMapsRasterOverlayImpl.h")]
    public partial class CesiumBingMapsRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _bingMapsKey = "";

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

        public BingMapsStyle mapStyle
        {
            get => this._mapStyle;
            set
            {
                this._mapStyle = value;
                this.Refresh();
            }
        }

        protected override partial void AddToTileset(Cesium3DTileset tileset);
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}