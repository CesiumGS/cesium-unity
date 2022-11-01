using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(Cesium3DTileset))]
    [CanEditMultipleObjects]
    public class Cesium3DTilesetEditor : Editor
    {
        private Cesium3DTileset _tileset;

        internal Cesium3DTileset tileset
        {
            get => this._tileset;
            set
            {
                this._tileset = value;
            }
        }

        internal bool showCreditsOnScreen
        {
            get => this._tileset.showCreditsOnScreen;
            set
            {
                if (this._tileset.showCreditsOnScreen != value)
                {
                    this._tileset.showCreditsOnScreen = value;
                }
            }
        }

        internal CesiumDataSource tilesetSource
        {
            get => this._tileset.tilesetSource;
            set
            {
                if (this._tileset.tilesetSource != value)
                {
                    this._tileset.tilesetSource = value;
                }
            }
        }

        internal string url
        {
            get => this._tileset.url;
            set
            {
                if (this._tileset.url != value)
                {
                    this._tileset.url = value;
                }
            }
        }

        internal long ionAssetID
        {
            get => this._tileset.ionAssetID;
            set
            {
                if (this._tileset.ionAssetID != value)
                {
                    this._tileset.ionAssetID = value;
                }
            }
        }

        internal string ionAccessToken
        {
            get => this._tileset.ionAccessToken;
            set
            {
                if (this._tileset.ionAccessToken != value)
                {
                    this._tileset.ionAccessToken = value;
                }
            }
        }

        internal float maximumScreenSpaceError
        {
            get => this._tileset.maximumScreenSpaceError;
            set
            {
                if (this._tileset.maximumScreenSpaceError != value)
                {
                    this._tileset.maximumScreenSpaceError = value;
                }
            }
        }

        internal bool preloadAncestors
        {
            get => this._tileset.preloadAncestors;
            set
            {
                if (this._tileset.preloadAncestors != value)
                {
                    this._tileset.preloadAncestors = value;
                }
            }
        }

        internal bool preloadSiblings
        {
            get => this._tileset.preloadSiblings;
            set
            {
                if (this._tileset.preloadSiblings != value)
                {
                    this._tileset.preloadSiblings = value;
                }
            }
        }

        internal bool forbidHoles
        {
            get => this._tileset.forbidHoles;
            set
            {
                if (this._tileset.forbidHoles != value)
                {
                    this._tileset.forbidHoles = value;
                }
            }
        }

        internal uint maximumSimultaneousTileLoads
        {
            get => this._tileset.maximumSimultaneousTileLoads;
            set
            {
                if (this._tileset.maximumSimultaneousTileLoads != value)
                {
                    this._tileset.maximumSimultaneousTileLoads = value;
                }
            }
        }

        internal long maximumCachedBytes
        {
            get => this._tileset.maximumCachedBytes;
            set
            {
                if (this._tileset.maximumCachedBytes != value)
                {
                    this._tileset.maximumCachedBytes = value;
                }
            }
        }

        internal uint loadingDescendantLimit
        {
            get => this._tileset.loadingDescendantLimit;
            set
            {
                if (this._tileset.loadingDescendantLimit != value)
                {
                    this._tileset.loadingDescendantLimit = value;
                }
            }
        }

        internal bool enableFrustumCulling
        {
            get => this._tileset.enableFrustumCulling;
            set
            {
                if (this._tileset.enableFrustumCulling != value)
                {
                    this._tileset.enableFrustumCulling = value;
                }
            }
        }

        internal bool enableFogCulling
        {
            get => this._tileset.enableFogCulling;
            set
            {
                if (this._tileset.enableFogCulling != value)
                {
                    this._tileset.enableFogCulling = value;
                }
            }
        }

        internal bool enforceCulledScreenSpaceError
        {
            get => this._tileset.enforceCulledScreenSpaceError;
            set
            {
                if (this._tileset.enforceCulledScreenSpaceError != value)
                {
                    this._tileset.enforceCulledScreenSpaceError = value;
                }
            }
        }

        internal float culledScreenSpaceError
        {
            get => this._tileset.culledScreenSpaceError;
            set
            {
                if (this._tileset.culledScreenSpaceError != value)
                {
                    this._tileset.culledScreenSpaceError = value;
                }
            }
        }

        internal Material? opaqueMaterial
        {
            get => this._tileset.opaqueMaterial;
            set
            {
                if (this._tileset.opaqueMaterial != value)
                {
                    this._tileset.opaqueMaterial = value;
                }
            }
        }

        internal bool useLodTransitions
        {
            get => this._tileset.useLodTransitions;
            set
            {
                if (this._tileset.useLodTransitions != value)
                {
                    this._tileset.useLodTransitions = value;
                }
            }
        }

        internal float lodTransitionLength
        {
            get => this._tileset.lodTransitionLength;
            set
            {
                if (this._tileset.lodTransitionLength != value)
                {
                    this._tileset.lodTransitionLength = value;
                }
            }
        }

        internal bool generateSmoothNormals
        {
            get => this._tileset.generateSmoothNormals;
            set
            {
                if (this._tileset.generateSmoothNormals != value)
                {
                    this._tileset.generateSmoothNormals = value;
                }
            }
        }

        internal bool suspendUpdate
        {
            get => this._tileset.suspendUpdate;
            set
            {
                if (this._tileset.suspendUpdate != value)
                {
                    this._tileset.suspendUpdate = value;
                }
            }
        }

        internal bool updateInEditor
        {
            get => this._tileset.updateInEditor;
            set
            {
                if (this._tileset.updateInEditor != value)
                {
                    this._tileset.updateInEditor = value;
                }
            }
        }

        internal bool logSelectionStats
        {
            get => this._tileset.logSelectionStats;
            set
            {
                if (this._tileset.logSelectionStats != value)
                {
                    this._tileset.logSelectionStats = value;
                }
            }
        }

        internal bool createPhysicsMeshes
        {
            get => this._tileset.createPhysicsMeshes;
            set
            {
                if (this._tileset.createPhysicsMeshes != value)
                {
                    this._tileset.createPhysicsMeshes = value;
                }
            }
        }

        // Unity doesn't provide a formal method for uints, so this
        // is simulated with an IntField and a range check.
        private static uint UintField(GUIContent content, uint value)
        {
            int result = EditorGUILayout.IntField(content, (int)value);
            return (uint)Mathf.Max(result, 0);
        }

        private void OnEnable()
        {
            this.tileset = (Cesium3DTileset)target;
        }

        public override void OnInspectorGUI()
        {
            DrawInspectorButtons();
            EditorGUILayout.Space(5);
            DrawShowCreditsOnScreenToggle();
            EditorGUILayout.Space(5);
            DrawSourceProperties();
            EditorGUILayout.Space(5);
            DrawLevelOfDetailProperties();
            EditorGUILayout.Space(5);
            DrawTileLoadingProperties();
            EditorGUILayout.Space(5);
            DrawTileCullingProperties();
            EditorGUILayout.Space(5);
            DrawRenderProperties();
            EditorGUILayout.Space(5);
            DrawDebugProperties();
            EditorGUILayout.Space(5);
            DrawPhysicsProperties();
        }

        private void DrawInspectorButtons()
        {
            GUILayout.BeginHorizontal();
            GUIContent refreshTilesetContent = new GUIContent(
                "Refresh Tileset",
                "Refreshes this tileset, ensuring that all materials and other settings are " +
                "applied. It is not usually necessary to invoke this, but when behind-the-scenes " +
                "changes are made and not reflected in the tileset, this function can help.");
            if (GUILayout.Button(refreshTilesetContent))
            {
                this.tileset.RecreateTileset();
            }

            GUIContent troubleshootTokenContent = new GUIContent(
                "Troubleshoot Token",
                "Check if the Cesium ion token used to access this tileset is working correctly, " +
                "and fix it if necessary.");
            if (GUILayout.Button(troubleshootTokenContent))
            {
                IonTokenTroubleshootingWindow.ShowWindow(this.tileset, false);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawShowCreditsOnScreenToggle()
        {
            GUIContent showCreditsOnScreenContent = new GUIContent(
              "Show Credits On Screen",
              "Whether or not to show this tileset's credits on screen.");
            this.showCreditsOnScreen =
                EditorGUILayout.Toggle(showCreditsOnScreenContent, this.showCreditsOnScreen);
        }

        private void DrawSourceProperties()
        {
            GUILayout.Label("Source", EditorStyles.boldLabel);

            GUIContent tilesetSourceContent = new GUIContent(
                "Tileset Source",
                "Whether this tileset should be loaded from a URL or from Cesium ion.");
            this.tilesetSource = (CesiumDataSource)
                EditorGUILayout.EnumPopup(tilesetSourceContent, tileset.tilesetSource);

            EditorGUI.BeginDisabledGroup(tileset.tilesetSource != CesiumDataSource.FromUrl);
            GUIContent urlContent = new GUIContent(
                "URL",
                "The URL of this tileset's \"tileset.json\" file.");
            this.url = EditorGUILayout.DelayedTextField(urlContent, this.url);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(tileset.tilesetSource != CesiumDataSource.FromCesiumIon);
            GUIContent ionAssetIDContent = new GUIContent(
                "ion Asset ID",
                "The ID of the Cesium ion asset to use.");
            this.ionAssetID = EditorGUILayout.DelayedIntField(ionAssetIDContent, (int)this.ionAssetID);
            GUIContent ionAccessTokenContent = new GUIContent(
                "ion Access Token",
                "The access token to use to access the Cesium ion resource.");
            this.ionAccessToken = EditorGUILayout.DelayedTextField(
                ionAccessTokenContent,
                this.ionAccessToken);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawLevelOfDetailProperties()
        {
            GUILayout.Label("Level of Detail", EditorStyles.boldLabel);

            GUIContent maximumScreenSpaceErrorContent =
                new GUIContent("Maximum Screen Space Error",
                "The maximum number of pixels of error when rendering this tileset." +
                "\n\n" +
                "This is used to select an appropriate level-of-detail: A low value " +
                "will cause many tiles with a high level of detail to be loaded, causing " +
                "a finer visual representation of the tiles, but with a higher performance " +
                "cost for loading and rendering. A higher value will cause a coarser " +
                "visual representation, with lower performance requirements." +
                "\n\n" +
                "When a tileset uses the older layer.json / quantized-mesh format rather " +
                "than 3D Tiles, this value is effectively divided by 8.0. So the default " +
                "value of 16.0 corresponds to the standard value for quantized-mesh " +
                "terrain of 2.0.");
            this.maximumScreenSpaceError
                = EditorGUILayout.FloatField(maximumScreenSpaceErrorContent,
                                           this.maximumScreenSpaceError);
        }

        private void DrawTileLoadingProperties()
        {
            GUILayout.Label("Tile Loading", EditorStyles.boldLabel);

            GUIContent preloadAncestorsContent = new GUIContent(
                "Preload Ancestors",
                "Whether to preload ancestor tiles." +
                "\n\n" +
                "Setting this to true optimizes the zoom-out experience and provides more " +
                "detail in newly-exposed areas when panning. The down side is that it " +
                "requires loading more tiles.");
            this.preloadAncestors
                = EditorGUILayout.Toggle(preloadAncestorsContent, this.preloadAncestors);

            GUIContent preloadSiblingsContent = new GUIContent(
                "Preload Siblings",
                "Whether to preload sibling tiles." +
                "\n\n" +
                "Setting this to true causes tiles with the same parent as a rendered " +
                "tile to be loaded, even if they are culled. Setting this to true may " +
                "provide a better panning experience at the cost of loading more tiles.");
            this.preloadSiblings
                = EditorGUILayout.Toggle(preloadSiblingsContent, this.preloadSiblings);

            GUIContent forbidHolesContent = new GUIContent(
               "Forbid Holes",
               "Whether to unrefine back to a parent tile when a child isn't done loading." +
               "\n\n" +
               "When this is set to true, the tileset will guarantee that the tileset will " +
               "never be rendered with holes in place of tiles that are not yet loaded," +
               "even though the tile that is rendered instead may have low resolution. " +
               "When false, overall loading will be faster, but newly-visible parts of the " +
               "tileset may initially be blank.");
            this.forbidHoles
                = EditorGUILayout.Toggle(forbidHolesContent, this.forbidHoles);

            GUIContent maximumSimultaneousTileLoadsContent =
                new GUIContent(
                    "Maximum Simultaneous Tile Loads",
                    "The maximum number of tiles that may be loaded at once." +
                    "\n\n" +
                    "When new parts of the tileset become visible, the tasks to load the " +
                    "corresponding tiles are put into a queue. This value determines how " +
                    "many of these tasks are processed at the same time. A higher value may " +
                    "cause the tiles to be loaded and rendered more quickly, at the cost of " +
                    "a higher network- and processing load.");
            this.maximumSimultaneousTileLoads = UintField(
                maximumSimultaneousTileLoadsContent,
                this.maximumSimultaneousTileLoads);

            GUIContent maximumCachedBytesContent = new GUIContent(
                "Maximum Cached Bytes",
                "The maximum number of bytes that may be cached." +
                "\n\n" +
                "Note that this value, even if 0, will never cause tiles that are needed " +
                "for rendering to be unloaded. However, if the total number of loaded " +
                "bytes is greater than this value, tiles will be unloaded until the " +
                "total is under this number or until only required tiles remain, whichever " +
                "comes first.");
            this.maximumCachedBytes = EditorGUILayout.LongField(
                maximumCachedBytesContent,
                this.maximumCachedBytes);

            GUIContent loadingDescendantLimitContent = new GUIContent(
                "Loading Descendant Limit",
                "The number of loading descendents a tile should allow before " +
                "deciding to render itself instead of waiting." +
                "\n\n" +
                "Setting this to 0 will cause each level of detail to be loaded " +
                "successively. This will increase the overall loading time, but cause " +
                "additional detail to appear more gradually. Setting this to a high value " +
                "like 1000 will decrease the overall time until the desired level of detail " +
                "is achieved, but this high-detail representation will appear at once, as " +
                "soon as it is loaded completely.");
            this.loadingDescendantLimit = UintField(
                loadingDescendantLimitContent,
                this.loadingDescendantLimit);
        }

        private void DrawTileCullingProperties()
        {
            GUILayout.Label("Tile Culling", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(this.useLodTransitions);
            GUIContent enableFrustumCullingContent = new GUIContent(
                "Enable Frustum Culling",
                "Whether to cull tiles that are outside the frustum." +
                "\n\n" +
                "By default this is true, meaning that tiles that are not visible with " +
                "the current camera configuration will be ignored. It can be set to false, " +
                "so that these tiles are still considered for loading, refinement and rendering." +
                "\n\n" +
                "This will cause more tiles to be loaded, but helps to avoid holes and " +
                "provides a more consistent mesh, which may be helpful for physics." +
                "\n\n" +
                "Note that this will always be disabled if \"Use Lod Transitions\" is set to true.");
            this.enableFrustumCulling =
                EditorGUILayout.Toggle(enableFrustumCullingContent, this.enableFrustumCulling);

            GUIContent enableFogCullingContent = new GUIContent(
                "Enable Fog Culling",
                "Whether to cull tiles that are occluded by fog." +
                "\n\n" +
                "This does not refer to the atmospheric fog rendered by Unity, but to an " +
                "internal representation of fog: Depending on the height of the camera " +
                "above the ground, tiles that are far away (close to the horizon) will be " +
                "culled when this flag is enabled." +
                "\n\n" +
                "Note that this will always be disabled if \"Use Lod Transitions\" is set to true.");
            this.enableFogCulling =
                EditorGUILayout.Toggle(enableFogCullingContent, this.enableFogCulling);
            EditorGUI.EndDisabledGroup();

            GUIContent enforceCulledScreenSpaceErrorContent = new GUIContent(
                "Enforce Culled Screen Space Error",
                "Whether a specified screen-space error should be enforced for tiles " +
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
                "of these would-be-culled tiles.");
            this.enforceCulledScreenSpaceError =
                EditorGUILayout.Toggle(
                    enforceCulledScreenSpaceErrorContent,
                    this.enforceCulledScreenSpaceError);

            EditorGUI.BeginDisabledGroup(!this.enforceCulledScreenSpaceError);
            GUIContent culledScreenSpaceErrorContent = new GUIContent(
                "Culled Screen Space Error",
                "The screen-space error to be enforced for tiles that are outside " +
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
                "of these would-be-culled tiles.");
            this.culledScreenSpaceError =
                EditorGUILayout.FloatField(
                    culledScreenSpaceErrorContent,
                    this.culledScreenSpaceError);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawRenderProperties()
        {
            GUILayout.Label("Render", EditorStyles.boldLabel);

            GUIContent opaqueMaterialContent = new GUIContent(
                "Opaque Material",
                "The Material to use to render opaque parts of tiles.");
            this.opaqueMaterial = (Material)EditorGUILayout.ObjectField(
                opaqueMaterialContent,
                this.opaqueMaterial,
                typeof(Material));

            GUIContent useLodTransitionsContent = new GUIContent(
                "Use Lod Transitions",
                "Use a dithering effect when transitioning between tiles of different LODs." +
                "\n\n" +
                "When this is set to true, Frustrum Culling and Fog Culling are always disabled.");
            this.useLodTransitions =
                EditorGUILayout.Toggle(useLodTransitionsContent, this.useLodTransitions);

            EditorGUI.BeginDisabledGroup(!this.useLodTransitions);
            GUIContent lodTransitionLengthContent = new GUIContent(
                "Lod Transition Length",
                "How long dithered LOD transitions between different tiles should take, in seconds." +
                "\n\n" +
                "Only relevant if \"Use Lod Transitions\" is true.");
            this.lodTransitionLength =
                EditorGUILayout.FloatField(lodTransitionLengthContent, this.lodTransitionLength);
            EditorGUI.EndDisabledGroup();

            GUIContent generateSmoothNormalsContent = new GUIContent(
                "Generate Smooth Normals",
                "Whether to generate smooth normals when normals are missing in the glTF." +
                "\n\n" +
                "According to the glTF spec: \"When normals are not specified, client " +
                "implementations should calculate flat normals.\" However, calculating flat " +
                "normals requires duplicating vertices. This option allows the glTFs to be " +
                "sent with explicit smooth normals when the original glTF was missing normals.");
            this.generateSmoothNormals = 
                EditorGUILayout.Toggle(generateSmoothNormalsContent, this.generateSmoothNormals);
        }

        private void DrawDebugProperties()
        {
            GUILayout.Label("Debug", EditorStyles.boldLabel);

            GUIContent suspendUpdateContent = new GUIContent(
                "Suspend Update",
                "Pauses level-of-detail and culling updates of this tileset.");
            this.suspendUpdate =
                EditorGUILayout.Toggle(suspendUpdateContent, this.suspendUpdate);

            GUIContent updateInEditorContent = new GUIContent(
                "Update in Editor",
                "If true, this tileset is ticked/updated in the editor. " +
                "If false, it is only ticked while playing (including Play-in-Editor).");
            this.updateInEditor =
                EditorGUILayout.Toggle(updateInEditorContent, this.updateInEditor);

            GUIContent logSelectionStatsContent = new GUIContent(
                "Log Selection Stats",
                "Whether to log details about the tile selection process.");
            this.logSelectionStats =
                EditorGUILayout.Toggle(logSelectionStatsContent, this.logSelectionStats);
        }

        private void DrawPhysicsProperties()
        {
            GUILayout.Label("Physics", EditorStyles.boldLabel);

            GUIContent createPhysicsMeshesContent = new GUIContent(
                "Create Physics Meshes",
                "Whether to generate physics meshes for this tileset.\n\n" +
                "Disabling this option will improve the performance of tile loading, " +
                "but it will no longer be possible to collide with the tileset since " +
                "the physics meshes will not be created.");
            this.createPhysicsMeshes =
                EditorGUILayout.Toggle(createPhysicsMeshesContent, this.createPhysicsMeshes);
        }
    }
}
