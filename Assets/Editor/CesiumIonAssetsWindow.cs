using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace CesiumForUnity
{
    public class CesiumIonAssetsWindow : EditorWindow
    {
        public static CesiumIonAssetsWindow currentWindow = null!;

        [MenuItem("Cesium/Cesium ion Assets")]
        public static void ShowWindow()
        {
            CesiumEditorStyle.Reload();

            if (currentWindow == null)
            {
                // Get existing open window or if none, make a new one docked next to the Project / Console window.
                Type[] siblingWindows = new Type[] {
                    Type.GetType("UnityEditor.ProjectBrowser,UnityEditor.dll"),
                    Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll")
                };
                currentWindow = GetWindow<CesiumIonAssetsWindow>("Cesium ion Assets", siblingWindows);
                currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        private TreeViewState _assetsTreeState;
        private IonAssetsTreeView _assetsTreeView;
        private SearchField _searchField;

        private void OnEnable()
        {
            CesiumIonSession.Ion().Resume();
            CesiumEditorStyle.Reload();
            BuildTreeView();
            _searchField = new SearchField();
            CesiumIonSession.OnConnectionUpdated += _assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated += _assetsTreeView.Refresh;
        }

        private void OnDisable()
        {
            CesiumIonSession.OnConnectionUpdated -= _assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated -= _assetsTreeView.Refresh;
        }

        void BuildTreeView()
        {
            _assetsTreeState = new TreeViewState();
            _assetsTreeView = new IonAssetsTreeView(_assetsTreeState);

            _assetsTreeView.Refresh();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            DrawAssetListPanel();
            DrawAssetDescriptionPanel();
            GUILayout.EndHorizontal();

            // Force the window to repaint if the cursor is hovered over it.
            // By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.
            if (mouseOverWindow == currentWindow)
            {
                Repaint();
            }
        }
        void Update()
        {
            CesiumIonSession.Ion().Tick();
        }


        void DrawAssetListPanel()
        {
            GUILayout.BeginVertical(GUILayout.Width(position.width / 2));
            DrawRefreshButtonAndSearchBar();
            GUILayout.Space(15);
            DrawAssetTreeView();
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
                _assetsTreeView.Refresh();
            }

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.Space(15);

            string searchString = _searchField.OnToolbarGUI(
                _assetsSearchString,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            if (searchString != null)
            {
                searchString = searchString.Trim();
                if (!searchString.Equals(_assetsSearchString))
                {
                    _assetsSearchString = searchString;
                    _assetsTreeView.searchString = _assetsSearchString;
                }
            } else if (_assetsSearchString.Length > 0)
            {
                _assetsSearchString = "";
                _assetsTreeView.searchString = _assetsSearchString;
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private Vector2 _scrollPosition;

        void DrawAssetTreeView()
        {
            // Use this to get the corner of the rect that the TreeView should fill.
            GUILayout.FlexibleSpace();
            Rect viewRect = GUILayoutUtility.GetLastRect();
            viewRect.width = position.width / 2.0f;

            _assetsTreeView.OnGUI(viewRect);
        }

        private static bool IsSupportedTileset(string type)
        {
            return type == "3DTILES" || type == "TERRAIN";
        }

        private static bool IsSupportedImagery(string type)
        {
            return type == "IMAGERY";
        }

        private Vector2 scrollPosition = Vector2.zero;
        
        void DrawAssetDescriptionPanel()
        {
            if (_assetsTreeView.GetAssetsCount() == 0) {
                return;
            }

            int selectedId = _assetsTreeState.lastClickedID;
            if(selectedId <= 0)
            {
                return;
            }

            IonAssetDetails assetDetails = _assetsTreeView.GetAssetDetails(selectedId);

            GUILayout.Space(10);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();

            GUILayout.Label(assetDetails.name, CesiumEditorStyle.descriptionHeaderStyle);
            GUILayout.Label("(ID: " + assetDetails.id + ")");

            if (IsSupportedTileset(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Button(
                    "Add to Level",
                    CesiumEditorStyle.cesiumButtonStyle
                );
                GUILayout.EndHorizontal();
            }
            else if (IsSupportedImagery(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Button(
                    "Use as Terrain Tileset Base Layer",
                    CesiumEditorStyle.cesiumButtonStyle
                );
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(
                    "This type of asset is not currently supported",
                    CesiumEditorStyle.descriptionCenterTextStyle
                );
            }

            GUILayout.Label("Description", CesiumEditorStyle.descriptionSubheaderStyle);
            EditorGUILayout.LabelField(assetDetails.description, EditorStyles.wordWrappedLabel);

            GUILayout.Label("Attribution", CesiumEditorStyle.descriptionSubheaderStyle);
            EditorGUILayout.LabelField(assetDetails.attribution, EditorStyles.wordWrappedLabel);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
    }
}
