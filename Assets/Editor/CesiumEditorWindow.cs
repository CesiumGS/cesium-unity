using System;
using UnityEngine;
using UnityEditor;

namespace CesiumForUnity
{
    //TODO: it would be nice to initialize this window on editor load
    //[InitializeOnLoad]
    public class CesiumEditorWindow : EditorWindow
    {
        private static CesiumEditorWindow editorWindow = null!;
        private static Texture2D cesiumIcon = null!;

        [MenuItem("Cesium/Cesium")]
        public static void ShowWindow()
        {
            // If no existing window, make a new one docked next to the Hierarchy window.
            if (editorWindow == null)
            {
                Type siblingWindow = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll");
                editorWindow = GetWindow<CesiumEditorWindow>("Cesium", new Type[] { siblingWindow });
            }

            if (cesiumIcon == null)
            {
                cesiumIcon = (Texture2D)Resources.Load("Cesium-icon-16x16");
            }

            editorWindow.titleContent.image = cesiumIcon;

            editorWindow.Show();
            editorWindow.Focus();
        }

        void Awake()
        {
            if (CesiumIonSession.currentSession == null)
            {
                CesiumIonSession.currentSession = new CesiumIonSession();
            }

            CesiumToolbar.LoadResources();
            CesiumIonLoginPanel.LoadResources();
            CesiumQuickAddPanel.LoadResources();
        }

        void OnGUI()
        {
            DrawCesiumToolbar();
            DrawQuickAddBasicAssetsPanel();

            GUILayout.Space(10);
            if (CesiumIonSession.currentSession.IsConnected())
            {
                DrawQuickAddIonAssetsPanel();
            }
            else
            {
                DrawIonLoginPanel();
            }


            // Force the window to repaint if the cursor is hovered over it.
            // (By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.)
            if (mouseOverWindow == editorWindow)
            {
                Repaint();
            }
        }

        private static class CesiumToolbar
        {
            public enum Index
            {
                Add = 0,
                Upload = 1,
                Token = 2,
                Learn = 3,
                Help = 4,
                SignOut = 5,
            }

            public static readonly GUIStyle style = new GUIStyle();
            public static readonly GUIContent[] icons = new GUIContent[Enum.GetNames(typeof(Index)).Length];

            private static bool resourcesLoaded = false;

            private static GUIContent MakeToolbarIcon(string label, string relativePath, string tooltip)
            {
                Texture2D icon = (Texture2D)Resources.Load(relativePath);
                icon.wrapMode = TextureWrapMode.Clamp;

                return new GUIContent(label, icon, tooltip);
            }

            public static void LoadResources()
            {
                if (resourcesLoaded)
                {
                    return;
                }

                icons[(int)Index.Add] = MakeToolbarIcon(
                    "Add",
                    "FontAwesome/plus-solid",
                    "Add a tileset from Cesium ion to this scene"
                );
                icons[(int)Index.Upload] = MakeToolbarIcon(
                    "Upload",
                    "FontAwesome/cloud-upload-alt-solid",
                    "Upload a tileset to Cesium ion to process it for efficient streaming to Cesium for Unity"
                );
                icons[(int)Index.Token] = MakeToolbarIcon(
                    "Token",
                    "FontAwesome/key-solid",
                    "Select or create a token to use to access Cesium ion assets"
                );
                icons[(int)Index.Learn] = MakeToolbarIcon(
                    "Learn",
                    "FontAwesome/book-reader-solid",
                    "Open Cesium for Unity tutorials and learning resources"
                );
                icons[(int)Index.Help] = MakeToolbarIcon(
                    "Help",
                    "FontAwesome/hands-helping-solid",
                    "Search for existing questions or ask a new question on the Cesium Community Forum"
                );
                icons[(int)Index.SignOut] = MakeToolbarIcon(
                    "Sign Out",
                    "FontAwesome/sign-out-alt-solid",
                    "Sign out of Cesium ion"
                );

                resourcesLoaded = true;

                style.imagePosition = ImagePosition.ImageAbove;
                style.padding = new RectOffset(5, 5, 5, 5);
                style.margin = new RectOffset(5, 5, 5, 5);
                style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

                style.hover.background = Texture2D.grayTexture;
                style.hover.textColor = new Color(0.9f, 0.9f, 0.9f);
            }
        }

        void DrawCesiumToolbar()
        {
            // TODO: prevent icons from being clipped by window height...
            // maybe make it a selection grid and resize manually?
            // TODO: add hover tints
            int selectedIndex = -1;

            selectedIndex = GUILayout.Toolbar(
                selectedIndex,
                CesiumToolbar.icons,
                CesiumToolbar.style,
                GUI.ToolbarButtonSize.FitToContents
            );

            if (selectedIndex < 0)
            {
                return;
            }

            CesiumToolbar.Index toolbarIndex = (CesiumToolbar.Index)selectedIndex;
            switch (toolbarIndex)
            {
                case CesiumToolbar.Index.Add:
                    break;
                case CesiumToolbar.Index.Upload:
                    Application.OpenURL("https://cesium.com/ion/addasset");
                    break;
                case CesiumToolbar.Index.Token:
                    break;
                case CesiumToolbar.Index.Learn:
                    Application.OpenURL("https://cesium.com/docs");
                    break;
                case CesiumToolbar.Index.Help:
                    Application.OpenURL("https://community.cesium.com/");
                    break;
                case CesiumToolbar.Index.SignOut:
                    CesiumIonSession.currentSession.Disconnect();
                    break;
            }
        }

        void DrawQuickAddBasicAssetsPanel()
        {
            GUILayout.Label("Quick Add Basic Assets", EditorStyles.boldLabel);

            // TODO: maybe replace with a selection grid (to simulate a vertical toolbar),
            // especially if we're moving towards a design where the UI mimics Unreal's.
            // e.g. Blank 3D Tiles Tileset [+] where [+] is an image
            if (GUILayout.Button("Blank 3D Tiles Tileset"))
            {
                AddBlankTileset();
            }

            // add other options as they come up
        }
        private void AddBlankTileset()
        {
            GameObject tilesetGameObject = new GameObject("Cesium3DTileset");
            tilesetGameObject.AddComponent<Cesium3DTileset>();
        }

        static class CesiumIonLoginPanel
        {
            public static readonly GUIStyle style = new GUIStyle();
            public static readonly Color buttonColor = new Color(0.07059f, 0.35686f, 0.59216f, 1.0f);

            /*const FLinearColor CesiumButtonLighter(0.16863f, 0.52941f, 0.76863f, 1.0f);
            const FLinearColor CesiumButtonDarker(0.05490f, 0.29412f, 0.45882f, 1.0f);;*/

            public static Texture2D cesiumForUnityLogo = null!;

            public static bool resourcesLoaded = false;

            public static void LoadResources()
            {
                if (resourcesLoaded)
                {
                    return;
                }

                cesiumForUnityLogo = (Texture2D)Resources.Load("Cesium-for-Unity-Logo");
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
                CesiumIonSession.currentSession.Connect();
            }
        }


        void DrawQuickAddIonAssetsPanel()
        {
            GUILayout.Label("Quick Add Cesium ion Assets", EditorStyles.boldLabel);

            CesiumQuickAddItem[] ionAssets = CesiumQuickAddPanel.ionAssets;
            for (int i = 0; i < ionAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumQuickAddPanel.assetItemStyle);
                GUILayout.Box(new GUIContent(ionAssets[i].name, ionAssets[i].tooltip), CesiumQuickAddPanel.assetItemLabelStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(CesiumQuickAddPanel.addButton, CesiumQuickAddPanel.addButtonStyle))
                {
                    // TODO: some function here
                }
                GUILayout.EndHorizontal();
            }
        }

        private class CesiumQuickAddItem
        {
            public string name;
            public string tooltip;
            public int tilesetId;
            public int overlayId;

            public CesiumQuickAddItem(string name, string tooltip, int tilesetId, int overlayId)
            {
                this.name = name;
                this.tooltip = tooltip;
                this.tilesetId = tilesetId;
                this.overlayId = overlayId;
            }
        }

        static class CesiumQuickAddPanel
        {
            private static bool resourcesLoaded = false;

            public static readonly CesiumQuickAddItem[] ionAssets = new CesiumQuickAddItem[5];
            public static readonly GUIStyle assetItemStyle = new GUIStyle();
            public static readonly GUIStyle assetItemLabelStyle = new GUIStyle();

            public static GUIContent addButton = null!;
            public static readonly GUIStyle addButtonStyle = new GUIStyle();

            public static void LoadResources()
            {
                if (resourcesLoaded)
                {
                    return;
                }

                ionAssets[0] = new CesiumQuickAddItem(
                    "Cesium World Terrain + Bing Maps Aerial imagery",
                    "High-resolution global terrain tileset curated from several data sources, textured with Bing Maps satellite imagery.",
                    1,
                    2
                );
                ionAssets[1] = new CesiumQuickAddItem(
                    "Cesium World Terrain + Bing Maps Aerial with Labels imagery",
                    "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps satellite imagery.",
                    1,
                    3
                );
                ionAssets[2] = new CesiumQuickAddItem(
                    "Cesium World Terrain + Bing Maps Road imagery",
                    "High-resolution global terrain tileset curated from several data sources, textured with labeled Bing Maps imagery.",
                    1,
                    4
                );
                ionAssets[3] = new CesiumQuickAddItem(
                    "Cesium World Terrain + Sentinel-2 imagery",
                    "High-resolution global terrain tileset curated from several data sources, textured with high-resolution satellite imagery from the Sentinel-2 project.",
                    1,
                    3954
                );
                ionAssets[4] = new CesiumQuickAddItem(
                     "Cesium OSM Buildings",
                     "A 3D buildings layer derived from OpenStreetMap covering the entire world.",
                     96188,
                     -1
                 );

                assetItemStyle.margin = new RectOffset(5, 5, 10, 10);

                assetItemLabelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
                assetItemLabelStyle.wordWrap = true;

                Texture2D addIcon = (Texture2D)Resources.Load("FontAwesome/plus-solid");
                addButton = new GUIContent(addIcon, "Add this item to the level");
                addButtonStyle.fixedWidth = 16.0f;
                addButtonStyle.fixedHeight = 16.0f;
                addButtonStyle.hover.background = Texture2D.grayTexture;

                resourcesLoaded = true;
            }
        }


    }

}