using Reinterop;
using System.Collections;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A <see cref="CesiumRasterOverlay"/> that uses an IMAGERY asset from Cesium ion.
    /// </summary>
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonRasterOverlayImpl", "CesiumIonRasterOverlayImpl.h")]
    [AddComponentMenu("Cesium/Cesium Ion Raster Overlay")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]
    public partial class CesiumIonRasterOverlay : CesiumRasterOverlay, ISerializationCallbackReceiver
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

        [SerializeField]
        private CesiumIonServer _ionServer = null;

        /// <summary>
        /// The Cesium ion server from which this raster overlay is loaded.
        /// </summary>
        public CesiumIonServer ionServer
        {
            get
            {
                if (this._ionServer == null)
                {
#if UNITY_EDITOR
                    // See OnAfterDeserialize.
                    if (this._useDefaultServer)
                        this._ionServer = CesiumIonServer.defaultServer;
                    else
                        this._ionServer = CesiumIonServer.serverForNewObjects;

                    this._useDefaultServer = false;
#else
                    this._ionServer = CesiumIonServer.serverForNewObjects;
#endif
                }
                return this._ionServer;
            }
            set
            {
                if (value == null) value = CesiumIonServer.serverForNewObjects;
                this._ionServer = value;
                this.Refresh();
            }
        }

        /// <inheritdoc/>
        protected override partial void AddToTileset(Cesium3DTileset tileset);
        /// <inheritdoc/>
        protected override partial void RemoveFromTileset(Cesium3DTileset tileset);

        internal IEnumerator AddToTilesetLater(Cesium3DTileset tileset)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            this.AddToTileset(tileset);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if UNITY_EDITOR
            // For backward compatibility, overlays loaded without a server should adopt
            // the default one rather than the current one.
            if (this._ionServer == null)
                this._useDefaultServer = true;
#endif
        }

#if UNITY_EDITOR
        private bool _useDefaultServer = false;
#endif
    }
}
