using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonRasterOverlayImpl", "CesiumIonRasterOverlayImpl.h")]
    public partial class CesiumIonRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private long _ionAssetID = 0;

        public long ionAssetID
        {
            get => this._ionAssetID;
            set
            {
                this._ionAssetID = value;
                this.Refresh();
            }
        }

        [SerializeField]
        private string _ionAccessToken = "";

        public string ionAccessToken
        {
            get => this._ionAccessToken;
            set
            {
                this._ionAccessToken = value;
                this.Refresh();
            }
        }

        protected override partial void AddToTileset(Cesium3DTileset tileset);

        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
