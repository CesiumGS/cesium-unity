using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CesiumForUnity
{
    [CustomEditor(typeof(Cesium3DTileset))]
    public class Cesium3DTilesetEditor : Editor
    {
        private Cesium3DTileset _tileset;

        private SerializedProperty _showCreditsOnScreen;

        private SerializedProperty _tilesetSource;
        private SerializedProperty _url;
        private SerializedProperty _ionAssetID;
        private SerializedProperty _ionAccessToken;

        private SerializedProperty _maximumScreenSpaceError;

        private SerializedProperty _preloadAncestors;
        private SerializedProperty _preloadSiblings;
        private SerializedProperty _forbidHoles;
        private SerializedProperty _maximumSimultaneousTileLoads;
        private SerializedProperty _maximumCachedBytes;
        private SerializedProperty _loadingDescendantLimit;

        private SerializedProperty _enableFrustumCulling;
        private SerializedProperty _enableFogCulling;
        private SerializedProperty _enforceCulledScreenSpaceError;
        private SerializedProperty _culledScreenSpaceError;

        private SerializedProperty _opaqueMaterial;
        //private SerializedProperty _useLodTransitions;
        //private SerializedProperty _lodTransitionLength;
        private SerializedProperty _generateSmoothNormals;

        private SerializedProperty _pointCloudShading;

        private SerializedProperty _showTilesInHierarchy;
        private SerializedProperty _suspendUpdate;
        private SerializedProperty _updateInEditor;
        private SerializedProperty _logSelectionStats;

        private SerializedProperty _createPhysicsMeshes;

        private void OnEnable()
        {
            this._tileset = (Cesium3DTileset)target;

            this._showCreditsOnScreen =
                this.serializedObject.FindProperty("_showCreditsOnScreen");

            this._tilesetSource = this.serializedObject.FindProperty("_tilesetSource");
            this._url = this.serializedObject.FindProperty("_url");
            this._ionAssetID = this.serializedObject.FindProperty("_ionAssetID");
            this._ionAccessToken = this.serializedObject.FindProperty("_ionAccessToken");

            this._maximumScreenSpaceError =
                this.serializedObject.FindProperty("_maximumScreenSpaceError");

            this._preloadAncestors = this.serializedObject.FindProperty("_preloadAncestors");
            this._preloadSiblings = this.serializedObject.FindProperty("_preloadSiblings");
            this._forbidHoles = this.serializedObject.FindProperty("_forbidHoles");
            this._maximumSimultaneousTileLoads =
                this.serializedObject.FindProperty("_maximumSimultaneousTileLoads");
            this._maximumCachedBytes = this.serializedObject.FindProperty("_maximumCachedBytes");
            this._loadingDescendantLimit =
                this.serializedObject.FindProperty("_loadingDescendantLimit");

            this._enableFrustumCulling =
                this.serializedObject.FindProperty("_enableFrustumCulling");
            this._enableFogCulling = this.serializedObject.FindProperty("_enableFogCulling");
            this._enforceCulledScreenSpaceError =
                this.serializedObject.FindProperty("_enforceCulledScreenSpaceError");
            this._culledScreenSpaceError =
                this.serializedObject.FindProperty("_culledScreenSpaceError");

            this._opaqueMaterial = this.serializedObject.FindProperty("_opaqueMaterial");
            //this._useLodTransitions = this.serializedObject.FindProperty("_useLodTransitions");
            //this._lodTransitionLength =
            //    this.serializedObject.FindProperty("_lodTransitionLength");
            this._generateSmoothNormals =
                this.serializedObject.FindProperty("_generateSmoothNormals");

            this._pointCloudShading = this.serializedObject.FindProperty("_pointCloudShading");

            this._showTilesInHierarchy =
                this.serializedObject.FindProperty("_showTilesInHierarchy");
            this._suspendUpdate = this.serializedObject.FindProperty("_suspendUpdate");
            this._updateInEditor = this.serializedObject.FindProperty("_updateInEditor");
            this._logSelectionStats = this.serializedObject.FindProperty("_logSelectionStats");

            this._createPhysicsMeshes =
                this.serializedObject.FindProperty("_createPhysicsMeshes");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();

            EditorGUIUtility.labelWidth = CesiumEditorStyle.inspectorLabelWidth;
            this.DrawInspectorButtons();
            EditorGUILayout.Space(5);
            this.DrawShowCreditsOnScreenToggle();
            EditorGUILayout.Space(5);
            this.DrawSourceProperties();
            EditorGUILayout.Space(5);
            this.DrawLevelOfDetailProperties();
            EditorGUILayout.Space(5);
            this.DrawTileLoadingProperties();
            EditorGUILayout.Space(5);
            this.DrawTileCullingProperties();
            EditorGUILayout.Space(5);
            this.DrawRenderProperties();
            EditorGUILayout.Space(5);
            this.DrawPointCloudShadingProperties();
            EditorGUILayout.Space(5);
            this.DrawDebugProperties();
            EditorGUILayout.Space(5);
            this.DrawPhysicsProperties();

            this.serializedObject.ApplyModifiedProperties();
        }

        public bool HasFrameBounds()
        {
            return true;
        }

        public Bounds OnGetFrameBounds()
        {
            // HACK: This function only gets called by Unity's editor when it is trying to focus the tileset.
            // Return dummy bounds with infinite extent so Unity's built in focusing fails. This allows us to
            // focus the editor view as we want, without it getting overwritten.

            // TODO: Maybe we can use reflection or something to only do this hack when the SceneView is 
            // invoking this method. It is not ideal for this method to have a side-effect when it is invoked
            // from anywhere else. 
            this._tileset.FocusTileset();
            return new Bounds(
                new Vector3(0, 0, 0),
                new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
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
                this._tileset.RecreateTileset();
            }

            GUIContent troubleshootTokenContent = new GUIContent(
                "Troubleshoot Token",
                "Check if the Cesium ion token used to access this tileset is working correctly, " +
                "and fix it if necessary.");
            if (GUILayout.Button(troubleshootTokenContent))
            {
                IonTokenTroubleshootingWindow.ShowWindow(this._tileset, false);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawShowCreditsOnScreenToggle()
        {
            GUIContent showCreditsOnScreenContent = new GUIContent(
              "Show Credits On Screen",
              "Whether or not to show this tileset's credits on screen.");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this._showCreditsOnScreen, showCreditsOnScreenContent);
            if (EditorGUI.EndChangeCheck())
            {
                // Trigger the OnSetShowCreditsOnScreen event in Cesium3DTileset.
                this._tileset.showCreditsOnScreen = this._showCreditsOnScreen.boolValue;
            }
        }

        private void DrawSourceProperties()
        {
            GUILayout.Label("Source", EditorStyles.boldLabel);

            GUIContent tilesetSourceContent = new GUIContent(
                "Tileset Source",
                "Whether this tileset should be loaded from a URL or from Cesium ion.");
            EditorGUILayout.PropertyField(this._tilesetSource, tilesetSourceContent);

            CesiumDataSource tilesetSource = (CesiumDataSource)this._tilesetSource.enumValueIndex;

            EditorGUI.BeginDisabledGroup(tilesetSource != CesiumDataSource.FromUrl);
            GUIContent urlContent = new GUIContent(
                "URL",
                "The URL of this tileset's \"tileset.json\" file.");
            EditorGUILayout.DelayedTextField(this._url, urlContent);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(tilesetSource != CesiumDataSource.FromCesiumIon);
            GUIContent ionAssetIDContent = new GUIContent(
                "ion Asset ID",
                "The ID of the Cesium ion asset to use.");
            EditorGUILayout.DelayedIntField(this._ionAssetID, ionAssetIDContent);
            GUIContent ionAccessTokenContent = new GUIContent(
                "ion Access Token",
                "The access token to use to access the Cesium ion resource.");
            EditorGUILayout.DelayedTextField(this._ionAccessToken, ionAccessTokenContent);
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
            EditorGUILayout.PropertyField(
                this._maximumScreenSpaceError, maximumScreenSpaceErrorContent);
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
            EditorGUILayout.PropertyField(this._preloadAncestors, preloadAncestorsContent);

            GUIContent preloadSiblingsContent = new GUIContent(
                "Preload Siblings",
                "Whether to preload sibling tiles." +
                "\n\n" +
                "Setting this to true causes tiles with the same parent as a rendered " +
                "tile to be loaded, even if they are culled. Setting this to true may " +
                "provide a better panning experience at the cost of loading more tiles.");
            EditorGUILayout.PropertyField(this._preloadSiblings, preloadSiblingsContent);

            GUIContent forbidHolesContent = new GUIContent(
               "Forbid Holes",
               "Whether to prevent refinement of a parent tile when a child isn't done loading." +
               "\n\n" +
               "When this is set to true, the tileset will guarantee that the tileset will " +
               "never be rendered with holes in place of tiles that are not yet loaded," +
               "even though the tile that is rendered instead may have low resolution. " +
               "When false, overall loading will be faster, but newly-visible parts of the " +
               "tileset may initially be blank.");
            EditorGUILayout.PropertyField(this._forbidHoles, forbidHolesContent);

            GUIContent maximumSimultaneousTileLoadsContent =
                new GUIContent(
                    "Maximum Simultaneous Tile Loads",
                    "The maximum number of tiles that may be loaded at once." +
                    "\n\n" +
                    "When new parts of the tileset become visible, the tasks to load the " +
                    "corresponding tiles are put into a queue. This value determines how " +
                    "many of these tasks are processed at the same time. A higher value may " +
                    "cause the tiles to be loaded and rendered more quickly, at the cost of " +
                    "a higher network and processing load.");
            EditorGUILayout.PropertyField(
                this._maximumSimultaneousTileLoads, maximumSimultaneousTileLoadsContent);

            GUIContent maximumCachedBytesContent = new GUIContent(
                "Maximum Cached Bytes",
                "The maximum number of bytes that may be cached." +
                "\n\n" +
                "Note that this value, even if 0, will never cause tiles that are needed " +
                "for rendering to be unloaded. However, if the total number of loaded " +
                "bytes is greater than this value, tiles will be unloaded until the " +
                "total is under this number or until only required tiles remain, whichever " +
                "comes first.");
            EditorGUILayout.PropertyField(this._maximumCachedBytes, maximumCachedBytesContent);

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
            EditorGUILayout.PropertyField(
                this._loadingDescendantLimit, loadingDescendantLimitContent);
        }

        private void DrawTileCullingProperties()
        {
            GUILayout.Label("Tile Culling", EditorStyles.boldLabel);

            //EditorGUI.BeginDisabledGroup(this._useLodTransitions.boolValue);
            GUIContent enableFrustumCullingContent = new GUIContent(
                "Enable Frustum Culling",
                "Whether to cull tiles that are outside the frustum." +
                "\n\n" +
                "By default this is true, meaning that tiles that are not visible with " +
                "the current camera configuration will be ignored. It can be set to false, " +
                "so that these tiles are still considered for loading, refinement and rendering." +
                "\n\n" +
                "This will cause more tiles to be loaded, but helps to avoid holes and " +
                "provides a more consistent mesh, which may be helpful for physics and shadows." +
                "\n\n" +
                "Note that this will always be disabled if \"Use Lod Transitions\" is set to true.");
            EditorGUILayout.PropertyField(this._enableFrustumCulling, enableFrustumCullingContent);

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
            EditorGUILayout.PropertyField(this._enableFogCulling, enableFogCullingContent);
            //EditorGUI.EndDisabledGroup();

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

            EditorGUILayout.PropertyField(
                this._enforceCulledScreenSpaceError, enforceCulledScreenSpaceErrorContent);

            EditorGUI.BeginDisabledGroup(!this._enforceCulledScreenSpaceError.boolValue);
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
            EditorGUILayout.PropertyField(
                this._culledScreenSpaceError, culledScreenSpaceErrorContent);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawRenderProperties()
        {
            GUILayout.Label("Render", EditorStyles.boldLabel);

            GUIContent opaqueMaterialContent = new GUIContent(
                "Opaque Material",
                "The Material to use to render opaque parts of tiles.");
            EditorGUILayout.PropertyField(this._opaqueMaterial, opaqueMaterialContent);

            //GUIContent useLodTransitionsContent = new GUIContent(
            //    "Use Lod Transitions",
            //    "Use a dithering effect when transitioning between tiles of different LODs." +
            //    "\n\n" +
            //    "When this is set to true, Frustrum Culling and Fog Culling are always disabled.");
            //EditorGUILayout.PropertyField(this._useLodTransitions, useLodTransitionsContent);

            //EditorGUI.BeginDisabledGroup(!this._useLodTransitions.boolValue);
            //GUIContent lodTransitionLengthContent = new GUIContent(
            //    "Lod Transition Length",
            //    "How long dithered LOD transitions between different tiles should take, in seconds." +
            //    "\n\n" +
            //    "Only relevant if \"Use Lod Transitions\" is true.");
            //EditorGUILayout.PropertyField(this._lodTransitionLength, lodTransitionLengthContent);
            //EditorGUI.EndDisabledGroup();

            GUIContent generateSmoothNormalsContent = new GUIContent(
                "Generate Smooth Normals",
                "Whether to generate smooth normals when normals are missing in the glTF." +
                "\n\n" +
                "According to the glTF spec: \"When normals are not specified, client " +
                "implementations should calculate flat normals.\" However, calculating flat " +
                "normals requires duplicating vertices. This option allows the glTFs to be " +
                "rendered with smooth normals instead when the original glTF is missing normals.");
            EditorGUILayout.PropertyField(this._generateSmoothNormals, generateSmoothNormalsContent);
        }

        private void DrawPointCloudShadingProperties()
        {
            // EditorGUILayout.PropertyField will trigger OnValidate() for the tileset,
            // even though the properties belong to CesiumPointCloudShading.
            // To avoid refreshing the tileset every time a parameter is changed,
            // the values are modified using the CesiumPointCloudShading setters instead.
            GUILayout.Label("Point Cloud Shading", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            SerializedProperty attenuationProperty =
                this._pointCloudShading.FindPropertyRelative("_attenuation");
            GUIContent attenuationContent = new GUIContent(
               "Attenuation",
               "Whether or not to perform point attenuation. Attenuation controls the size of " +
               "the points rendered based on the geometric error of their tile.");
            bool attenuationValue =
                EditorGUILayout.Toggle(attenuationContent, attenuationProperty.boolValue);

            SerializedProperty geometricErrorScaleProperty =
                this._pointCloudShading.FindPropertyRelative("_geometricErrorScale");
            GUIContent geometricErrorScaleContent = new GUIContent(
                "Geometric Error Scale",
                "The scale to be applied to the tile's geometric error before it is used " +
                "to compute attenuation. Larger values will result in larger points.");
            float geometricErrorScaleValue = EditorGUILayout.FloatField(
                geometricErrorScaleContent,
                geometricErrorScaleProperty.floatValue);

            SerializedProperty maximumAttenuationProperty =
                this._pointCloudShading.FindPropertyRelative("_maximumAttenuation");
            GUIContent maximumAttenuationContent = new GUIContent(
                "Maximum Attenuation",
                "The maximum point attenuation in pixels. If this is zero, the " +
                "Cesium3DTileset's maximumScreenSpaceError will be used as the " +
                "maximum point attenuation.");
            float maximumAttenuationValue = EditorGUILayout.FloatField(
                maximumAttenuationContent,
                maximumAttenuationProperty.floatValue);

            SerializedProperty baseResolutionProperty =
                this._pointCloudShading.FindPropertyRelative("_baseResolution");
            GUIContent baseResolutionContent = new GUIContent(
                "Base Resolution",
                "The average base resolution for the dataset in meters. " +
                "For example, a base resolution of 0.05 assumes an original " +
                "capture resolution of 5 centimeters between neighboring points." +
                "\n\n" +
                "This is used in place of geometric error when the tile's " +
                "geometric error is 0. If this value is zero, each tile with " +
                "a geometric error of 0 will have its geometric error " +
                "approximated instead.");
            float baseResolutionValue = EditorGUILayout.FloatField(
                baseResolutionContent,
                baseResolutionProperty.floatValue);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(
                    this._tileset,
                    "Modified Point Cloud Shading in " + this._tileset.gameObject.name);
                this._tileset.pointCloudShading.attenuation = attenuationValue;
                this._tileset.pointCloudShading.geometricErrorScale = geometricErrorScaleValue;
                this._tileset.pointCloudShading.maximumAttenuation = maximumAttenuationValue;
                this._tileset.pointCloudShading.baseResolution = baseResolutionValue;
                // Force the scene view to repaint so that the changes are immediately reflected.
                SceneView.RepaintAll();
            }
        }

        private void DrawDebugProperties()
        {
            GUILayout.Label("Debug", EditorStyles.boldLabel);

            GUIContent showTilesInHierarchyContent = new GUIContent(
                "Show Tiles In Hierarchy",
                "Whether to show tiles as individual game objects in the hierarchy window.");
            EditorGUILayout.PropertyField(this._showTilesInHierarchy, showTilesInHierarchyContent);

            GUIContent suspendUpdateContent = new GUIContent(
                "Suspend Update",
                "Pauses level-of-detail and culling updates of this tileset.");
            EditorGUILayout.PropertyField(this._suspendUpdate, suspendUpdateContent);

            GUIContent updateInEditorContent = new GUIContent(
                "Update in Editor",
                "If true, this tileset is ticked/updated in the editor. " +
                "If false, it is only ticked while playing (including Play-in-Editor).");
            EditorGUILayout.PropertyField(this._updateInEditor, updateInEditorContent);

            GUIContent logSelectionStatsContent = new GUIContent(
                "Log Selection Stats",
                "Whether to log details about the tile selection process.");
            EditorGUILayout.PropertyField(this._logSelectionStats, logSelectionStatsContent);
        }

        private void DrawPhysicsProperties()
        {
            GUILayout.Label("Physics", EditorStyles.boldLabel);

            GUIContent createPhysicsMeshesContent = new GUIContent(
                "Create Physics Meshes",
                "Whether to generate physics meshes for this tileset." +
                "\n\n" +
                "Disabling this option will improve the performance of tile loading, " +
                "but it will no longer be possible to collide with the tileset since " +
                "the physics meshes will not be created." +
                "\n\n" +
                "Physics meshes cannot be generated for primitives containing points.");
            EditorGUILayout.PropertyField(this._createPhysicsMeshes, createPhysicsMeshesContent);
        }
    }
}
