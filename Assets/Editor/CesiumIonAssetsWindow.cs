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
            GUILayout.BeginVertical();
            DrawRefreshButtonAndSearchBar();
            GUILayout.Space(15);
            DrawAssetTreeView();
            GUILayout.EndVertical();
        }

        void DrawAssetDescriptionPanel()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("I'm going to be where the description goes");
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
    }
}
