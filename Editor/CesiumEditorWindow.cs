using Reinterop;
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumEditorWindowImpl", "CesiumEditorWindowImpl.h")]
    public partial class CesiumEditorWindow : EditorWindow
    {
        public static CesiumEditorWindow currentWindow = null;

        [MenuItem("Cesium/Cesium")]
        public static void ShowWindow()
        {
            // If no existing window, make a new one docked next to the Hierarchy window.
            if (currentWindow == null)
            {
                Type siblingWindow = Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll");
                currentWindow = GetWindow<CesiumEditorWindow>(new Type[] { siblingWindow });

            }

            currentWindow.Show();
            currentWindow.Focus();
        }

        void OnEnable()
        {
            // Load the icon separately from the other resources.
            Texture2D icon = Resources.Load<Texture2D>("Cesium-64x64");
            icon.wrapMode = TextureWrapMode.Clamp;
            this.titleContent = new GUIContent("Cesium", icon);

            CesiumIonSession.Ion().Resume();
        }

        private bool _isIonConnected = false;
        private bool _isIonConnecting = false;
        private bool _isIonProfileLoaded = false;
        private bool _isIonLoadingProfile = false;

        private Vector2 _scrollPosition = Vector2.zero;

        void OnGUI()
        {
            // OnGUI is called for multiple events, including the Layout
            // and Repaint events. If the ion connection status changes
            // between these events, the UI will adapt to the change.
            // Unity will log an error because of the different layout.
            // To avoid this, we only check the connection variables when
            // OnGUI is invoked in the Layout event.
            if (Event.current.type == EventType.Layout)
            {
                CesiumIonSession ion = CesiumIonSession.Ion();
                this._isIonConnected = ion.IsConnected();
                this._isIonConnecting = ion.IsConnecting();
                this._isIonProfileLoaded = ion.IsProfileLoaded();
                this._isIonLoadingProfile = ion.IsLoadingProfile();
            }

            this.DrawCesiumToolbar();

            this._scrollPosition = EditorGUILayout.BeginScrollView(this._scrollPosition);
            this.DrawQuickAddBasicAssetsPanel();

            GUILayout.Space(10);
            if (this._isIonConnected)
            {
                this.DrawQuickAddIonAssetsPanel();
            }
            else
            {
                this.DrawIonLoginPanel();
            }

            GUILayout.Space(10);
            this.DrawConnectionStatusPanel();

            EditorGUILayout.EndScrollView();

            // Force the window to repaint if the cursor is hovered over it.
            // By default, it only repaints sporadically, so the hover and
            // selection behavior will appear delayed.
            if (EditorWindow.mouseOverWindow == this)
            {
                this.Repaint();
            }
        }

        public enum ToolbarButton
        {
            Add,
            Upload,
            Token,
            Learn,
            Help,
            SignOut,
        }

        void DrawToolbarButton(string name, ToolbarButton button, string tooltip, bool enableForIonOnly, Action func)
        {
            GUIContent content = new GUIContent(name, CesiumEditorStyle.toolbarIcons[button], tooltip);
            GUIStyle style = CesiumEditorStyle.toolbarButtonStyle;

            if (enableForIonOnly && !this._isIonConnected)
            {
                GUI.color = Color.grey;
                style = CesiumEditorStyle.toolbarButtonDisabledStyle;
            }

            if (GUILayout.Button(content, style))
            {
                if (!enableForIonOnly || this._isIonConnected)
                {
                    func();
                }
            }

            GUI.color = Color.white;
        }

        void DrawCesiumToolbar()
        {
            GUILayout.BeginHorizontal(CesiumEditorStyle.toolbarStyle);
            this.DrawToolbarButton(
                "Add",
                ToolbarButton.Add,
                "Add a tileset from Cesium ion to this scene",
                true,
                this.AddFromIon);
            this.DrawToolbarButton(
                "Upload",
                ToolbarButton.Upload,
                "Upload a tileset to Cesium ion to process it for efficient streaming to Cesium for Unity",
                true,
                this.UploadToIon);
            this.DrawToolbarButton(
                "Token",
                ToolbarButton.Token,
                "Select or create a token to use to access Cesium ion assets",
                false,
                this.SetToken);
            this.DrawToolbarButton(
                "Learn",
                ToolbarButton.Learn,
                "Open Cesium for Unity tutorials and learning resources",
                false,
                this.OpenDocumentation);
            this.DrawToolbarButton(
                "Help",
                ToolbarButton.Help,
                "Search for existing questions or ask a new question on the Cesium Community Forum",
                false,
                this.OpenSupport);
            this.DrawToolbarButton(
                "Sign Out",
                ToolbarButton.SignOut,
                "Sign out of Cesium ion",
                true,
                this.SignOutOfIon);
            GUILayout.EndHorizontal();
        }

        private enum QuickAddItemType
        {
            BlankTileset,
            DynamicCamera,
            IonTileset
        }

        private class QuickAddItem
        {
            public QuickAddItemType type;
            public string name;
            public string tooltip;
            public string tilesetName;
            public long tilesetId;
            public string overlayName;
            public long overlayId;

            public QuickAddItem(
                QuickAddItemType type,
                string name,
                string tooltip,
                string tilesetName,
                long tilesetId,
                string overlayName,
                long overlayId)
            {
                this.type = type;
                this.name = name;
                this.tooltip = tooltip;
                this.tilesetName = tilesetName;
                this.tilesetId = tilesetId;
                this.overlayName = overlayName;
                this.overlayId = overlayId;
            }
        }

        private readonly QuickAddItem[] _basicAssets = new[]
        {
            new QuickAddItem(
                QuickAddItemType.BlankTileset,
                "Blank 3D Tiles Tileset",
                "An empty tileset that can be configured to show Cesium ion assets " +
                "or tilesets from other sources.",
                "Cesium3DTileset",
                -1,
                "",
                -1),
            new QuickAddItem(
                QuickAddItemType.DynamicCamera,
                "Dynamic Camera",
                "A free camera that can be used to intuitively navigate in a " +
                "geospatial environment.",
                "",
                -1,
                "",
                -1)
        };

        private readonly QuickAddItem[] _ionAssets = new[]
        {
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Google Photorealistic 3D Tiles",
                "Photorealistic 3D Tiles from Google Maps Platform.",
                "Google Photorealistic 3D Tiles",
                2275207,
                "",
                -1),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial imagery",
                "High-resolution global terrain tileset curated from several data sources, " +
                "textured with Bing Maps satellite imagery.",
                "Cesium World Terrain",
                1,
                "Bing Maps Aerial imagery",
                2),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Aerial with Labels imagery",
                "High-resolution global terrain tileset curated from several data sources, " +
                "textured with labeled Bing Maps satellite imagery.",
                "Cesium World Terrain",
                1,
                "Bing Maps Aerial with Labels imagery",
                3),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Bing Maps Road imagery",
                "High-resolution global terrain tileset curated from several data sources, " +
                "textured with labeled Bing Maps imagery.",
                "Cesium World Terrain",
                1,
                "Bing Maps Road imagery",
                4),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium World Terrain + Sentinel-2 imagery",
                "High-resolution global terrain tileset curated from several data sources, " +
                "textured with high-resolution satellite imagery from the Sentinel-2 project.",
                "Cesium World Terrain",
                1,
                "Sentinel-2 imagery",
                3954),
            new QuickAddItem(
                QuickAddItemType.IonTileset,
                "Cesium OSM Buildings",
                "A 3D buildings layer derived from OpenStreetMap covering the entire world.",
                "Cesium OSM Buildings",
                96188,
                "",
                -1)
        };

        void DrawQuickAddBasicAssetsPanel()
        {
            GUILayout.Label("Quick Add Basic Assets", CesiumEditorStyle.subheaderStyle);
            GUIContent addButtonContent = new GUIContent(CesiumEditorStyle.quickAddIcon, "Add this item to the level");

            for (int i = 0; i < this._basicAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(
                    new GUIContent(
                        this._basicAssets[i].name,
                        this._basicAssets[i].tooltip),
                    EditorStyles.wordWrappedLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    this.QuickAddAsset(this._basicAssets[i]);
                }
                GUILayout.EndHorizontal();
            }

            // add other options as they come up
        }
        private void QuickAddAsset(QuickAddItem item)
        {
            switch (item.type)
            {
                case QuickAddItemType.IonTileset:
                    AddAssetFromIon(
                        item.name,
                        item.tilesetName,
                        item.tilesetId,
                        item.overlayName,
                        item.overlayId);
                    break;
                case QuickAddItemType.BlankTileset:
                    Cesium3DTileset blankTileset =
                        CesiumEditorUtility.CreateTileset(item.tilesetName, 0);
                    Selection.activeGameObject = blankTileset.gameObject;
                    break;
                case QuickAddItemType.DynamicCamera:
                    CesiumCameraController dynamicCamera
                        = CesiumEditorUtility.CreateDynamicCamera();
                    Selection.activeGameObject = dynamicCamera.gameObject;
                    break;
                default:
                    break;
            }

            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
        }

        private partial void AddAssetFromIon(
            string name,
            string tilesetName,
            long tilesetID,
            string overlayName,
            long overlayID);

        void DrawQuickAddIonAssetsPanel()
        {
            GUILayout.Label("Quick Add Cesium ion Assets", CesiumEditorStyle.subheaderStyle);
            GUIContent addButtonContent = new GUIContent(
                CesiumEditorStyle.quickAddIcon,
                "Add this item to the level");

            for (int i = 0; i < this._ionAssets.Length; i++)
            {
                GUILayout.BeginHorizontal(CesiumEditorStyle.quickAddItemStyle);
                GUILayout.Box(new GUIContent(
                    this._ionAssets[i].name,
                    this._ionAssets[i].tooltip),
                    EditorStyles.wordWrappedLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(addButtonContent, CesiumEditorStyle.quickAddButtonStyle))
                {
                    this.QuickAddAsset(this._ionAssets[i]);
                }
                GUILayout.EndHorizontal();
            }
        }


        void DrawIonLoginPanel()
        {
            Texture2D logo = EditorGUIUtility.isProSkin ? 
                CesiumEditorStyle.cesiumForUnityLogoLight : 
                CesiumEditorStyle.cesiumForUnityLogoDark;
            GUIContent logoContent = new GUIContent(logo);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                logoContent,
                GUILayout.Height(175));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Access global high - resolution 3D content, " +
                "including photogrammetry, terrain, imagery, and buildings. " +
                "Bring your own data for tiling, hosting, and streaming to Unity.",
                EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Connect to Cesium ion", CesiumEditorStyle.cesiumButtonStyle))
            {
                CesiumIonSession.Ion().Connect();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DrawConnectionStatusPanel()
        {
            if (this._isIonConnecting)
            {
                EditorGUILayout.LabelField(
                    "Waiting for you to sign into Cesium ion with your web browser...",
                    EditorStyles.wordWrappedLabel);

                string authorizeUrl = CesiumIonSession.Ion().GetAuthorizeUrl();
                if (EditorGUILayout.LinkButton("Open web browser again"))
                {
                    Application.OpenURL(authorizeUrl);
                }
                GUILayout.Space(5);
                EditorGUILayout.LabelField(
                    "Or copy the URL below into your web browser",
                    EditorStyles.wordWrappedLabel);

                GUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel(
                    authorizeUrl,
                    EditorStyles.textField,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("Copy To Clipboard"))
                {
                    CopyLinkToClipboard(authorizeUrl);
                }
                GUILayout.EndHorizontal();
            }
            else if (this._isIonProfileLoaded)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                string username = CesiumIonSession.Ion().GetProfileUsername();
                if (EditorGUILayout.LinkButton(
                    new GUIContent(
                        "Connected to Cesium ion as " + username,
                        "Open your Cesium ion account in your browser")))
                {
                    this.VisitIon();
                }
                GUILayout.EndHorizontal();
            }
            else if (this._isIonLoadingProfile)
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Loading user information...");
                GUILayout.EndHorizontal();
            }
            else if (this._isIonConnected)
            {
                CesiumIonSession.Ion().RefreshProfile();
            }

            GUILayout.Space(5);
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
            SelectIonTokenWindow.ShowWindow();
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
            CesiumIonSession.Ion().Disconnect();
        }

        void CopyLinkToClipboard(string link)
        {
            EditorGUIUtility.systemCopyBuffer = link;
        }

        void VisitIon()
        {
            Application.OpenURL("https://cesium.com/ion");
        }
    }

}
