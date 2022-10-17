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

        private MultiColumnHeaderState _assetsHeaderState;
        private MultiColumnHeader _assetsHeader;
        private TreeViewState _assetsTreeState;
        private IonAssetsTreeView _assetsTreeView;

        void Awake()
        {
            BuildTreeView();

            CesiumIonSession.OnConnectionUpdated += _assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated += _assetsTreeView.Refresh;

            CesiumIonSession.Ion().Resume();
        }

        private void OnDestroy()
        {
            CesiumIonSession.OnConnectionUpdated -= _assetsTreeView.Refresh;
            CesiumIonSession.OnAssetsUpdated -= _assetsTreeView.Refresh;
        }

        void BuildTreeView()
        {
            string[] columnNames = new string[Enum.GetNames(typeof(IonAssetsColumn)).Length];
            columnNames[(int)IonAssetsColumn.Name] = "Name";
            columnNames[(int)IonAssetsColumn.Type] = "Type";
            columnNames[(int)IonAssetsColumn.DateAdded] = "Date added";

            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[columnNames.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new MultiColumnHeaderState.Column()
                {
                    allowToggleVisibility = false,
                    autoResize = true,
                    minWidth = 135.0f,
                    canSort = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    headerContent = new GUIContent(columnNames[i]),
                    headerTextAlignment = TextAlignment.Left
                };
            }

            _assetsHeaderState = new MultiColumnHeaderState(columns);
            _assetsHeader = new MultiColumnHeader(_assetsHeaderState);

            _assetsTreeState = new TreeViewState();
            _assetsTreeView = new IonAssetsTreeView(_assetsTreeState, _assetsHeader);

            _assetsTreeView.Refresh();
            _assetsHeader.ResizeToFit();
        }

        void OnGUI()
        {
            GUILayout.Space(10);
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
            // TODO: search bar here
            // https://docs.unity3d.com/ScriptReference/IMGUI.Controls.SearchField.html ?

            GUILayout.EndHorizontal();
        }

        private Vector2 _scrollPosition;

        void DrawAssetTreeView()
        {
            // Use this to get the corner of the rect that
            // the TreeView should fill.
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
        private int selectedId = -1;
        
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

            GUILayout.BeginVertical();
            GUILayout.Label(assetDetails.name, CesiumEditorStyle.descriptionHeaderStyle);
            GUILayout.Label("(ID: " + assetDetails.id + ")");

            if (IsSupportedTileset(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Button(
                    "Add to Level",
                    CesiumEditorStyle.cesiumButtonStyle,
                    GUILayout.Width(200)
                );
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (IsSupportedImagery(assetDetails.type))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Button(
                    "Use as Terrain Tileset Base Layer",
                    CesiumEditorStyle.cesiumButtonStyle,
                    GUILayout.Width(300)
                );
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label(
                    "This type of asset is not currently supported",
                    CesiumEditorStyle.descriptionCenterTextStyle
                );
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("Description", CesiumEditorStyle.descriptionSubheaderStyle);
            EditorGUILayout.LabelField(assetDetails.description, EditorStyles.wordWrappedLabel);

            GUILayout.Label("Attribution", CesiumEditorStyle.descriptionSubheaderStyle);
            EditorGUILayout.LabelField(assetDetails.attribution, EditorStyles.wordWrappedLabel);
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
