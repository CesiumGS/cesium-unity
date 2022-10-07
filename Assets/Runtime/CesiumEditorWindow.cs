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
        private static CesiumIonSession ionSession = null!;

        [MenuItem("Cesium/Cesium")]
        public static void ShowWindow()
        {
            // If no existing window, make a new one docked next to the Hierarchy window.
            if (editorWindow == null)
            {
                Type siblingWindow = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll");
                editorWindow = GetWindow<CesiumEditorWindow>("Cesium", new Type[] { siblingWindow });
                ionSession = new CesiumIonSession();
            }

            //editorWindow.titleContent.image; // add cesium icon here
            editorWindow.Show();
            editorWindow.Focus();
        }

        void OnGUI()
        {
            DrawCesiumToolbar();
            DrawQuickAddBasicAssetsPanel();

            GUILayout.Space(10);
            DrawIonLoginPanel();
        }


        void DrawCesiumToolbar()
        {
            int selectedIndex = -1;
        CesiumToolbar.LoadToolbarIcons();
            // TODO: prevent icons from being clipped by window height...
            // maybe make it a selection grid and resize manually?
            selectedIndex = GUILayout.Toolbar(
                selectedIndex,
                CesiumToolbar.icons,
                CesiumToolbar.style,
                GUI.ToolbarButtonSize.FitToContents
            );

            if (selectedIndex < 0) {
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
                    ionSession.Disconnect();
                    break;
            }
        }

        void DrawQuickAddBasicAssetsPanel()
        {
            GUILayout.Label("Quick Add Basic Assets", EditorStyles.boldLabel);

            if (GUILayout.Button("Blank 3D Tiles Tileset"))
            {
                AddBlankTileset();
            }

            // add other options as they come up
        }

        void DrawQuickAddIonAssetsPanel()
        {
            GUILayout.Label("Quick Add Cesium ion Assets", EditorStyles.boldLabel);
        }

        void DrawIonLoginPanel()
        {

            // Cesium For Unity image here using
            // https://docs.unity3d.com/ScriptReference/GUI.DrawTexture.html
            //GUILayout.Box(image);

            EditorGUILayout.LabelField("Access global high - resolution 3D content, including photogrammetry, " +
                "terrain, imagery, and buildings. Bring your own data for tiling, hosting, and " +
                "streaming to Unity.", EditorStyles.wordWrappedLabel);
        }

        private void AddBlankTileset()
        {
            GameObject tilesetGameObject = new GameObject("Cesium3DTileset");
            tilesetGameObject.AddComponent<Cesium3DTileset>();
        }

        static class CesiumToolbar
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

            private static bool iconsLoaded = false;

            private static GUIContent MakeToolbarIcon(string label, string relativePath, string tooltip)
            {
                Texture2D icon = (Texture2D)Resources.Load(relativePath);
                icon.wrapMode = TextureWrapMode.Clamp;

                return new GUIContent(label, icon, tooltip);
            }

            public static void LoadToolbarIcons()
            {
                if (iconsLoaded)
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

                iconsLoaded = true;

                style.imagePosition = ImagePosition.ImageAbove;
                style.padding = new RectOffset(10, 10, 0, 0);
                style.margin = new RectOffset(0, 0, 5, 5);
                style.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            }
        }
    }

}