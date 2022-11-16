using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumWebMapServiceRasterOverlayImpl",
        "CesiumWebMapServiceRasterOverlayImpl.h")]
    public partial class CesiumWebMapServiceRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _baseUrl = "";

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

        public int maximumLevel
        {
            get => this._maximumLevel;
            set
            {
                this._maximumLevel = value;
                this.Refresh();
            }
        }

        protected override partial void AddToTileset(Cesium3DTileset tileset);
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}