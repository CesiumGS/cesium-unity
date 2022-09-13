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
        [Tooltip("The maximum number of pixels of error when rendering this tileset." +
            "\n\n" +
            "This is used to select an appropriate level-of-detail: A low value will " +
            "cause many tiles with a high level of detail to be loaded, causing a " +
            "finer visual representation of the tiles, but with a higher performance " +
            "cost for loading and rendering. A higher value will cause a coarser " +
            "visual representation, with lower performance requirements." +
            "\n\n" +
            "When a tileset uses the older layer.json / quantized-mesh format rather " +
            "than 3D Tiles, this value is effectively divided by 8.0. So the default " +
            "value of 16.0 corresponds to the standard value for quantized-mesh " +
            "terrain of 2.0.")]
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
        [Tooltip("Whether to preload ancestor tiles." +
            "\n\n" +
            "Setting this to true optimizes the zoom-out experience and provides more " +
            "detail in newly-exposed areas when panning. The down side is that it " +
            "requires loading more tiles.")]
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
        [Tooltip("Whether to preload sibling tiles." +
            "\n\n" +
            "Setting this to true causes tiles with the same parent as a rendered " +
            "tile to be loaded, even if they are culled. Setting this to true may " +
            "provide a better panning experience at the cost of loading more tiles.")]
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
        [Tooltip("Whether to unrefine back to a parent tile when a child isn't done loading." +
            "\n\n" +
            "When this is set to true, the tileset will guarantee that the tileset will " +
            "never be rendered with holes in place of tiles that are not yet loaded," +
            "even though the tile that is rendered instead may have low resolution. " +
            "When false, overall loading will be faster, but newly-visible parts of the " +
            "tileset may initially be blank.")]
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
        [Tooltip("The maximum number of tiles that may be loaded at once." +
            "\n\n" +
            "When new parts of the tileset become visible, the tasks to load the " +
            "corresponding tiles are put into a queue. This value determines how many " +
            "of these tasks are processed at the same time. A higher value may cause " +
            "the tiles to be loaded and rendered more quickly, at the cost of a higher " +
            "network- and processing load.")]
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
        [Tooltip("The maximum number of bytes that may be cached." +
            "\n\n" +
            "Note that this value, even if 0, will never cause tiles that are needed " +
            "for rendering to be unloaded. However, if the total number of loaded " +
            "bytes is greater than this value, tiles will be unloaded until the " +
            "total is under this number or until only required tiles remain, whichever " +
            "comes first.")]
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
        [Tooltip("The number of loading descendents a tile should allow before " +
            "deciding to render itself instead of waiting." +
            "\n\n" +
            "Setting this to 0 will cause each level of detail to be loaded " +
            "successively. This will increase the overall loading time, but cause " +
            "additional detail to appear more gradually. Setting this to a high value " +
            "like 1000 will decrease the overall time until the desired level of detail " +
            "is achieved, but this high-detail representation will appear at once, as " +
            "soon as it is loaded completely.")]
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
        [Header("Tile Culling")]
        [Tooltip("Whether to cull tiles that are outside the frustum." +
            "\n\n" +
            "By default this is true, meaning that tiles that are not visible with the " +
            "current camera configuration will be ignored. It can be set to false, so " +
            "that these tiles are still considered for loading, refinement and rendering." +
            "\n\n" +
            "This will cause more tiles to be loaded, but helps to avoid holes and " +
            "provides a more consistent mesh, which may be helpful for physics." +
            "\n\n" +
            "Note that this will always be disabled if \"Use Lod Transitions\" is set to true.")]
        [InspectorName("Enable Frustum Culling")]
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
        [Tooltip("Whether to cull tiles that are occluded by fog." +
            "\n\n" +
            "This does not refer to the atmospheric fog of the Unreal Engine, but to " +
            "an internal representation of fog: Depending on the height of the camera " +
            "above the ground, tiles that are far away (close to the horizon) will be " +
            "culled when this flag is enabled." +
            "\n\n" +
            "Note that this will always be disabled if \"Use Lod Transitions\" is set to true.")]
        [InspectorName("Enable Fog Culling")]
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
        [Tooltip("Whether a specified screen-space error should be enforced for tiles " +
            "that are outside the frustum or hidden in fog." +
            "\n\n" +
            "When \"Enable Frustum Culling\" and \"Enable Fog Culling\" are both true, " +
            "tiles outside the view frustum or hidden in fog are effectively ignored, " +
            "and so their level-of-detail doesn't matter. And in this scenario, this " +
            "property is ignored." +
            "\n\n" +
            "However, when either of those flags are false, these \"would-be-culled\" " +
            "tiles continue to be processed, and the question arises of how to handle " +
            "their level-of-detail. When this property is false, refinement terminates " +
            "at these tiles, no matter what their current screen-space error. The tiles " +
            "are available for physics, shadows, etc., but their level-of-detail may be " +
            "very low." +
            "\n\n" +
            "When set to true, these tiles are refined until they achieve the specified " +
            "\"Culled Screen Space Error\". This allows control over the minimum quality " +
            "of these would-be-culled tiles.")]
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
        [Tooltip("The screen-space error to be enforced for tiles that are outside " +
            "the frustum or hidden in fog." +
            "\n\n" +
            "When \"Enable Frustum Culling\" and \"Enable Fog Culling\" are both true, " +
            "tiles outside the view frustum or hidden in fog are effectively ignored, " +
            "and so their level-of-detail doesn't matter. And in this scenario, this " +
            "property is ignored." +
            "\n\n" +
            "However, when either of those flags are false, these \"would-be-culled\" " +
            "tiles continue to be processed, and the question arises of how to handle " +
            "their level-of-detail. When this property is false, refinement terminates " +
            "at these tiles, no matter what their current screen-space error. The tiles " +
            "are available for physics, shadows, etc., but their level-of-detail may be " +
            "very low." +
            "\n\n" +
            "When set to true, these tiles are refined until they achieve the specified " +
            "\"Culled Screen Space Error\". This allows control over the minimum quality " +
            "of these would-be-culled tiles.")]
        [InspectorName("Culled Screen Space Error")]
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
        [Tooltip("Use a dithering effect when transitioning between tiles of different LODs." +
            "\n\n" +
            "When this is set to true, Frustrum Culling and Fog Culling are always disabled.")]
        [InspectorName("Use Lod Transitions")]
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
        [Tooltip("How long dithered LOD transitions between different tiles should take, in seconds." +
            "\n\n" +
            "Only relevant if \"Use Lod Transitions\" is true.")]
        [InspectorName("Lod Transition Length")]
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
        [Tooltip("Whether to generate smooth normals when normals are missing in the glTF." +
            "\n\n" +
            "According to the glTF spec: \"When normals are not specified, client " +
            "implementations should calculate flat normals.\" However, calculating flat " +
            "normals requires duplicating vertices. This option allows the glTFs to be " +
            "sent with explicit smooth normals when the original glTF was missing normals.")]
        [InspectorName("Generate Smooth Normals")]
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
        [Header("Debug")]
        [Tooltip("Pauses level-of-detail and culling updates of this tileset.")]
        [InspectorName("Suspend Update")]
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
        [Tooltip("If true, this tileset is ticked/updated in the editor. " +
            "If false, it is only ticked while playing (including Play-in-Editor).")]
        [InspectorName("Update in Editor")]
        private bool _updateInEditor = true;

        public bool updateInEditor
        {
            get => this._updateInEditor;
            set
            {
                this._updateInEditor = value;
            }
        }

        [SerializeField]
        [Tooltip("Whether to log details about the tile selection process.")]
        [InspectorName("Log Selection Stats")]
        private bool _logSelectionStats = false;

        public bool logSelectionStats
        {
            get => this._logSelectionStats;
            set { this._logSelectionStats = value; }
        }

        [SerializeField]
        [Header("Physics")]
        [Tooltip("Whether to generate physics meshes for this tileset.\n\n" +
            "Disabling this option will improve the performance of tile loading, " +
            "but it will no longer be possible to collide with the tileset since " +
            "the physics meshes will not be created.")]
        [InspectorName("Create Physics Meshes")]
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

        private partial void RecreateTileset();
    }
}
