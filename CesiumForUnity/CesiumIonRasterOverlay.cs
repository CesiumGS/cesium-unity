using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonRasterOverlayImpl", "CesiumIonRasterOverlayImpl.h")]
    public partial class CesiumIonRasterOverlay : MonoBehaviour
    {
        [SerializeField]
        [Header("Source")]
        [Tooltip("The ID of the Cesium ion asset to use.")]
        [InspectorName("ion Asset ID")]
        private ulong _ionAssetID = 0;

        public ulong ionAssetID
        {
            get => this._ionAssetID;
            set
            {
                this._ionAssetID = value;
                this.RecreateRasterOverlay();
            }
        }

        [SerializeField]
        [Tooltip("The access token to use to access the Cesium ion resource.")]
        [InspectorName("ion Access Token")]
        private string _ionAccessToken = "";

        public string ionAccessToken
        {
            get => this._ionAccessToken;
            set
            {
                this._ionAccessToken = value;
                this.RecreateRasterOverlay();
            }
        }

        private partial void RecreateRasterOverlay();
    }
}
