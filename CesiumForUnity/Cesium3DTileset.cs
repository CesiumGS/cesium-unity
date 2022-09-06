using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{

    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::Cesium3DTilesetImpl", "Cesium3DTilesetImpl.h")]
    public partial class Cesium3DTileset : MonoBehaviour
    {
        [SerializeField]
        [Header("Source")]
        [Tooltip("The URL of this tileset's \"tileset.json\" file.")]
        [InspectorName("URL")]
        private string _url = "";

        public string url
        {
            get => this._url;
            set
            {
                this._url = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("The ID of the Cesium ion asset to use.")]
        [InspectorName("ion Asset ID")]
        private long _ionAssetID = 0;

        public long ionAssetID
        {
            get => this._ionAssetID;
            set
            {
                this._ionAssetID = value;
                this.RecreateTileset();
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
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Header("Render")]
        [Tooltip("The Material to use to render opaque parts of tiles.")]
        [InspectorName("Opaque Material")]
        private Material? _opaqueMaterial = null;

        public Material? opaqueMaterial
        {
            get => this._opaqueMaterial;
            set
            {
                this._opaqueMaterial = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Header("Debug")]
        [Tooltip("Whether to log details about the tile selection process.")]
        [InspectorName("Log Selection Stats")]
        private bool _logSelectionStats = false;

        public bool logSelectionStats
        {
            get => this._logSelectionStats;
            set { this._logSelectionStats = value; }
        }

        private partial void Start();
        private partial void Update();
        private partial void OnValidate();

        private partial void OnEnable();
        private partial void OnDisable();

        private partial void RecreateTileset();
    }
}
