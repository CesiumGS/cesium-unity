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
                // Get existing open window or if none, make a new one docked next to Console window.
                Type siblingWindow = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
                currentWindow = GetWindow<CesiumIonAssetsWindow>("Cesium ion Assets", new Type[] { siblingWindow });
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
            if (CesiumIonSession.currentSession == null)
            {
                CesiumIonSession.currentSession = new CesiumIonSession();
            }

            CesiumIonSession.currentSession.Resume();
            BuildTreeView();
        }

        CesiumIonSession Ion()
        {
            return CesiumIonSession.currentSession;
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
            GUILayout.FlexibleSpace();
            Rect viewRect = GUILayoutUtility.GetLastRect();
            viewRect.width = position.width / 2.0f;
            viewRect.height = position.height;

            GUILayout.Label("This is the tree view");
            _assetsTreeView.OnGUI(viewRect);
        }
    }
}
