using Reinterop;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace CesiumForUnity
{
    public class CesiumIonAssetsWindow : EditorWindow
    {
        public static CesiumIonAssetsWindow currentWindow = null;

        [MenuItem("Cesium/Cesium ion Assets")]
        public static void ShowWindow()
        {
            if (currentWindow == null)
            {
                // Get existing open window or if none, make a new one docked next
                // to the Project / Console window.
                Type[] siblingWindows = new Type[] {
                    Type.GetType("UnityEditor.ProjectBrowser,UnityEditor.dll"),
                    Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll")
                };
                currentWindow = GetWindow<CesiumIonAssetsWindow>(siblingWindows);
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        private TreeViewState _assetsTreeState;
        private IonAssetsTreeView _assetsTreeView;
        private SearchField _searchField;

        private void OnEnable()
        { 
            // Load the icon separately from the other resources.
            Texture2D icon = Resources.Load<Texture2D>("Cesium-64x64");
            icon.wrapMode = TextureWrapMode.Clamp;
            this.titleContent = new GUIContent("Cesium ion Assets", icon);

            this._searchField = new SearchField();

            CesiumIonSession.Ion().Resume();
            BuildTreeView();
            CesiumIonSession.OnConnectionUpdated += this._assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated += this._assetsTreeView.Refresh;

            CesiumIonSession.Ion().RefreshAssets();
        }

        private void OnDisable()
        {
            CesiumIonSession.OnConnectionUpdated -= this._assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated -= this._assetsTreeView.Refresh;
        }

        void BuildTreeView()
        {
            this._assetsTreeState = new TreeViewState();
            this._assetsTreeView = new IonAssetsTreeView(this._assetsTreeState);
            this._assetsTreeView.Reload();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            this.DrawAssetListPanel();
            this.DrawAssetDescriptionPanel();
            GUILayout.EndHorizontal();

            // Force the window to repaint if the cursor is hovered over it.
            // By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.
            if (mouseOverWindow == currentWindow)
            {
                Repaint();
            }
        }

        void DrawAssetListPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
            this.DrawRefreshButtonAndSearchBar();
            GUILayout.Space(15);
            this.DrawAssetTreeView();
            GUILayout.EndVertical();
        }

        private string _assetsSearchString = "";

        void DrawRefreshButtonAndSearchBar()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(
                new GUIContent(CesiumEditorStyle.refreshIcon, "Refresh the asset list"),
                CesiumEditorStyle.refreshButtonStyle))
            {
                CesiumIonSession.Ion().RefreshAssets();
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(15);

            string searchString = this._searchField.OnToolbarGUI(
                this._assetsSearchString,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            if (searchString != null)
            {
                searchString = searchString.Trim();
                if (!searchString.Equals(this._assetsSearchString))
                {
                    this._assetsSearchString = searchString;
                    this._assetsTreeView.searchString = this._assetsSearchString;
                }
            }
            else if (this._assetsSearchString.Length > 0)
            {
                this._assetsSearchString = "";
                this._assetsTreeView.searchString = this._assetsSearchString;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DrawAssetTreeView()
        {
            // Use this to get the corner of the rect that the TreeView should fill.
            GUILayout.FlexibleSpace();
            Rect viewRect = GUILayoutUtility.GetLastRect();
            viewRect.width = position.width / 2.0f;

            this._assetsTreeView.OnGUI(viewRect);
        }

        private static bool IsSupportedTileset(string type)
        {
            return type == "3DTILES" || type == "TERRAIN";
        }

        private static bool IsSupportedImagery(string type)
        {
            return type == "IMAGERY";
        }

        private Vector2 _scrollPosition = Vector2.zero;

        void DrawAssetDescriptionPanel()
        {
            if (this._assetsTreeView.GetAssetsCount() == 0)
            {
                return;
            }

            int selectedId = this._assetsTreeState.lastClickedID;
            if (selectedId <= 0 || selectedId > this._assetsTreeView.GetAssetsCount())
            {
                return;
            }

            IonAssetDetails assetDetails = this._assetsTreeView.GetAssetDetails(selectedId);

            GUILayout.Space(10);
            this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition);
            GUILayout.BeginVertical();

            GUILayout.Label(assetDetails.name, CesiumEditorStyle.headerStyle);
            GUILayout.Label("(ID: " + assetDetails.id + ")");

            if (IsSupportedTileset(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(
                    "Add to Level",
                    CesiumEditorStyle.cesiumButtonStyle))
                {
                    // Asset indices are offset from the tree item IDs by 1.
                    this._assetsTreeView.AddAssetToLevel(selectedId - 1);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (IsSupportedImagery(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                // TODO: once multiple overlays are supported, we need the option to
                // add the asset as a layer.
                GUIContent addOverlayAsBaseLayerContent = new GUIContent(
                    "Use as Tileset Base Layer",
                    "Makes this asset the base overlay on the selected tileset. " +
                    "If no tileset is selected, then the overlay will be applied to the " +
                    "first found tileset." +
                    "\n\n" +
                    "If the tileset already has an overlay it will be replaced. " +
                    "If no tileset exists in the level, Cesium World Terrain is added.");
                if (GUILayout.Button(
                    addOverlayAsBaseLayerContent,
                    CesiumEditorStyle.cesiumButtonStyle))
                {
                    // Asset indices are offset from the tree item IDs by 1.
                    this._assetsTreeView.AddOverlayToTerrain(selectedId - 1);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Button(
                    "This type of asset is not currently supported",
                    CesiumEditorStyle.cesiumButtonDisabledStyle
                );
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Description", CesiumEditorStyle.subheaderStyle);
            EditorGUILayout.LabelField(assetDetails.description, EditorStyles.wordWrappedLabel);

            GUILayout.Label("Attribution", CesiumEditorStyle.subheaderStyle);
            EditorGUILayout.LabelField(assetDetails.attribution, EditorStyles.wordWrappedLabel);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
