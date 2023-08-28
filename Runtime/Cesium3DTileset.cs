using Reinterop;
using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// Specifies the source of a Cesium dataset.
    /// </summary>
    public enum CesiumDataSource
    {
        /// <summary>
        /// The dataset is from Cesium ion.
        /// </summary>
        FromCesiumIon,

        /// <summary>
        /// The dataset is from a regular web URL.
        /// </summary>
        FromUrl
    }

    /// <summary>
    /// A tileset in the 3D Tiles format. <see href="https://github.com/CesiumGS/3d-tiles">3D Tiles</see>
    /// is an open specification for sharing, visualizing, fusing, and interacting with massive
    /// heterogenous 3D geospatial content across desktop, web, and mobile applications. The tileset is
    /// streamed incrementally into Unity based on the current camera view(s).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A GameObject with this component must be nested inside one with a <see cref="CesiumGeoreference"/>
    /// component. The georeference controls how this tileset is mapped into the Unity world.
    /// </para>
    /// <para>
    /// In most cases, the Transform of the GameObject that contains this component, and its ancestors, should
    /// be an identity transform: 0 position, 0 rotation, 1 scale. Otherwise, this tileset will be misaligned
    /// with other globe tilesets. However, it is sometimes useful to purposely offset a tileset.
    /// </para>
    /// </remarks>
    [ExecuteInEditMode]
    [ReinteropNativeImplementation("CesiumForUnityNative::Cesium3DTilesetImpl", "Cesium3DTilesetImpl.h")]
    [AddComponentMenu("Cesium/Cesium 3D Tileset")]
    [IconAttribute("Packages/com.cesium.unity/Editor/Resources/Cesium-24x24.png")]  
    public partial class Cesium3DTileset : MonoBehaviour, IDisposable
    {
        public void Dispose()
        {
            this.OnDisable();
            this.DisposeImplementation();
        }

        /// <summary>
        /// Encapsulates a method that receives details of a tileset load failure.
        /// </summary>
        /// <param name="details">The details of the load failure.</param>
        public delegate void TilesetLoadFailureDelegate(
            Cesium3DTilesetLoadFailureDetails details);

        /// <summary>
        /// An event that is raised when the tileset encounters an error that prevents it from loading.
        /// </summary>
        public static event TilesetLoadFailureDelegate OnCesium3DTilesetLoadFailure;

        internal static void
            BroadcastCesium3DTilesetLoadFailure(Cesium3DTilesetLoadFailureDetails details)
        {
            if (OnCesium3DTilesetLoadFailure != null)
            {
                OnCesium3DTilesetLoadFailure(details);
            }
        }
        /// <summary>
        /// Occurs when a new GameObject is instantiated for a Tile in the tileset.
        /// </summary>
        /// <remarks>
        /// This event can be used to customize the Tile GameObjects as they are loaded,
        /// such as adding components, changing materials, or applying transformations.
        /// </remarks>
        public event Action<GameObject> OnTileGameObjectCreated;

        internal void BroadcastNewGameObjectCreated(GameObject go)
        {
            if(OnTileGameObjectCreated != null)
            {
                OnTileGameObjectCreated(go);
            }
        }

        internal static event Action OnSetShowCreditsOnScreen;

        [SerializeField]
        private bool _showCreditsOnScreen = false;

        /// <summary>
        /// Whether or not to force this tileset's credits to be shown on the main screen. If false, the
        /// credits are usually only shown on a "Data Attribution" popup.
        /// </summary>
        public bool showCreditsOnScreen
        {
            get => this._showCreditsOnScreen;
            set
            {
                this._showCreditsOnScreen = value;
                this.SetShowCreditsOnScreen(this._showCreditsOnScreen);
                if (Cesium3DTileset.OnSetShowCreditsOnScreen != null)
                {
                    Cesium3DTileset.OnSetShowCreditsOnScreen();
                }
            }
        }

        [SerializeField]
        private CesiumDataSource _tilesetSource = CesiumDataSource.FromCesiumIon;

        /// <summary>
        /// The source of the data for this tileset: Cesium ion or a regular URL.
        /// </summary>
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

        /// <summary>
        /// The URL from which to load the tileset. This property is used only if
        /// <see cref="tilesetSource"/> is set to "FromUrl".
        /// </summary>
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

        /// <summary>
        /// The Cesium ion asset ID from which to load the tileset. This property is used
        /// only if <see cref="tilesetSource"/> is set to "FromCesiumIon".
        /// </summary>
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

        /// <summary>
        /// The Cesium ion access token to use when loading the tileset from Cesium ion. This property is used
        /// only if <see cref="tilesetSource"/> is set to "FromCesiumIon".
        /// </summary>
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

        /// <summary>
        /// The maximum number of pixels of error when rendering this tileset.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used to select an appropriate level-of-detail: A low value
        /// will cause many tiles with a high level of detail to be loaded, causing
        /// a finer visual representation of the tiles, but with a higher performance
        /// cost for loading and rendering. A higher value will cause a coarser
        /// visual representation, with lower performance requirements.
        /// </para>
        /// <para>
        /// When a tileset uses the older layer.json / quantized-mesh format rather
        /// than 3D Tiles, this value is effectively divided by 8.0. So the default
        /// value of 16.0 corresponds to the standard value for quantized-mesh
        /// terrain of 2.0.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Whether to preload ancestor tiles.
        /// </summary>
        /// <remarks>
        /// Setting this to true optimizes the zoom-out experience and provides more
        /// detail in newly-exposed areas when panning. The down side is that it
        /// requires loading more tiles.
        /// </remarks>
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

        /// <summary>
        /// Whether to preload sibling tiles.
        /// </summary>
        /// <remarks>
        /// Setting this to true causes tiles with the same parent as a rendered
        /// tile to be loaded, even if they are culled. Setting this to true may
        /// provide a better panning experience at the cost of loading more tiles.
        /// </remarks>
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

        /// <summary>
        /// Whether to prevent refinement of a parent tile when a child isn't done loading.
        /// </summary>
        /// <remarks>
        /// When this is set to true, the tileset will guarantee that the tileset will
        /// never be rendered with holes in place of tiles that are not yet loaded,
        /// even though the tile that is rendered instead may have low resolution.
        /// When false, overall loading will be faster, but newly-visible parts of the
        /// tileset may initially be blank.
        /// </remarks>
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

        /// <summary>
        /// The maximum number of tiles that may be loaded simultaneously.
        /// </summary>
        /// <remarks>
        /// When new parts of the tileset become visible, the tasks to load the
        /// corresponding tiles are put into a queue. This value determines how
        /// many of these tasks are processed at the same time. A higher value may
        /// cause the tiles to be loaded and rendered more quickly, at the cost of
        /// a higher network and processing load.
        /// </remarks>
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

        /// <summary>
        /// The maximum number of bytes that may be cached for this tileset.
        /// </summary>
        /// <remarks>
        /// Note that this value, even if 0, will never cause tiles that are needed
        /// for rendering to be unloaded. However, if the total number of loaded
        /// bytes is greater than this value, tiles will be unloaded until the
        /// total is under this number or until only required tiles remain, whichever
        /// comes first.
        /// </remarks>
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

        /// <summary>
        /// The number of loading descendents a tile should allow before deciding to render itself instead of waiting.
        /// </summary>
        /// <remarks>
        /// Setting this to 0 will cause each level of detail to be loaded
        /// successively. This will increase the overall loading time, but cause
        /// additional detail to appear more gradually. Setting this to a high value
        /// like 1000 will decrease the overall time until the desired level of detail
        /// is achieved, but this high-detail representation will appear at once, as
        /// soon as it is loaded completely.
        /// </remarks>
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

        /// <summary>
        /// Whether to cull tiles that are outside the frustum.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this is true, meaning that tiles that are not visible with
        /// the current camera configuration will be ignored. It can be set to false,
        /// so that these tiles are still considered for loading, refinement and rendering.
        /// </para>
        /// <para>
        /// This will cause more tiles to be loaded, but helps to avoid holes and
        /// provides a more consistent mesh, which may be helpful for physics and shadows.
        /// </para>
        /// <para>
        /// Note that frustum calling will be disabled if <see cref="useLodTransitions"/> is set to true.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Whether to cull tiles that are occluded by fog.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This does not refer to the atmospheric fog rendered by Unity, but to an
        /// internal representation of fog: Depending on the height of the camera
        /// above the ground, tiles that are far away (close to the horizon) will be
        /// culled when this flag is enabled.
        /// </para>
        /// <para>
        /// Note that this will always be disabled if <see cref="useLodTransitions"/> is set to true.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Whether a specified screen-space error should be enforced for tiles
        /// that are outside the frustum or hidden in fog.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="enableFrustumCulling"/> and <see cref="enableFogCulling"/> are both true,
        /// tiles outside the view frustum or hidden in fog are effectively ignored,
        /// and so their level-of-detail doesn't matter. And in this scenario, this
        /// property is ignored.
        /// </para>
        /// <para>
        /// However, when either of those flags are false, these "would-be-culled"
        /// tiles continue to be processed, and the question arises of how to handle
        /// their level-of-detail. When this property is false, refinement terminates
        /// at these tiles, no matter what their current screen-space error. The tiles
        /// are available for physics, shadows, etc., but their level-of-detail may be
        /// very low.
        /// </para>
        /// <para>
        /// When set to true, these tiles are refined until they achieve the specified
        /// <see cref="culledScreenSpaceError"/>. This allows control over the minimum quality
        /// of these would-be-culled tiles.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// The screen-space error to be enforced for tiles that are outside the frustum or hidden in fog.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="enableFrustumCulling"/> and <see cref="enableFogCulling"/> are both true,
        /// tiles outside the view frustum or hidden in fog are effectively ignored,
        /// and so their level-of-detail doesn't matter. And in this scenario, this
        /// property is ignored.
        /// </para>
        /// <para>
        /// However, when either of those flags are false, these \"would-be-culled\"
        /// tiles continue to be processed, and the question arises of how to handle
        /// their level-of-detail. When this property is false, refinement terminates
        /// at these tiles, no matter what their current screen-space error. The tiles
        /// are available for physics, shadows, etc., but their level-of-detail may be
        /// very low.
        /// </para>
        /// <para>
        /// When set to true, these tiles are refined until they achieve the specified
        /// "Culled Screen Space Error". This allows control over the minimum quality
        /// of these would-be-culled tiles.
        /// </para>
        /// </remarks>
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
        private Material _opaqueMaterial = null;

        /// <summary>
        /// The Material to use to render opaque parts of tiles.
        /// </summary>
        public Material opaqueMaterial
        {
            get => this._opaqueMaterial;
            set
            {
                this._opaqueMaterial = value;
                this.RecreateTileset();
            }
        }

        //[SerializeField]
        //private bool _useLodTransitions = false;

        //public bool useLodTransitions
        //{
        //    get => this._useLodTransitions;
        //    set
        //    {
        //        this._useLodTransitions = value;
        //        this.RecreateTileset();
        //    }
        //}


        //[SerializeField]
        //private float _lodTransitionLength = 0.5f;

        //public float lodTransitionLength
        //{
        //    get => this._lodTransitionLength;
        //    set
        //    {
        //        this._lodTransitionLength = value;
        //        this.RecreateTileset();
        //    }
        //}

        [SerializeField]
        private bool _generateSmoothNormals = false;

        /// <summary>
        /// Whether to generate smooth normals when normals are missing in the glTF.
        /// </summary>
        /// <remarks>
        /// According to the glTF spec: "When normals are not specified, client
        /// implementations should calculate flat normals." However, calculating flat
        /// normals requires duplicating vertices. This option allows the glTFs to be rendered
        /// with smooth normals instead when the original glTF is missing normals.
        /// </remarks>
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
        private CesiumPointCloudShading _pointCloudShading;

        /// <summary>
        /// The CesiumPointCloudShading attached to this tileset. If the tileset
        /// contains points, their appearance can be configured with the point
        /// cloud shading parameters.
        /// </summary>
        public CesiumPointCloudShading pointCloudShading
        {
            get => this._pointCloudShading;
        }

        [SerializeField]
        private bool _suspendUpdate = false;

        /// <summary>
        /// Pauses level-of-detail and culling updates of this tileset.
        /// </summary>
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

        internal bool previousSuspendUpdate
        {
            get => this._previousSuspendUpdate;
            set
            {
                this._previousSuspendUpdate = value;
            }
        }

        [SerializeField]
        private bool _showTilesInHierarchy = false;

        /// <summary>
        /// Whether to show tiles as individual game objects in the hierarchy window.
        /// </summary>
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

        /// <summary>
        /// If true, this tileset is ticked/updated in the editor. If false, it is only ticked while playing (including Play-in-Editor).
        /// </summary>
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

        /// <summary>
        /// Whether to log details about the tile selection process.
        /// </summary>
        public bool logSelectionStats
        {
            get => this._logSelectionStats;
            set { this._logSelectionStats = value; }
        }

        [SerializeField]
        private bool _createPhysicsMeshes = true;

        /// <summary>
        /// Whether to generate physics meshes for this tileset.
        /// </summary>
        /// <remarks>
        /// Disabling this option will improve the performance of tile loading,
        /// but it will no longer be possible to collide with the tileset since
        /// the physics meshes will not be created.
        /// </remarks>
        public bool createPhysicsMeshes
        {
            get => this._createPhysicsMeshes;
            set
            {
                this._createPhysicsMeshes = value;
                this.RecreateTileset();
            }
        }

        /// <summary>
        /// Estimate the percentage of the tiles for the current view that have been loaded. 
        /// </summary>
        /// <returns>
        /// A float value between 0 and 100 representing the load progress.
        /// </returns>
        public partial float ComputeLoadProgress();

        private partial void SetShowCreditsOnScreen(bool value);

        private partial void Start();
        private partial void Update();
        private partial void OnValidate();

        private partial void OnEnable();
        private partial void OnDisable();

        /// <summary>
        /// Destroy and recreate the tilset. All tiles are unloaded, and then the tileset is reloaded
        /// based on the current view.
        /// </summary>
        public partial void RecreateTileset();

        /// <summary>
        /// Zoom the Editor camera to this tileset. This method does nothing outside of the Editor.
        /// </summary>
        public partial void FocusTileset();
    }
}
