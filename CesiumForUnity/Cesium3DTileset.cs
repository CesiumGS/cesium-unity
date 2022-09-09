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
        [Header("Level of Detail")]
        [Tooltip("The maximum number of pixels of error when rendering this tileset.")]
        [InspectorName("Maximum Screen Space Error")]
        private float _maximumScreenSpaceError = 16.0f;

        public float maximumScreenSpaceError
        {
            get => this._maximumScreenSpaceError;
            set
            {
                this._maximumScreenSpaceError = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Header("Tile Loading")]
        [Tooltip("Whether to preload ancestor tiles.")]
        [InspectorName("Preload Ancestors")]
        private bool _preloadAncestors = true;

        public bool preloadAncestors
        {
            get => this._preloadAncestors;
            set
            {
                this._preloadAncestors = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("Whether to preload sibling tiles.")]
        [InspectorName("Preload Siblings")]
        private bool _preloadSiblings = true;

        public bool preloadSiblings
        {
            get => this._preloadSiblings;
            set
            {
                this._preloadSiblings = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("Whether to unrefine back to a parent tile when a child isn't done loading.")]
        [InspectorName("Forbid Holes")]
        private bool _forbidHoles = false;

        public bool forbidHoles
        {
            get => this._forbidHoles;
            set
            {
                this._forbidHoles = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of tiles that may be loaded at once.")]
        [InspectorName("Maximum Simultaneous Tile Loads")]
        private uint _maximumSimultaneousTileLoads = 20;

        public uint maximumSimultaneousTileLoads
        {
            get => this._maximumSimultaneousTileLoads;
            set
            {
                this._maximumSimultaneousTileLoads = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of bytes that may be cached.")]
        [InspectorName("Maximum Cached Bytes")]
        private long _maximumCachedBytes = 512 * 1024 * 1024;

        public long maximumCachedBytes
        {
            get => this._maximumCachedBytes;
            set
            {
                this._maximumCachedBytes = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("The number of loading descendents a tile should allow before deciding to render itself instead of waiting.")]
        [InspectorName("Loading Descendant Limit")]
        private uint _loadingDescendantLimit = 20;

        public uint loadingDescendantLimit
        {
            get => this._loadingDescendantLimit;
            set
            {
                this._loadingDescendantLimit = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("Whether a specified screen-space error should be enforced for tiles that are outside the frustum or hidden in fog.")]
        [InspectorName("Enforce Culled Screen Space Error")]
        private bool _enforceCulledScreenSpaceError = true;

        public bool enforceCulledScreenSpaceError
        {
            get => this._enforceCulledScreenSpaceError;
            set
            {
                this._enforceCulledScreenSpaceError = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        [Tooltip("Whether a specified screen-space error should be enforced for tiles that are outside the frustum or hidden in fog.")]
        [InspectorName("Enforce Culled Screen Space Error")]
        private float _culledScreenSpaceError = 64.0f;

        public float culledScreenSpaceError
        {
            get => this._culledScreenSpaceError;
            set
            {
                this._culledScreenSpaceError = value;
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
