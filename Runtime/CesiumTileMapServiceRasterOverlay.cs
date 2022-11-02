using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation(
        "CesiumForUnityNative::CesiumTileMapServiceRasterOverlayImpl",
        "CesiumTileMapServiceRasterOverlayImpl.h")]
    public partial class CesiumTileMapServiceRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private string _url = "";

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