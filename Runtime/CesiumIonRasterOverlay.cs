using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A <see cref="CesiumRasterOverlay"/> that uses an IMAGERY asset from Cesium ion.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonRasterOverlayImpl", "CesiumIonRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Ion Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumIonRasterOverlay : CesiumRasterOverlay
    {
        [SerializeField]
        private long _ionAssetID = 0;

        /// <summary>
        /// The ID of the Cesium ion asset to use.
        /// </summary>
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

        /// <summary>
        /// The access token to use to access the Cesium ion resource.
        /// </summary>
        public string ionAccessToken
        {
            get => this._ionAccessToken;
            set
            {
                this._ionAccessToken = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);
    }
}
