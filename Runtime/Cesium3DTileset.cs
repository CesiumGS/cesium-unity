using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    public enum CesiumDataSource
    {
        FromCesiumIon,
        FromUrl
    }

    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::Cesium3DTilesetImpl", "Cesium3DTilesetImpl.h")]
    public partial class Cesium3DTileset : MonoBehaviour
    {
        public delegate void TilesetLoadFailureDelegate(
            Cesium3DTilesetLoadFailureDetails details);
        public static event TilesetLoadFailureDelegate? OnCesium3DTilesetLoadFailure;

        public static void
            BroadcastCesium3DTilesetLoadFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (OnCesium3DTilesetLoadFailure != null)
            {
                OnCesium3DTilesetLoadFailure(details);
            }
        }

        [SerializeField]
        private bool _showCreditsOnScreen = false;
        
        public bool showCreditsOnScreen
        {
            get => this._showCreditsOnScreen;
            set
            {
                this._showCreditsOnScreen = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private CesiumDataSource _tilesetSource = CesiumDataSource.FromCesiumIon;

        public CesiumDataSource tilesetSource
        {
            get => this._tilesetSource;
            set
            {
                this._tilesetSource = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
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
        private bool _enableFrustumCulling = true;

        public bool enableFrustumCulling
        {
            get => this._enableFrustumCulling;
            set
            {
                this._enableFrustumCulling = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private bool _enableFogCulling = true;

        public bool enableFogCulling
        {
            get => this._enableFogCulling;
            set
            {
                this._enableFogCulling = value;
                this.RecreateTileset();
            }
        }


        [SerializeField]
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
        private bool _useLodTransitions = false;

        public bool useLodTransitions
        {
            get => this._useLodTransitions;
            set
            {
                this._useLodTransitions = value;
                this.RecreateTileset();
            }
        }


        [SerializeField]
        private float _lodTransitionLength = 0.5f;

        public float lodTransitionLength
        {
            get => this._lodTransitionLength;
            set
            {
                this._lodTransitionLength = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private bool _generateSmoothNormals = false;

        public bool generateSmoothNormals
        {
            get => this._generateSmoothNormals;
            set
            {
                this._generateSmoothNormals = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private bool _suspendUpdate = false;

        public bool suspendUpdate
        {
            get => this._suspendUpdate;
            set
            {
                this._suspendUpdate = value;
            }
        }

        // Normally tilesets are destroyed when anything in the editor changes.
        // But if suspendUpdate is the only value that has changed, the tileset
        // should not be reloaded, and instead continue updating after the setting
        // has been toggled. This variable saves the last value of suspendUpdate,
        // so OnValidate() can determine if this property was modified. If so, it
        // prevents the tileset from being destroyed.
        private bool _previousSuspendUpdate = false;

        public bool previousSuspendUpdate
        {
            get => this._previousSuspendUpdate;
            set
            {
                this._previousSuspendUpdate = value;
            }
        }

        [SerializeField]
        [Header("Debug")]
        [Tooltip("Whether to show tiles as individual components in the hierarchy window.")]
        [InspectorName("ShowTilesInHierarchy")]
        private bool _showTilesInHierarchy = false;

        public bool showTilesInHierarchy
        {
            get => this._showTilesInHierarchy;
            set
            {
                this._showTilesInHierarchy = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private bool _updateInEditor = true;

        public bool updateInEditor
        {
            get => this._updateInEditor;
            set
            {
                this._updateInEditor = value;
                this.RecreateTileset();
            }
        }

        [SerializeField]
        private bool _logSelectionStats = false;

        public bool logSelectionStats
        {
            get => this._logSelectionStats;
            set { this._logSelectionStats = value; }
        }

        [SerializeField]
        private bool _createPhysicsMeshes = true;

        public bool createPhysicsMeshes
        {
            get => this._createPhysicsMeshes;
            set
            {
                this._createPhysicsMeshes = value;
                this.RecreateTileset();
            }
        }

        private partial void Start();
        private partial void Update();
        private partial void OnValidate();

        private partial void OnEnable();
        private partial void OnDisable();
        public partial void RecreateTileset();
    }
}
