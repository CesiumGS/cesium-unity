using System;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    //TODO: it would be nice to initialize this window on editor load
    //[InitializeOnLoad]
    public class CesiumEditorWindow : EditorWindow
    {
        public static CesiumEditorWindow currentWindow = null!;

        [MenuItem("Cesium/Cesium")]
        public static void ShowWindow()
        {
            CesiumEditorStyle.Reload();

            // If no existing window, make a new one docked next to the Hierarchy window.
            if (currentWindow == null)
            {
                Type siblingWindow = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll");
                currentWindow = GetWindow<CesiumEditorWindow>("Cesium", new Type[] { siblingWindow });
                currentWindow.titleContent.image = CesiumEditorStyle.cesiumIcon;
            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        void Awake()
        {
            if (CesiumIonSession.currentSession == null)
            {
                CesiumIonSession.currentSession = new CesiumIonSession();
            }

            PopulateQuickAddLists();
        }

        CesiumIonSession Ion()
        {
            return CesiumIonSession.currentSession;
        }

        void OnGUI()
        {
            DrawCesiumToolbar();
            DrawQuickAddBasicAssetsPanel();

            GUILayout.Space(10);
            if (Ion().IsConnected())
            {
                DrawQuickAddIonAssetsPanel();
            }
            else
            {
                DrawIonLoginPanel();
            }

            GUILayout.Space(10);
            DrawConnectionStatusPanel();

            // Force the window to repaint if the cursor is hovered over it.
            // (By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.)
            if (mouseOverWindow == currentWindow)
            {
                Repaint();
            }
        }

        private void Update()
        {
            CesiumIonSession session = Ion();
            if (session != null)
                session.Tick();
        }

        public enum ToolbarIndex
        {
            Add = 0,
            Upload = 1,
            Token = 2,
            Learn = 3,
            Help = 4,
            SignOut = 5,
        }

        void DrawToolbarButton(string name, int iconIndex, string tooltip, bool enableForIonOnly, Action func)
        {
            GUIContent content = new GUIContent(name, CesiumEditorStyle.toolbarIcons[iconIndex], tooltip);
            GUIStyle style = CesiumEditorStyle.toolbarButtonStyle;
            bool isConnectedToIon = Ion().IsConnected();

            if (enableForIonOnly && !isConnectedToIon)
            {
                GUI.color = Color.grey;
                style = CesiumEditorStyle.toolbarButtonDisabledStyle;
            }

            if (GUILayout.Button(content, style))
            {
                if (!enableForIonOnly || isConnectedToIon)
                {
                    func();
                }
            }

            GUI.color = Color.white;
        }

        void DrawCesiumToolbar()
        {
            // TODO: prevent icons from being clipped by window...
            // maybe resize manually?

            GUILayout.BeginHorizontal(CesiumEditorStyle.toolbarStyle);
            DrawToolbarButton(
                "Add",
                (int)ToolbarIndex.Add,
                "Add a tileset from Cesium ion to this scene",
                true,
                AddFromIon
            );
            DrawToolbarButton(
                "Upload",
                (int)ToolbarIndex.Upload,
                "Upload a tileset to Cesium ion to process it for efficient streaming to Cesium for Unity",
                true,
                UploadToIon
            );
            DrawToolbarButton(
                "Token",
                (int)ToolbarIndex.Token,
                "Select or create a token to use to access Cesium ion assets",
                false,
                SetToken
            );
            DrawToolbarButton(
                "Learn",
                (int)ToolbarIndex.Learn,
                "Open Cesium for Unity tutorials and learning resources",
                false,
                OpenDocumentation
            );
            DrawToolbarButton(
                "Help",
                (int)ToolbarIndex.Help,
                "Search for existing questions or ask a new question on the Cesium Community Forum",
                false,
                OpenSupport
            );
            DrawToolbarButton(
                "Sign Out",
                (int)ToolbarIndex.SignOut,
                "Sign out of Cesium ion",
                true,
                SignOutOfIon
            );
            GUILayout.EndHorizontal();
        }

        private enum QuickAddItemType
        {
            BlankTileset,
            IonTileset
        }
        
        private class QuickAddItem
        {
            public QuickAddItemType type;
            public string name;
            public string tooltip;
            public int tilesetId;
            public int overlayId;

            public QuickAddItem(QuickAddItemType type, string name, string tooltip, int tilesetId, int overlayId)
            {
                this.type = type;
                this.name = name;
                this.tooltip = tooltip;
                this.tilesetId = tilesetId;
                this.overlayId = overlayId;
            }
        }

        private readonly QuickAddItem[] basicAssets = new QuickAddItem[1];
        private readonly QuickAddItem[] ionAssets = new QuickAddItem[5];

        void PopulateQuickAddLists()
        {
            basicAssets[0] = new QuickAddItem(
                QuickAddItemType.BlankTileset,
                "Blank 3D Tiles Tileset",
                "An empty tileset that can be configured to show Cesium ion assets or tilesets from other sources.",
                -1,
                -1
            );

            ionAssets[0] = new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with Bing Maps satellite imagery.",
                1,
                2
            );
            ionAssets[1] = new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial with Labels imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps satellite imagery.",
                1,
                3
            );
            ionAssets[2] = new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Road imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps imagery.",
                1,
                4
            );
            ionAssets[3] = new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Sentinel-2 imagery",
                "High-resolution global terrain tileset curated from several data sources, textured with high-resolution satellite imagery from the Sentinel-2 project.",
                1,
                3954
            );
            ionAssets[4] = new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium OSM Buildings",
                "A 3D buildings layer derived from OpenStreetMap covering the entire world.",
                96188,
                -1
            );
        }


        void DrawQuickAddBasicAssetsPanel()
        {
            GUILayout.Label("Quick Add Basic Assets", EditorStyles.boldLabel);
            GUIContent addButtonContent = new GUIContent(CesiumEditorStyle.quickAddIcon, "Add this item to the level");

            for (int i = 0; i < basicAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(new GUIContent(basicAssets[i].name, basicAssets[i].tooltip), CesiumEditorStyle.quickAddItemLabelStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    AddBasicAsset(basicAssets[i]);
                }
                GUILayout.EndHorizontal();
            }

            // add other options as they come up
        }
        private void AddBasicAsset(QuickAddItem item)
        {
            GameObject addedObject = new GameObject("Cesium3DTileset");
            QuickAddItemType type = item.type;

            switch (type) {
                case QuickAddItemType.BlankTileset:
                    addedObject.AddComponent<Cesium3DTileset>();
                    break;
                default:
                    break;
            }
        }

        void DrawQuickAddIonAssetsPanel()
        {
            GUILayout.Label("Quick Add Cesium ion Assets", EditorStyles.boldLabel);
            GUIContent addButtonContent = new GUIContent(CesiumEditorStyle.quickAddIcon, "Add this item to the level");

            for (int i = 0; i < ionAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(new GUIContent(ionAssets[i].name, ionAssets[i].tooltip), CesiumEditorStyle.quickAddItemLabelStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    // TODO: some function here
                }
                GUILayout.EndHorizontal();
            }
        }

        void DrawIonLoginPanel()
        {
            //GUILayout.Box(CesiumIonLoginPanel.cesiumForUnityLogo, GUILayout.Width(500.0f));

            EditorGUILayout.LabelField("Access global high - resolution 3D content, including photogrammetry, " +
                "terrain, imagery, and buildings. Bring your own data for tiling, hosting, and " +
                "streaming to Unity.", EditorStyles.wordWrappedLabel);

            if (GUILayout.Button("Connect to Cesium ion"))
            {
                Ion().Connect();
            }
        }

        void DrawConnectionStatusPanel()
        {
            if (Ion().IsConnecting()) {
                EditorGUILayout.LabelField(
                    "Waiting for you to sign into Cesium ion with your web browser...",
                    EditorStyles.wordWrappedLabel
                );

                string authorizeUrl = Ion().GetAuthorizeUrl();
                if (EditorGUILayout.LinkButton("Open web browser again")) {
                    Application.OpenURL(authorizeUrl);
                }

                EditorGUILayout.LabelField(
                    "Or copy the URL below into your web browser",
                    EditorStyles.wordWrappedLabel
                );

                GUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(
                    authorizeUrl,
                    EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)
                );

                if (GUILayout.Button("Copy To Clipboard")) {
                    GUIUtility.systemCopyBuffer = authorizeUrl;
                }
                GUILayout.EndHorizontal();

            } else if (Ion().IsProfileLoaded())
            {
                string username = Ion().GetProfileUsername();
                if (GUILayout.Button("Connected to Cesium ion as " + username, "Open your Cesium ion account in your browser")) {
                    VisitIon();
                }
            } else if (Ion().IsLoadingProfile()) {
                GUILayout.Label("Loading user information...");
            }
        }

        void AddFromIon()
        {
            CesiumIonAssetsWindow.ShowWindow();
        }

        void UploadToIon()
        {
            Application.OpenURL("https://cesium.com/ion/addasset");
        }

        void SetToken()
        {

        }

        void OpenDocumentation()
        {
            Application.OpenURL("https://cesium.com/docs");
        }

        void OpenSupport()
        {
            Application.OpenURL("https://community.cesium.com/");
        }

        void SignOutOfIon()
        {
            Ion().Disconnect();
        }
        void VisitIon()
        {
            Application.OpenURL("https://cesium.com/ion");
        }
    }

}